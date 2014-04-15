using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using PolyCommitment;
using Unm.Common.FiniteField;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Started.");
			var prime = BigInteger.Parse("730750862221594424981965739670091261094297337857");

			Console.WriteLine("t\tSetup\tCommit\tVerify");

			for (int t = 3; t <= 30; t++)
			{
				PcParams pcParams = null;
				var setupTime = Timex.Run(() => PolyCommit.CreateParams(t), ref pcParams);

				// polynomial coefficients
				var coeffs = new BigZp[t];
				for (int i = 0; i < t; i++)
					coeffs[i] = new BigZp(prime, new BigInteger(5));

				// evaluation points
				var izVec = new BigZp[3 * t];
				for (int i = 1; i < 3 * t + 1; i++)
					izVec[i - 1] = new BigZp(prime, new BigInteger(i));

				var pc = new PolyCommit();

				MG mg = null;
				byte[] proof = null;
				MG[] witnesses = null;

				var cTime = Timex.Run(() =>
					pc.Commit(pcParams, coeffs, izVec, ref witnesses, ref proof, true), ref mg);

				bool result = false;
				var vpTime = Timex.Run(() =>
					pc.VerifyProof(pcParams, mg, proof), ref result);

				if (!result)
					Console.WriteLine("VerifyProof FAILED!");

				double veTime = 0;
				for (int j = 1; j < 3 * t + 1; j++)
				{
					var i = new BigZp(prime, new BigInteger(j));
					var fi = PolyCommit.Eval(coeffs, i);

					result = false;
					veTime += Timex.Run(() =>
						pc.VerifyEval(pcParams, mg, i, fi, witnesses[j - 1]), ref result);

					if (!result)
						Console.WriteLine("VerifyEval FAILED!");
				}
				veTime /= 3 * t;

				Console.WriteLine(t + "\t" +
					setupTime.ToString("#") + "\t" +
					cTime.ToString("#") + "\t" +
					veTime.ToString("#.##"));

				//Console.WriteLine("t = " + t +
				//	", Setup = " + setupTime.ToString("#") +
				//	", Commit = " + cTime.ToString("#") +
				//	", VerifyEval = " + veTime.ToString("#"),
				//	", VerifyProof = " + vpTime.ToString("#"));
			}

			Console.WriteLine("Finished.");
			Console.ReadLine();
		}
	}
}
