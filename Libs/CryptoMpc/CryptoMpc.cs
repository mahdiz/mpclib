using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using MpcLib.Common;
using MpcLib.Common.FiniteField;
using MpcLib.Common.FiniteField.Circuits;
using MpcLib.Common.StochasticUtils;
using MpcLib.DistributedSystem;
using MpcLib.DistributedSystem.QuorumBuilding;
using MpcLib.SecretSharing;
using MpcLib.SecretSharing.eVSS;

namespace MpcLib.MpcProtocols.Crypto
{
	public class Quorum
	{
		public int[] PartyIds;
		public CryptoMpc Protocol;
		protected int[] Dealers;

		public eVSS MainVSS;
		public eVSS ReshareVSS;
		public BigZp[,] Triples;

		public BigZp InputShare1, InputShare2;

		static int tempSync;	////////// TEMP FOR SYNCHRONIZATION

		public int this[int partyId]
		{
			get { return PartyIds[partyId]; }
		}

		public Quorum(CryptoMpc p, int[] partyIds, int[] dealers, int numTriples)
		{
			Debug.Assert(partyIds.Contains(p.Party.Id) || dealers.Contains(p.Party.Id), "Party not involved with this quorum!");

			Protocol = p;
			var prime = p.Input.Prime;
			var polyDegree = (int)Math.Ceiling(partyIds.Length / 3.0);
			Dealers = dealers;

			MainVSS = new eVSS(p.Party, partyIds, prime, polyDegree);
			MainVSS.Setup(p.Seed);

			// initialize VSS instances
			if (partyIds.Contains(p.Party.Id))
			{
				ReshareVSS = new eVSS(p.Party, partyIds, prime, polyDegree - 1);
				ReshareVSS.Setup(p.Seed);

				var tripleVSS = new eVSS(p.Party, partyIds, prime, polyDegree / 2);
				tripleVSS.Setup(p.Seed);

				// generate random triples
				Triples = GenTriples(tripleVSS, numTriples, prime);
				tempSync++;
			}

			while (tempSync < numTriples) 
				Thread.Sleep(10);		///////// TEMP FOR SYNCHRONIZATION
		}

		public bool Contains(int partyId)
		{
			return PartyIds.Contains(partyId);
		}

		public void InputCommitment()
		{
			if (Dealers.Contains(Protocol.Party.Id))
			{
				// I am a dealer. Secret-share my input in the quorum.
				MainVSS.ShareVerify(Protocol.Input, false);
				Console.WriteLine("P" + Protocol.Party.Id + ": shared his input: " + Protocol.Input);
			}
			else
			{
				// I am not a dealer. Verify dealer shares.
				// verify input 1
				var commit = Protocol.Receive<CommitMsg>();
				var shareMsg = Protocol.Receive<SecretSharing.eVSS.ShareWitnessMsg<BigZp>>();

				MainVSS.Verify(commit, shareMsg);
				InputShare1 = shareMsg.Share;

				// verify input 2
				commit = Protocol.Receive<CommitMsg>();
				shareMsg = Protocol.Receive<SecretSharing.eVSS.ShareWitnessMsg<BigZp>>();

				MainVSS.Verify(commit, shareMsg);
				InputShare2 = shareMsg.Share;

				// CHECK WHY EVSS SHAREVERIFY PUTS POLYDEG - 1 FOR SHAMIR SHARING.
			}
		}

		public BigZp Multiply(BigZp ai, BigZp bi)
		{
			var ui = Triples[0, 0];		// u_i
			var vi = Triples[0, 1];		// v_i
			var wi = Triples[0, 2];		// w_i
			var ei = ai + ui;			// epsilon_i
			var di = bi + vi;			// delta_i

			// reveal epsilon and delta
			var e = MainVSS.Reconst(ei);
			var d = MainVSS.Reconst(di);

			return wi + (d * ai) + (e * bi) - (e * d);	// gamma_i
		}

		public BigZp Reconst(BigZp ui)
		{
			return MainVSS.Reconst(ui);
		}

		public static BigZp[,] GenTriples(eVSS evss, int numTriples, BigInteger prime)
		{
			var t = new BigZp[numTriples, 3];
			for (int j = 0; j < numTriples; j++)
			{
				t[j, 0] = GenRand(evss, prime);
				t[j, 1] = GenRand(evss, prime);
				t[j, 2] = t[j, 0] * t[j, 1];
			}
			return t;
		}

		public static BigZp GenRand(eVSS evss, BigInteger prime)
		{
			var shares = evss.ShareVerify(new BigZp(prime, StaticRandom.Next(prime - 1)), true);

			var sumShares = new BigZp(prime);
			foreach (var share in shares)
				sumShares += share;

			return sumShares;
		}

	}
	
	/// <summary>
	/// Implements the MPC protocol of Zamani-Movahedi-Saia 2014 with eVSS.
	/// </summary>
	public class CryptoMpc : SyncMpc<BigZp>
	{
		/// <summary>
		/// MPC field modulus. Party's input is an element of this field.
		/// </summary>
		protected readonly BigInteger Prime;
		protected readonly BigCircuit Circuit;
		protected IList<BigZp> Shares;
		public readonly int Seed;
		public override ProtocolIds Id { get { return ProtocolIds.ZMS; } }

		public CryptoMpc(SyncParty p, BigCircuit circuit, IList<int> pIds, BigZp input, int seed)
			: base(p, pIds, input)
		{
			Debug.Assert(circuit.InputCount == pIds.Count);
			Circuit = circuit;
			Prime = input.Prime;
			Seed = seed;
		}

		public override void Run()
		{
			// Build quorums
			var q = (int)Math.Log(PartyIds.Count, 2);
			var qb = new StaticSampler(Party, PartyIds);
			var qList = qb.CreateQuorums(PartyIds.Count, q);

			var dealer1 = qList[2][0];
			var dealer2 = qList[2][1];

			// For simplicity, we run the protocol only for the first two quorums Q1 and Q2.
			// Two parties from the third quorum Q3 share their inputs to Q1 and Q2. We call them dealers.

			if (qList[0].Contains(Party.Id) || Party.Id == dealer1 || Party.Id == dealer2)
			{
				var Q1 = new Quorum(this, qList[0], new int[] { dealer1, dealer2 }, 1);
				Q1.InputCommitment();

				if (qList[0].Contains(Party.Id))
				{
					// Perform computation on the input shares
					var gi = Q1.Multiply(Q1.InputShare1, Q1.InputShare2);
					gi = Q1.Multiply(gi, Q1.InputShare2);
					gi = Q1.Multiply(gi, Q1.InputShare1);
					gi = Q1.Multiply(gi, Q1.InputShare2);
					gi = gi + Q1.InputShare1;
					Result = Q1.Reconst(gi);

					// Reshare the result sharing to the parties of Q2
				}
			}

			if (qList[1].Contains(Party.Id))
			{
				var Q2 = new Quorum(this, qList[1], null, 1);

				// wait until Q1 finishes computation and resharing
			}
		}

		protected void Compute()
		{
			// evaluate the circuit gate by gate
			foreach (var gate in Circuit.Gates)
				ComputeGate(gate, Shares);

			// publish the circuit output
			var resultMsgs = BroadcastReceive(new ShareMsg<BigZp>(new Share<BigZp>(Circuit.Output), Stage.ResultReceive));

			var zpList = new List<BigZp>();
			foreach (var resultMsg in resultMsgs.OrderBy(s => s.SenderId))
				zpList.Add(resultMsg.Share.Value);

			//Result = BigShamirSharing.Recombine(zpList, ShamirPolyDegree, Prime);
		}

		/// <summary>
		/// Inputs are all the inputs to the circuit. Gate should know to choose the inputs it needs.
		/// </summary>
		private void ComputeGate(BigGate gate, IList<BigZp> shares)
		{
			var values = new List<BigZp>();
			foreach (var wire in gate.InputWires)
			{
				if (wire.IsInput)
				{
					if (shares.Count <= wire.InputIndex)
						throw new Exception("Input " + wire.InputIndex + " is expected - not found in the list given");
					values.Add(shares[wire.InputIndex]);
				}
				else
				{
					Debug.Assert(wire.SourceGate != null && wire.SourceGate.IsOutputReady);
					values.Add(wire.ConstValue != null ? wire.ConstValue : wire.SourceGate.OutputValue);
				}
			}
			var result = new BigZp(values[0]);
			values.RemoveAt(0);

			foreach (var value in values)
			{
				var currValue = value;
				if (gate.Operation == Operation.Div)
					throw new NotImplementedException();

				result.Calculate(currValue, gate.Operation == Operation.Div ? Operation.Mul : gate.Operation);
			}
			gate.OutputValue = result;
		}
	}
}