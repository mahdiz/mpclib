using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using MpcLib.Common;
using MpcLib.Common.FiniteField;
using MpcLib.Common.FiniteField.Circuits;
using MpcLib.DistributedSystem;
using MpcLib.SecretSharing;

namespace MpcLib.MpcProtocols.Crypto
{
	/// <summary>
	/// Implements the MPC protocol of Zamani-Movahedi-Saia 2014 with discrete log commitment (Feldman 87).
	/// </summary>
	public class DlCryptoMpc : MpcProtocol<BigZp>
	{
		#region Fields

		protected readonly BigCircuit Circuit;

		/// <summary>
		/// MPC field modulus. Party's input is an element of this field.
		/// </summary>
		protected readonly BigInteger Prime;

		/// <summary>
		/// Discrete log encrytion field modulus.
		/// We highly recommend this field to be initialized with
		/// a simulation-wide static value to keep the required memory small
		/// since one object of this MPC protocol is created for every party in the network.
		/// </summary>
		protected readonly DiscreteLogCrypto DlCrypto;

		protected IList<ShareMsg<BigZp>> RecvShares;
		protected IList<DlCommitMsg> RecvCommitments;
		protected readonly int ShamirPolyDegree;
		public override ProtocolIds Id { get { return ProtocolIds.ZMS_DL; } }

		#endregion Fields

		public DlCryptoMpc(Entity e, BigCircuit circuit, ReadOnlyCollection<int> pIds,
			BigZp input, StateKey stateKey, DiscreteLogCrypto dlCrypto)
			: base(e, pIds, input, stateKey)
		{
			Debug.Assert(circuit.InputCount == pIds.Count);
			Circuit = circuit;
			Prime = input.Prime;
			DlCrypto = dlCrypto;

			// k: min number of shares required to reconstruct the secret
			// t: max number of bad players
			// d: degree of polynomial in Shamir's scheme
			// we have d=k-1 because with k points we can interpolate 
			// a polynomial of degree at most k-1.
			// we also have k>t to prevent bad parties from sharing a fake secret.
			// hence we have d=t. In sync model, d=t=n/3.
			ShamirPolyDegree = (int)Math.Ceiling(NumParties / 3.0);
		}

		#region Methods
		
		public override void Run()
		{
			IList<BigZp> coeffs;

			// secret-share my input among all players
			var shareValues = BigShamirSharing.Share(
				Input, NumParties, ShamirPolyDegree, out coeffs);

			// calculate commitments to the share value, which is
			// the commitment to Shamir's polynomial coefficients
			var commitments = CalculateCommitments(coeffs);
			var commitMsg = new DlCommitMsg(commitments, Stage.CommitBroadcast);

			var shareMsgs = new List<ShareMsg<BigZp>>();
			foreach (var shareValue in shareValues)
				shareMsgs.Add(new ShareMsg<BigZp>(new Share<BigZp>(shareValue), Stage.InputReceive));

			// send the i-th share to the i-th party
			Send(shareMsgs);

			OnReceive((int)Stage.InputReceive,
				delegate(List<ShareMsg<BigZp>> shares)
				{
					RecvShares = shares.OrderBy(s => s.SenderId).ToList();
					if (RecvCommitments != null)
						VerifyAndCompute();
				});

			// broadcast the commitments 
			Broadcast(commitMsg);

			OnReceive((int)Stage.CommitBroadcast,
				delegate(List<DlCommitMsg> msgs)
				{
					RecvCommitments = msgs.OrderBy(s => s.SenderId).ToList();
					if (RecvShares != null)
						VerifyAndCompute();
				});
		}

		protected void VerifyAndCompute()
		{
			// verify received shares
			var suspectParties = Verify();

			// evaluate the circuit gate by gate
			foreach (var gate in Circuit.Gates)
				ComputeGate(gate, RecvShares);

			// publish the circuit output
			Broadcast(new ShareMsg<BigZp>(new Share<BigZp>(Circuit.Output), Stage.ResultReceive));

			OnReceive((int)Stage.ResultReceive,
				delegate(List<ShareMsg<BigZp>> resultMsgs)
				{
					var zpList = new List<BigZp>();
					foreach (var resultMsg in resultMsgs.OrderBy(s => s.SenderId))
						zpList.Add(resultMsg.Share.Value);

					Result = BigShamirSharing.Recombine(zpList, ShamirPolyDegree, Prime);
				});
		}

		protected virtual List<BigInteger> CalculateCommitments(IList<BigZp> values)
		{
			var commitments = new List<BigInteger>(values.Count);
			foreach (var value in values)
			{
				//commitments.Add();
			}
			return commitments;
		}

		protected virtual List<int> Verify()
		{
			var suspiciousParties = new List<int>();

			for (int i = 0; i < RecvCommitments.Count; i++)
			{
				BigInteger pr = 1;
				var commitMsg = RecvCommitments[i];				
				int k = Entity.Id + 1;

				Debug.Assert(commitMsg.SenderId == RecvShares[i].SenderId);

				for (int j = 0; j < commitMsg.Commitments.Count; j++)
				{
					var c = commitMsg.Commitments[j];
					pr = (pr * BigInteger.ModPow(c, BigInteger.Pow(k, j), DlCrypto.Prime)) % DlCrypto.Prime;
				}
				var efi = DlCrypto.Encrypt(RecvShares[i].Share.Value.Value);

				Debug.Assert(efi == pr, "Commitment check failed! Do we have malicious parties or the invalid fields?");
				if (efi != pr)
					suspiciousParties.Add(commitMsg.SenderId);
			}
			return suspiciousParties;
		}

		/// <summary>
		/// Inputs are all the inputs to the circuit. Gate should know to choose the inputs it needs.
		/// </summary>
		private void ComputeGate(BigGate gate, IList<ShareMsg<BigZp>> inputs)
		{
			var values = new List<BigZp>();
			foreach (var wire in gate.InputWires)
			{
				if (wire.IsInput)
				{
					if (inputs.Count <= wire.InputIndex)
						throw new Exception("Input " + wire.InputIndex + " is expected - not found in the list given");
					values.Add(inputs[wire.InputIndex].Share.Value);
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

		#endregion Methods
	}
}