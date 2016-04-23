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
        //static readonly BigInteger prime = BigInteger.Parse("921883609544031586687447918048845036407010879311");

        public static void Main(string[] args)
        {
            Debug.Assert(NumTheoryUtils.MillerRabin(prime, 5) == false);        // must be a prime
            int n = 5;      // number of parties

            // Create an MPC network, add parties, and init them with random inputs
            NetSimulator.Init(seed);

            SetupRandomGenProtocol(n);

            Console.WriteLine(n + " parties initialized. Running simulation...\n");

            // run the simulator
            var elapsedTime = Timex.Run(() => NetSimulator.Run());

            Console.WriteLine("Simulation finished.  Checking results...\n");

            CheckRandomGenProtocol();

            Console.WriteLine("# parties    = " + n);
            Console.WriteLine("# msgs sent  = " + NetSimulator.SentMessageCount);
            Console.WriteLine("# bits sent  = " + (NetSimulator.SentByteCount * 8).ToString("0.##E+00"));
			Console.WriteLine("Key size     = " + NumTheoryUtils.GetBitLength(prime) + " bits");
			Console.WriteLine("Seed         = " + seed + "\n");
			Console.WriteLine("Elapsed time = " + elapsedTime.ToString("hh':'mm':'ss'.'fff") + "\n");
            Console.ReadKey();
		}


        public static void SetupReconstructionProtocol(int n)
        {
            var input = new BigZp(prime, 20);
            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;

            var shares = BigShamirSharing.Share(input, n, polyDeg);
            Quorum quorum = new Quorum(0, n);
            for (int i = 0; i < n; i++)
            {
                TestParty<BigZp> party = new TestParty<BigZp>();
                ReconstructionProtocol rp = new ReconstructionProtocol(party, quorum, shares[i]);
                party.UnderTest = rp;
                NetSimulator.RegisterParty(party);
            }
        }

        public static void CheckReconstructionProtocol()
        {
            Console.WriteLine("Valid Output: " + 2);
            foreach (var party in NetSimulator.Parties)
            {
                var p = party as TestParty<BigZp>;
                Console.WriteLine("Party: " + party.Id);
                if (!p.UnderTest.IsCompleted)
                    Console.WriteLine("Did not complete!");
                else
                    Console.WriteLine("Result: " + p.UnderTest.Result);
            }
        }

        public static void SetupRandomGenProtocol(int n)
        {
            Quorum quorum = new ByzantineQuorum(0, n, 0);
            for (int i = 0; i < n; i++)
            {
                TestParty<BigZp> party = new TestParty<BigZp>();
                party.UnderTest = new RandomGenProtocol(party, quorum, new BigZp(prime, i), prime);
                NetSimulator.RegisterParty(party);
            }
        }

        public static void CheckRandomGenProtocol()
        {
            int n = NetSimulator.PartyCount;
            Console.WriteLine("Expected Output: " + (n * (n - 1) / 2));
            Console.WriteLine("Result: " + Reconstruct(new Quorum(0, n)));
        }

        public static void SetupShareMultiplicationProtocol(int n)
        {
            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;

            var sharesA = BigShamirSharing.Share(new BigZp(prime, 20), n, polyDeg);
            var sharesB = BigShamirSharing.Share(new BigZp(prime, 50), n, polyDeg);

            Quorum quorum = new ByzantineQuorum(0, n, 0);
            for (int i = 0; i < n; i++)
            {
                TestParty<BigZp> party = new TestParty<BigZp>();
                party.UnderTest = new ShareMultiplicationProtocol(party, quorum, sharesA[i], sharesB[i]);
                NetSimulator.RegisterParty(party);
            }
        }

        public static void CheckShareMultiplicationProtocol()
        {
            int n = NetSimulator.PartyCount;
            Console.WriteLine("Expected Output: " + 1000);
            Console.WriteLine("Result: " + Reconstruct(new Quorum(0, n)));
        }

        public static void SetupRandomBitGenProtocol(int n)
        {
            Quorum quorum = new ByzantineQuorum(0, n, 0);
            for (int i = 0; i < n; i++)
            {
                TestParty<BigZp> party = new TestParty<BigZp>();
                party.UnderTest = new RandomBitGenProtocol(party, quorum, prime);
                NetSimulator.RegisterParty(party);
            }
        }

        public static void CheckRandomBitGenProtocol()
        {
            int n = NetSimulator.PartyCount;
            Console.WriteLine("Result: " + Reconstruct(new Quorum(0, n)));
        }

        public static void SetupBitCompositionProtocol(int n)
        {
            Quorum quorum = new ByzantineQuorum(0, n, 0);

            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;

            var sharesA = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);
            var sharesB = BigShamirSharing.Share(new BigZp(prime, 1), n, polyDeg);
            var sharesC = BigShamirSharing.Share(new BigZp(prime, 1), n, polyDeg);

            for (int i = 0; i < n; i++)
            {
                TestParty<BigZp> party = new TestParty<BigZp>();
                party.UnderTest = new BitCompositionProtocol(party, quorum, 
                    new List<BigZp>(new BigZp[] { sharesA[i], sharesB[i], sharesC[i] }), prime);
                NetSimulator.RegisterParty(party);
            }
        }

        public static void CheckBitCompositionProtocol()
        {
            int n = NetSimulator.PartyCount;
            Console.WriteLine("Expected Output: " + 6);
            Console.WriteLine("Result: " + Reconstruct(new Quorum(0, n)));
        }

        public static BigZp Reconstruct(Quorum q)
        {
            BigZp[] shares = new BigZp[q.Size];

            int i = 0;
            foreach (var id in q.Members)
            {
                shares[i++] = (NetSimulator.GetParty(id) as TestParty<BigZp>).UnderTest.Result;
            }

            return BigShamirSharing.Recombine(new List<BigZp>(shares), (int)Math.Ceiling(q.Size / 3.0) - 1, prime);
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