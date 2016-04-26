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

        //static readonly BigInteger prime = BigInteger.Parse("730750862221594424981965739670091261094297337857");
        static readonly BigInteger prime = BigInteger.Parse("997");

        public static void Main(string[] args)
        {
            Debug.Assert(NumTheoryUtils.MillerRabin(prime, 5) == false);        // must be a prime
            int n = 5;      // number of parties

            // Create an MPC network, add parties, and init them with random inputs
            NetSimulator.Init(seed);
            
            SetupCompareAndSwap(new Quorum(0, n));

            Console.WriteLine(n + " parties initialized. Running simulation...\n");

            // run the simulator
            var elapsedTime = Timex.Run(() => NetSimulator.Run());

            Console.WriteLine("Simulation finished.  Checking results...\n");

            CheckCompareAndSwap();

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
                ReconstructionProtocol rp = new ReconstructionProtocol(party, quorum, new Share<BigZp>(shares[i]));
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
                TestParty<Share<BigZp>> party = new TestParty<Share<BigZp>>();
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
                TestParty<Share<BigZp>> party = new TestParty<Share<BigZp>>();
                party.UnderTest = new ShareMultiplicationProtocol(party, quorum, new Share<BigZp>(sharesA[i]), new Share<BigZp>(sharesB[i]));
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
                TestParty<Share<BigZp>> party = new TestParty<Share<BigZp>>();
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
                TestParty<Share<BigZp>> party = new TestParty<Share<BigZp>>();
                party.UnderTest = new BitCompositionProtocol(party, quorum, 
                    MakeList(sharesA[i], sharesB[i], sharesC[i]), prime);
                NetSimulator.RegisterParty(party);
            }
        }

        public static void CheckBitCompositionProtocol()
        {
            int n = NetSimulator.PartyCount;
            Console.WriteLine("Expected Output: " + 6);
            Console.WriteLine("Result: " + Reconstruct(new Quorum(0, n)));
        }

        public static void SetupBitwiseAndProtocol(int n)
        {
            Quorum quorum = new ByzantineQuorum(0, n, 0);

            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;

            var sharesA = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);
            var sharesB = BigShamirSharing.Share(new BigZp(prime, 1), n, polyDeg);
            var sharesC = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);

            var sharesD = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);
            var sharesE = BigShamirSharing.Share(new BigZp(prime, 1), n, polyDeg);
            var sharesF = BigShamirSharing.Share(new BigZp(prime, 1), n, polyDeg);


            for (int i = 0; i < n; i++)
            {
                TestParty<List<Share<BigZp>>> party = new TestParty<List<Share<BigZp>>>();
                party.UnderTest = new BitwiseOperationProtocol(party, quorum,
                    MakeList(sharesA[i], sharesB[i], sharesC[i]),
                    MakeList(sharesD[i], sharesE[i], sharesF[i]),
                    new SharedBitAnd.ProtocolFactory(party, quorum));
                NetSimulator.RegisterParty(party);
            }
        }

        public static void CheckBitwiseAndProtocol()
        {
            int n = NetSimulator.PartyCount;
            List<BigZp> result = ReconstructBitwise(new Quorum(0, n), 3);
            Console.WriteLine("Expected Output: 010");
            Console.Write("Result: ");
            foreach (var bit in result)
            {
                Console.Write(bit);
            }
            Console.WriteLine();
        }

        public static void SetupPrefixOrProtocol(Quorum quorum)
        {
            int n = quorum.Size;
            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;

            var sharesA = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);
            var sharesB = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);
            var sharesC = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);
            var sharesD = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);

            for (int i = 0; i < n; i++)
            {
                TestParty<List<Share<BigZp>>> party = new TestParty<List<Share<BigZp>>>();
                party.UnderTest = new PrefixOperationProtocol(party, quorum, 
                    MakeList(sharesA[i], sharesB[i], sharesC[i], sharesD[i]), 
                    new SharedBitOr.ProtocolFactory(party, quorum));
                NetSimulator.RegisterParty(party);
            }
        }

        public static void CheckPrefixOrProtocol()
        {
            int n = NetSimulator.PartyCount;
            List<BigZp> result = ReconstructBitwise(new Quorum(0, n), 4);
            Console.WriteLine("Expected Output: 011");
            Console.Write("Result: ");
            foreach (var bit in result)
            {
                Console.Write(bit);
            }
            Console.WriteLine();
        }

        public static void SetupBitwiseLessThan(Quorum quorum)
        {
            int n = quorum.Size;
            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;

            var sharesA = BigShamirSharing.Share(new BigZp(prime, 1), n, polyDeg);
            var sharesB = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);
            var sharesC = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);
            var sharesD = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);

            var sharesE = BigShamirSharing.Share(new BigZp(prime, 1), n, polyDeg);
            var sharesF = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);
            var sharesG = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);
            var sharesH = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);

            for (int i = 0; i < n; i++)
            {
                TestParty<Share<BigZp>> party = new TestParty<Share<BigZp>>();
                party.UnderTest = new BitwiseLessThanProtocol(party, quorum,
                    MakeList(sharesA[i], sharesB[i], sharesC[i], sharesD[i]),
                    MakeList(sharesE[i], sharesF[i], sharesG[i], sharesH[i]));
                NetSimulator.RegisterParty(party);
            }
        }

        public static void CheckBitwiseLessThan()
        {
            int n = NetSimulator.PartyCount;
            Console.WriteLine("Expected Output: " + 1);
            Console.WriteLine("Result: " + Reconstruct(new Quorum(0, n)));
        }

        public static void SetupBitwiseRandomGeneration(int n)
        {
            Quorum quorum = new ByzantineQuorum(0, n, 0);

            for (int i = 0; i < n; i++)
            {
                TestParty<List<Share<BigZp>>> party = new TestParty<List<Share<BigZp>>>();
                party.UnderTest = new RandomBitwiseGenProtocol(party, quorum, prime, 15);
                NetSimulator.RegisterParty(party);
            }
        }

        public static void SetupLeastSignificantBit(Quorum quorum)
        {
            int n = quorum.Size;

            var input = new BigZp(prime, 2);
            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;

            var shares = BigShamirSharing.Share(input, n, polyDeg);
            for (int i = 0; i < n; i++)
            {
                TestParty<Share<BigZp>> party = new TestParty<Share<BigZp>>();
                party.UnderTest = new LeastSignificantBitProtocol(party, quorum, new Share<BigZp>(shares[i]));
                NetSimulator.RegisterParty(party);
            }
        }

        public static void CheckLeastSignificantBitProtocol()
        {
            int n = NetSimulator.PartyCount;
            Console.WriteLine("Result: " + Reconstruct(new Quorum(0, n)));
        }

        public static void SetupLessThan(Quorum quorum)
        {
            int n = quorum.Size;
            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;

            var sharesA = BigShamirSharing.Share(new BigZp(prime, 700), n, polyDeg);
            var sharesB = BigShamirSharing.Share(new BigZp(prime, 549), n, polyDeg);

            for (int i = 0; i < n; i++)
            {
                TestParty<Share<BigZp>> party = new TestParty<Share<BigZp>>();
                party.UnderTest = new ShareLessThanProtocol(party, quorum, new Share<BigZp>(sharesA[i]), new Share<BigZp>(sharesB[i]));
                NetSimulator.RegisterParty(party);
            }
        }

        public static void CheckLessThan()
        {
            int n = NetSimulator.PartyCount;
            Console.WriteLine("Result: " + Reconstruct(new Quorum(0, n)));
        }

        public static void SetupCompareAndSwap(Quorum quorum)
        {
            int n = quorum.Size;
            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;

            var sharesA = BigShamirSharing.Share(new BigZp(prime, 700), n, polyDeg);
            var sharesB = BigShamirSharing.Share(new BigZp(prime, 701), n, polyDeg);

            for (int i = 0; i < n; i++)
            {
                TestParty<Tuple<Share<BigZp>, Share<BigZp>>> party = new TestParty<Tuple<Share<BigZp>, Share<BigZp>>>();
                party.UnderTest = new CompareAndSwapProtocol(party, quorum, new Share<BigZp>(sharesA[i]), new Share<BigZp>(sharesB[i]));
                NetSimulator.RegisterParty(party);
            }
        }

        public static void CheckCompareAndSwap()
        {
            int n = NetSimulator.PartyCount;
            Console.WriteLine("Result: " + ReconstructTuple(new Quorum(0, n)));
        }

        public static BigZp Reconstruct(Quorum q)
        {
            BigZp[] shares = new BigZp[q.Size];

            int i = 0;
            foreach (var id in q.Members)
            {
                shares[i++] = (NetSimulator.GetParty(id) as TestParty<Share<BigZp>>).UnderTest.Result.Value;
            }

            return BigShamirSharing.Recombine(new List<BigZp>(shares), (int)Math.Ceiling(q.Size / 3.0) - 1, prime);
        }

        public static Tuple<BigZp, BigZp> ReconstructTuple(Quorum q)
        {
            BigZp[] shares1 = new BigZp[q.Size];
            BigZp[] shares2 = new BigZp[q.Size];

            int i = 0;
            foreach (var id in q.Members)
            {
                var result = (NetSimulator.GetParty(id) as TestParty<Tuple<Share<BigZp>, Share<BigZp>>>).UnderTest.Result;
                shares1[i] = result.Item1.Value;
                shares2[i] = result.Item2.Value;
                i++;
            }

            var val1 = BigShamirSharing.Recombine(new List<BigZp>(shares1), (int)Math.Ceiling(q.Size / 3.0) - 1, prime);
            var val2 = BigShamirSharing.Recombine(new List<BigZp>(shares2), (int)Math.Ceiling(q.Size / 3.0) - 1, prime);

            return new Tuple<BigZp, BigZp>(val1, val2);
        }

        public static List<BigZp> ReconstructBitwise(Quorum q, int bitCount)
        {
            List<BigZp> result = new List<BigZp>();
            for (int i = bitCount - 1; i >= 0; i--)
            {
                BigZp[] shares = new BigZp[q.Size];

                int j = 0;
                foreach (var id in q.Members)
                {
                    shares[j++] = (NetSimulator.GetParty(id) as TestParty<List<Share<BigZp>>>).UnderTest.Result[i].Value;
                }
                result.Add(BigShamirSharing.Recombine(new List<BigZp>(shares), (int)Math.Ceiling(q.Size / 3.0) - 1, prime));
            }
            
            return result;
        }

        public static List<Share<BigZp>> MakeList(params BigZp[] vals)
        {
            List<Share<BigZp>> result = new List<Share<BigZp>>();
            foreach (var val in vals)
                result.Add(new Share<BigZp>(val));

            return result;
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