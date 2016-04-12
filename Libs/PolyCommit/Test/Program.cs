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
        static readonly int Prime = 16651;
        
        static void Main(string[] args)
		{
			Console.WriteLine("Started.");
			StaticRandom.Init(seed);

            int quorumSize = 20;
            int degree = quorumSize / 3;
            
            var secret = new Zp(Prime, 3);
            var shareMatrix = ZpMatrix.GetIdentityMatrix(quorumSize, Prime);

            // create the initial shares
            var initalShares = ShamirSharing.Share(secret, quorumSize, degree);
            for (var i = 0; i < quorumSize; i++)
            {
                IList<Zp> reshares = QuorumSharing.CreateReshares(initalShares[i], quorumSize, degree);
                
                for (var j = 0; j < quorumSize; j++)
                {
                    shareMatrix.SetMatrixCell(j, i, reshares[j]);
                }
            }

            // combine the reshares
            List<Zp> finalShares = new List<Zp>();
            for (var i = 0; i < quorumSize; i++)
            {
                Zp finalShare = QuorumSharing.CombineReshares(shareMatrix.GetMatrixRow(i), quorumSize, Prime);
                finalShares.Add(finalShare);
            }

            // combine the shares
            Zp final = ShamirSharing.Recombine(finalShares, degree, Prime);
            Console.WriteLine(final.Value);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
	}
}
