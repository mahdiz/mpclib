//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Diagnostics;
//using System.Globalization;
//using System.Linq;
//using System.Numerics;
//using System.Threading;
//using MpcLib.Common;
//using MpcLib.Common.FiniteField;
//using MpcLib.Common.FiniteField.Circuits;
//using MpcLib.Common.StochasticUtils;
//using MpcLib.DistributedSystem;
//using MpcLib.SecretSharing;
//using MpcLib.SecretSharing.eVSS;

//namespace MpcLib.MpcProtocols.Crypto
//{	
//	/// <summary>
//	/// Implements the MPC protocol of Zamani-Movahedi-Saia 2014 with eVSS.
//	/// </summary>
//	public class OldCryptoMpc
//	{
//		/// <summary>
//		/// MPC field modulus. Party's input is an element of this field.
//		/// </summary>
//		protected readonly BigInteger Prime;
//		protected IList<BigZp> Shares;
//		public readonly int Seed;

//		public OldCryptoMpc(Party p, IList<int> pIds, BigZp input, int seed)
//		{
//			Prime = input.Prime;
//			Seed = seed;
//		}

//		public override void Start()
//		{
//			// For simplicity, we run the protocol only for the first two quorums Q1 and Q2.
//			// Two parties from the third quorum Q3 share their inputs to Q1 and Q2. We call them dealers.

//			// Build quorums
//			//var qb = new StaticSampler(Party, PartyIds);
//			//var qList = qb.CreateQuorums(2, PartyIds.Count / 2);

//			/////////// TEMPORARY FOR DISC'14
//			//var dealer1 = qList[1][0];
			
//			if (Party.Id == 0)
//			{
//				var polyDegree = (int)Math.Ceiling(Parties.Count / 3.0);
//				var evss = new eVSS(Party, Parties, Prime, polyDegree - 1);
//				evss.Setup(Seed);
//				evss.Share(new BigZp(Prime, StaticRandom.Next(Prime - 1)));

//				//var shares = BigShamirSharing.Share(new BigZp(29, 24), 10, 5);
//				//var v = BigShamirSharing.Recombine(shares, 5, 29, false);

//				//evss.Reconst(shares[0]);
//			}

//			/////// END TEMPORARY FOR DISC'14

//			//var dealer1 = qList[1][0];
//			//var dealer2 = qList[1][1];

//			//if (qList[0].Contains(Party.Id) || Party.Id == dealer1 || Party.Id == dealer2)
//			//{
//			//	var Q1 = new QuorumMpc(Party, qList[0], Input, new int[] { dealer1, dealer2 }, 1, Seed);
//			//	Q1.InputCommitment();

//			//	if (qList[0].Contains(Party.Id))
//			//	{
//			//		// Perform computation on the input shares
//			//		var one = new BigZp(Prime, 1);

//			//		//var x3 = Q1.Multiply(Q1.InputShare1, Q1.InputShare2.MultipInverse);
//			//		//var b = Q1.Multiply(x3, x3.MultipInverse);

//			//		//var rr = Q1.InputShare1 * Q1.InputShare1.MultipInverse;
//			//		//var tt = Q1.Multiply(Q1.InputShare1, Q1.InputShare1.MultipInverse);

//			//		//Console.WriteLine(rr);
//			//		//var test = Q1.Reconst(tt);
//			//		//Console.WriteLine(test);

//			//		//var y1 = Q1.Multiply(b, Q1.InputShare1) + Q1.Multiply(one - b, Q1.InputShare2);
//			//		//var y2 = Q1.Multiply(b, Q1.InputShare2) + Q1.Multiply(one - b, Q1.InputShare1);

//			//		//var yy = Q1.InputShare1 * Q1.InputShare1.MultipInverse;
//			//		//var ee = Q1.Multiply(Q1.InputShare1, Q1.InputShare1.MultipInverse);
//			//		//var tt = Q1.Reconst(yy);
//			//		//var tt2 = Q1.Reconst(ee);
//			//		//Console.WriteLine(yy);
//			//		//Console.WriteLine(tt);
//			//		//Console.WriteLine(tt2);

//			//		var x = Q1.InputShare1.MultipInverse * Q1.InputShare1;

//			//		var xtt = Q1.Reconst(Q1.InputShare1.MultipInverse);
//			//		Console.WriteLine(xtt);
//			//		Console.WriteLine(xtt.MultipInverse);
//			//		Console.WriteLine("");

//			//		//Result = Q1.Reconst(y1);

//			//		// Reshare the result sharing to the parties of Q2
//			//		//Q1.Reshare(gi, qList[1]);
//			//	}
//			//}

//			//if (qList[1].Contains(Party.Id))
//			//{
//			//	var Q2 = new QuorumMpc(Party, qList[1], Input, null, 1, Seed);

//			//	// wait until Q1 finishes computation and resharing
//			//}
//		}

//		//protected void Compute()
//		//{
//		//	// evaluate the circuit gate by gate
//		//	foreach (var gate in Circuit.Gates)
//		//		ComputeGate(gate, Shares);

//		//	// publish the circuit output
//		//	var resultMsgs = BroadcastReceive(new ShareMsg<BigZp>(new Share<BigZp>(Circuit.Output), Stage.ResultReceive));

//		//	var zpList = new List<BigZp>();
//		//	foreach (var resultMsg in resultMsgs.OrderBy(s => s.SenderId))
//		//		zpList.Add(resultMsg.Share.Value);

//		//	//Result = BigShamirSharing.Recombine(zpList, ShamirPolyDegree, Prime);
//		//}

//		///// <summary>
//		///// Inputs are all the inputs to the circuit. Gate should know to choose the inputs it needs.
//		///// </summary>
//		//private void ComputeGate(BigGate gate, IList<BigZp> shares)
//		//{
//		//	var values = new List<BigZp>();
//		//	foreach (var wire in gate.InputWires)
//		//	{
//		//		if (wire.IsInput)
//		//		{
//		//			if (shares.Count <= wire.InputIndex)
//		//				throw new Exception("Input " + wire.InputIndex + " is expected - not found in the list given");
//		//			values.Add(shares[wire.InputIndex]);
//		//		}
//		//		else
//		//		{
//		//			Debug.Assert(wire.SourceGate != null && wire.SourceGate.IsOutputReady);
//		//			values.Add(wire.ConstValue != null ? wire.ConstValue : wire.SourceGate.OutputValue);
//		//		}
//		//	}
//		//	var result = new BigZp(values[0]);
//		//	values.RemoveAt(0);

//		//	foreach (var value in values)
//		//	{
//		//		var currValue = value;
//		//		if (gate.Operation == Operation.Div)
//		//			throw new NotImplementedException();

//		//		result.Calculate(currValue, gate.Operation == Operation.Div ? Operation.Mul : gate.Operation);
//		//	}
//		//	gate.OutputValue = result;
//		//}
//	}
//}