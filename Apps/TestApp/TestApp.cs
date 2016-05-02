using System;
using System.Diagnostics;
using System.Numerics;
using System.Linq;
using MpcLib.Common;
using MpcLib.Common.FiniteField;
using MpcLib.Common.StochasticUtils;
using MpcLib.DistributedSystem;
using MpcLib.MpcProtocols;
using MpcLib.MpcProtocols.Crypto;
using MpcLib.MultiPartyShuffling;
using System.Collections.Generic;
using MpcLib.SecretSharing;
using MpcLib.Circuits;

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
            int n = 16;      // number of parties

            // Create an MPC network, add parties, and init them with random inputs
            NetSimulator.Init(seed);   //seed
            StaticRandom.Init(seed+1); //seed + 1

            Quorum q = new Quorum(0, 0, n);

            SetupMps(n);
            Console.WriteLine(n + " parties initialized. Running simulation...\n");
            
            // run the simulator
            var elapsedTime = Timex.Run(() => NetSimulator.Run());

            CheckMps(n);
            
            Console.WriteLine("Simulation finished.  Checking results...\n");

            Console.WriteLine("# parties    = " + n);
            Console.WriteLine("# msgs sent  = " + NetSimulator.SentMessageCount);
            Console.WriteLine("# bits sent  = " + (NetSimulator.SentByteCount * 8).ToString("0.##E+00"));
			Console.WriteLine("Key size     = " + NumTheoryUtils.GetBitLength(prime) + " bits");
			Console.WriteLine("Seed         = " + seed + "\n");
			Console.WriteLine("Elapsed time = " + elapsedTime.ToString("hh':'mm':'ss'.'fff") + "\n");
            Console.ReadKey();
		}


        public static void SetupReconstructionProtocol(Quorum quorum)
        {
            int n = quorum.Size;
            var input = new BigZp(prime, 20);
            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;

            var shares = BigShamirSharing.Share(input, n, polyDeg);
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

        public static void SetupRandomGenProtocol(Quorum quorum)
        {
            int n = quorum.Size;
            for (int i = 0; i < n; i++)
            {
                TestParty<Share<BigZp>> party = new TestParty<Share<BigZp>>();
                party.UnderTest = new RandomGenProtocol(party, quorum.Clone() as Quorum, new BigZp(prime, i), prime);
                NetSimulator.RegisterParty(party);
            }
        }

        public static void SetupShareMultiplicationProtocol(Quorum quorum)
        {
            int n = quorum.Size;
            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;

            var sharesA = BigShamirSharing.Share(new BigZp(prime, 20), n, polyDeg);
            var sharesB = BigShamirSharing.Share(new BigZp(prime, 3), n, polyDeg);
            
            for (int i = 0; i < n; i++)
            {
                TestParty<Share<BigZp>> party = new TestParty<Share<BigZp>>();
                party.UnderTest = new ShareMultiplicationProtocol(party, quorum.Clone() as Quorum, new Share<BigZp>(sharesA[i]), new Share<BigZp>(sharesB[i]));
                NetSimulator.RegisterParty(party);
            }
        }

        public static void SetupRandomBitGenProtocol(Quorum quorum)
        {
            int n = quorum.Size;
            for (int i = 0; i < n; i++)
            {
                TestParty<Share<BigZp>> party = new TestParty<Share<BigZp>>();
                party.UnderTest = new RandomBitGenProtocol(party, quorum.Clone() as Quorum, prime);
                NetSimulator.RegisterParty(party);
            }
        }

        public static void SetupBitCompositionProtocol(Quorum quorum)
        {
            int n = quorum.Size;

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

        public static void SetupBitwiseAndProtocol(Quorum quorum)
        {
            int n = quorum.Size;

            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;

            var sharesA = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);
            var sharesB = BigShamirSharing.Share(new BigZp(prime, 1), n, polyDeg);
            var sharesC = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);

            var sharesD = BigShamirSharing.Share(new BigZp(prime, 0), n, polyDeg);
            var sharesE = BigShamirSharing.Share(new BigZp(prime, 1), n, polyDeg);
            var sharesF = BigShamirSharing.Share(new BigZp(prime, 1), n, polyDeg);


            for (int i = 0; i < n; i++)
            {
                Quorum q = quorum.Clone() as Quorum;
                TestParty<List<Share<BigZp>>> party = new TestParty<List<Share<BigZp>>>();
                party.UnderTest = new BitwiseOperationProtocol(party, q,
                    MakeList(sharesA[i], sharesB[i], sharesC[i]),
                    MakeList(sharesD[i], sharesE[i], sharesF[i]),
                    new SharedBitAnd.ProtocolFactory(party, q));
                NetSimulator.RegisterParty(party);
            }
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

        public static void SetupBitwiseRandomGeneration(Quorum quorum)
        {
            int n = quorum.Size;
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
        
        public static void SetupCompareAndSwap(Quorum quorum)
        {
            int n = quorum.Size;
            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;

            var sharesA = BigShamirSharing.Share(new BigZp(prime, 440), n, polyDeg);
            var sharesB = BigShamirSharing.Share(new BigZp(prime, 450), n, polyDeg);

            for (int i = 0; i < n; i++)
            {
                TestParty<Tuple<Share<BigZp>, Share<BigZp>>> party = new TestParty<Tuple<Share<BigZp>, Share<BigZp>>>();
                party.UnderTest = new CompareAndSwapProtocol(party, quorum.Clone() as Quorum, new Share<BigZp>(sharesA[i]), new Share<BigZp>(sharesB[i]));
                NetSimulator.RegisterParty(party);
            }
        }
        
        public static void Reconstruct(Quorum q)
        {
            BigZp[] shares = new BigZp[q.Size];

            int i = 0;
            foreach (var id in q.Members)
            {
                shares[i++] = (NetSimulator.GetParty(id) as TestParty<Share<BigZp>>).UnderTest.Result.Value;
            }

            var val = BigShamirSharing.Recombine(new List<BigZp>(shares), (int)Math.Ceiling(q.Size / 3.0) - 1, prime);

            Console.WriteLine("Output: " + val);
        }

        public static void ReconstructTuple(Quorum q)
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

            Console.WriteLine(val1 + " " + val2);
        }

        public static void ReconstructBitwise(Quorum q, int bitCount)
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

            foreach (var bit in result)
            {
                Console.Write(bit);
            }
            Console.WriteLine();
        }

        public static void ReconstructDictionary(Quorum q, OutputGateAddress[] ordering, int qSize)
        {
            List<BigZp> result = new List<BigZp>();
            foreach (OutputGateAddress outAddr in ordering)
            {
                if (outAddr == null)
                    continue;
                BigZp[] shares = new BigZp[qSize];

                int j = 0;
                foreach (var id in q.Members)
                {
                    Protocol<IDictionary<OutputGateAddress, Share<BigZp>>> p = (NetSimulator.GetParty(id) as TestParty<IDictionary<OutputGateAddress, Share<BigZp>>>).UnderTest;
                    if (p.Result.ContainsKey(outAddr))
                        shares[j++] = p.Result[outAddr].Value;
                }
                result.Add(BigShamirSharing.Recombine(new List<BigZp>(shares), (int)Math.Ceiling(qSize / 3.0) - 1, prime));
            }

            Console.WriteLine("Result: " + string.Join(" ", result));
        }

        public static List<Share<BigZp>> MakeList(params BigZp[] vals)
        {
            List<Share<BigZp>> result = new List<Share<BigZp>>();
            foreach (var val in vals)
                result.Add(new Share<BigZp>(val));

            return result;
        }

        public static void TestCreateCircuit()
        {
            var c = new LPSortingNetwork(1 << 3);
            Console.WriteLine(c);
        }

        public static PermutationNetwork network;
        public static void SetupSimpleCircuitEvaluation(Quorum quorum)
        {
            int n = quorum.Size;
            var polyDeg = (int)Math.Ceiling(n / 3.0) - 1;
            
            Debug.Assert((n & (n - 1)) == 0); // is power of 2
            
            network = new LPSortingNetwork(n);

            IList<BigZp>[] shares = new IList<BigZp>[n];

            for (int i = 0; i < n; i++)
                shares[i] = BigShamirSharing.Share(new BigZp(prime, 500 - 2*i), n, polyDeg);

            foreach (var id in quorum.Members)
            {
                Dictionary<InputGateAddress, Share<BigZp>> inShares = new Dictionary<InputGateAddress, Share<BigZp>>();

                int i = 0;
                foreach (var inAddr in network.Circuit.InputAddrs)
                {
                    inShares[inAddr] = new Share<BigZp>(shares[i][id]);
                    i++;
                }

                TestParty<IDictionary<OutputGateAddress, Share<BigZp>>> party = new TestParty<IDictionary<OutputGateAddress, Share<BigZp>>>();
                party.UnderTest = new SecureGroupCircuitEvaluation(party, quorum.Clone() as Quorum, network.Circuit, inShares);
                NetSimulator.RegisterParty(party);
            }
        }

        public static void SetupMultiQuorumCircuitEvaluation(Quorum bigQuorum)
        {
            int n = bigQuorum.Size;

            int qSize = n / 2;

            var polyDeg = (int)Math.Ceiling(qSize / 3.0) - 1;

            var quorums = new List<Quorum>();
            quorums.Add(new Quorum(0, 0, qSize));
            quorums.Add(new Quorum(1, qSize, 2*qSize));

            Debug.Assert((n & (n - 1)) == 0); // is power of 2

            network = new LPSortingNetwork(n);
            //network = SortingNetworkFactory.CreateButterflyTournamentRound(4);

            network.CollapsePermutationGates();

            IList<BigZp>[] shares = new IList<BigZp>[n];

            for (int i = 0; i < n; i++)
                shares[i] = BigShamirSharing.Share(new BigZp(prime, 500 - 2 * i), qSize, polyDeg);
            
            foreach (var id in bigQuorum.Members)
            {
                Dictionary<InputGateAddress, Share<BigZp>> inShares = new Dictionary<InputGateAddress, Share<BigZp>>();

                int i = 0;
                foreach (var inAddr in network.Circuit.InputAddrs)
                {
                    inShares[inAddr] = new Share<BigZp>(shares[i][id % 4]);
                    i++;
                }

                TestParty<IDictionary<OutputGateAddress, Share<BigZp>>> party = new TestParty<IDictionary<OutputGateAddress, Share<BigZp>>>();
                Quorum[] quorumsClone = quorums.Select(a => a.Clone() as Quorum).ToArray();
                
                party.UnderTest = 
                    new SecureMultiQuorumCircuitEvaluation(party, quorumsClone[id / qSize], quorumsClone,
                    ProtocolIdGenerator.GenericIdentifier(0), network.Circuit, inShares, prime);

                NetSimulator.RegisterParty(party);
            }
        }

        public static void SetupMps(int n)
        {
            List<BigZp> inputs = new List<BigZp>();
            for (int i = 0; i < n; i++)
            {
                inputs.Add(new BigZp(prime, StaticRandom.Next(prime)));
            }

            Console.WriteLine("Inputs: " + string.Join(" ", inputs));
            var sorted = inputs.Select(zp => zp.Value).ToList();
            sorted.Sort();

            Console.WriteLine("Expected Output: " + string.Join(" ", sorted));

            for (int i = 0; i < n; i++)
            {
                NetSimulator.RegisterParty(new MpsParty(n, inputs[i]));
            }
        }

        public static void CheckMps(int n)
        {
            for (int i = 0; i < n; i++)
            {
                MpsParty party = (MpsParty)NetSimulator.GetParty(i);
                Console.WriteLine("Party " + i + ": " + string.Join(" ", party.Results));
            }
        }

        public static void TestShamir(int n, BigInteger prime, int seed)
		{
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