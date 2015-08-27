using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using MpcLib.Commitments.PolyCommitment;
using MpcLib.Common.FiniteField;
using MpcLib.SecretSharing;
using System.Linq;
using MpcLib.Common.StochasticUtils;

namespace Test
{
	class Program
	{
		const int seed = 1234;
		static readonly BigInteger Prime = BigInteger.Parse("730750862221594424981965739670091261094297337857");

		static void Main(string[] args)
		{
			Console.WriteLine("Started.");
			var PolyCommit = new PolyCommit();
			StaticRandom.Init(seed);

			for (int n = 16; n < 17; n++)
			{
				var polyDegree = (int)Math.Ceiling(n / 3.0);
				PolyCommit.Setup(polyDegree, seed);

				var prime = BigInteger.Parse("730750862221594424981965739670091261094297337857");
				IList<BigZp> coeffs = null;

				// generate a random polynomial
				var shares = BigShamirSharing.Share(new BigZp(Prime, 2), n, polyDegree - 1, out coeffs);

				// generate evaluation points x = {1,...,n}
				var iz = new BigZp[n];
				for (int i = 0; i < n; i++)
					iz[i] = new BigZp(Prime, new BigInteger(i + 1));

				// calculate the commitment and witnesses
				byte[] proof = null;
				MG[] witnesses = null;
				var commit = PolyCommit.Commit(coeffs.ToArray(), iz, ref witnesses, ref proof, false);

				var rankZp = new BigZp(Prime, 2);
				PolyCommit.VerifyEval(commit, rankZp, shares[2], witnesses.First());
			}
			Console.WriteLine("Finished.");
		}

		public static int GetRank(int myId, IList<int> ids)
		{
			// find my rank in sorted list of ids
			int rank = 1;
			foreach (var id in ids.OrderBy(p => p))
			{
				if (id == myId)
					break;
				rank++;
			}
			return rank;
		}

		//static void Main(string[] args)
		//{
		//	Console.WriteLine("Started.");
		//	var prime = BigInteger.Parse("730750862221594424981965739670091261094297337857");

		//	Console.WriteLine("t\tSetup\tCommit\tVerify");

		//	for (int t = 3; t <= 30; t++)
		//	{
		//		PcParams pcParams = null;
		//		var setupTime = Timex.Run(() => PolyCommit.CreateParams(t), ref pcParams);

		//		// polynomial coefficients
		//		var coeffs = new BigZp[t];
		//		for (int i = 0; i < t; i++)
		//			coeffs[i] = new BigZp(prime, new BigInteger(5));

		//		// evaluation points
		//		var izVec = new BigZp[3 * t];
		//		for (int i = 1; i < 3 * t + 1; i++)
		//			izVec[i - 1] = new BigZp(prime, new BigInteger(i));

		//		var pc = new PolyCommit();

		//		MG mg = null;
		//		byte[] proof = null;
		//		MG[] witnesses = null;

		//		var cTime = Timex.Run(() =>
		//			pc.Commit(pcParams, coeffs, izVec, ref witnesses, ref proof, true), ref mg);

		//		bool result = false;
		//		var vpTime = Timex.Run(() =>
		//			pc.VerifyProof(pcParams, mg, proof), ref result);

		//		if (!result)
		//			Console.WriteLine("VerifyProof FAILED!");

		//		double veTime = 0;
		//		for (int j = 1; j < 3 * t + 1; j++)
		//		{
		//			var i = new BigZp(prime, new BigInteger(j));
		//			var fi = PolyCommit.Eval(coeffs, i);

		//			result = false;
		//			veTime += Timex.Run(() =>
		//				pc.VerifyEval(pcParams, mg, i, fi, witnesses[j - 1]), ref result);

		//			if (!result)
		//				Console.WriteLine("VerifyEval FAILED!");
		//		}
		//		veTime /= 3 * t;

		//		Console.WriteLine(t + "\t" +
		//			setupTime.ToString("#") + "\t" +
		//			cTime.ToString("#") + "\t" +
		//			veTime.ToString("#.##"));

		//		//Console.WriteLine("t = " + t +
		//		//	", Setup = " + setupTime.ToString("#") +
		//		//	", Commit = " + cTime.ToString("#") +
		//		//	", VerifyEval = " + veTime.ToString("#"),
		//		//	", VerifyProof = " + vpTime.ToString("#"));
		//	}

		//	Console.WriteLine("Finished.");
		//	Console.ReadLine();
		//}
	}
}
