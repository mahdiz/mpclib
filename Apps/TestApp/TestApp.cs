using System;
using System.Diagnostics;
using System.Numerics;
using MpcLib.Common;
using MpcLib.Common.FiniteField;
using MpcLib.Common.FiniteField.Circuits;
using MpcLib.Common.StochasticUtils;
using MpcLib.DistributedSystem;
using MpcLib.MpcProtocols;
using MpcLib.MpcProtocols.Crypto;
using MpcLib.MultiPartyShuffling;
using System.Collections.Generic;
using MpcLib.SecretSharing;

namespace MpcLib.Apps
{
    public enum FunctionTypes
    {
        Sum,
        Mul,
        Equal
    }

    public class TestApp
    {
        const int seed = 1234;
        const int min_logn = 10;        // min log number of parties
        const int max_logn = 30;        // max log number of parties

        static readonly BigInteger prime = BigInteger.Parse("730750862221594424981965739670091261094297337857");

        public static void Main(string[] args)
        {
            Debug.Assert(NumTheoryUtils.MillerRabin(prime, 5) == false);        // must be a prime
            int n = 4;      // number of parties

            // Create an MPC network, add parties, and init them with random inputs
            NetSimulator.Init(seed);

            // create honest users
            var parties = new List<MpsParty>(n);
            for (int i = 0; i < n; i++)
            {
                var randInput = new BigZp(prime, StaticRandom.Next(1000000));
                parties.Add(new MpsParty(n, randInput, prime, seed));
            }
            Console.WriteLine(n + " parties initialized. Running simulation...\n");

            // run the simulator
            var elapsedTime = Timex.Run(() => NetSimulator.Run());

            // print each party's input/outputs
            var validOuput = new BigZp(prime);
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine("Party " + parties[i].Id + ": Input = " + parties[i].Input + ", Output = " + parties[i].Output);
                validOuput += parties[i].Input;
            }

            Console.WriteLine("\nValid Output = " + validOuput + "\n");
            Console.WriteLine("# parties    = " + n);
            Console.WriteLine("# msgs sent  = " + NetSimulator.SentMessageCount);
            Console.WriteLine("# bits sent  = " + (NetSimulator.SentByteCount * 8).ToString("0.##E+00"));
            Console.WriteLine("Key size     = " + NumTheoryUtils.GetBitLength(prime) + " bits");
            Console.WriteLine("Seed         = " + seed + "\n");
            Console.WriteLine("Elapsed time = " + elapsedTime.ToString("hh':'mm':'ss'.'fff") + "\n");
            Console.ReadKey();
        }

        public static void TestShamir(int n, BigInteger prime, int seed)
        {
            StaticRandom.Init(seed);

            var PolyDegree = (int)Math.Ceiling(n / 3.0);
            var i1 = new BigZp(prime, StaticRandom.Next(1000000000));
            var i2 = new BigZp(prime, StaticRandom.Next(1000000000));
            var i3 = new BigZp(prime, StaticRandom.Next(1000000000));
            var i4 = new BigZp(prime, StaticRandom.Next(1000000000));
            var a = i1 + i2 + i3 + i4;

            var shares1 = BigShamirSharing.Share(i1, n, PolyDegree - 1);
            var shares2 = BigShamirSharing.Share(i2, n, PolyDegree - 1);
            var shares3 = BigShamirSharing.Share(i3, n, PolyDegree - 1);
            var shares4 = BigShamirSharing.Share(i4, n, PolyDegree - 1);

            var rs = new List<BigZp>(n);
            for (int i = 0; i < n; i++)
            {
                rs.Add(new BigZp(prime));
                rs[i] += shares1[i];
                rs[i] += shares2[i];
                rs[i] += shares3[i];
                rs[i] += shares4[i];
            }

            var t = BigShamirSharing.Recombine(rs, PolyDegree - 1, prime);
        }
    }
}