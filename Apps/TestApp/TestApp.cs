using System;
using System.Diagnostics;
using System.Numerics;
using MpcLib.Common;
using MpcLib.Common.FiniteField;
using MpcLib.Common.FiniteField.Circuits;
using MpcLib.Common.StochasticUtils;
using MpcLib.DistributedSystem;
using MpcLib.DistributedSystem.Anonymity.Maskz;
using MpcLib.DistributedSystem.QuorumSystem;
using MpcLib.MpcProtocols;
using MpcLib.MpcProtocols.Crypto;
using MpcLib.MpcProtocols.Dkms;
using MpcLib.RandomGeneration.AllToAllGeneration;
using MpcLib.SecretSharing;
using MpcLib.SecretSharing.QuorumShareRenewal;
using MpcLib.Simulation;
using MpcLib.Simulation.Des;
using System.Collections.Generic;

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
		const int min_logn = 10;		// min log number of parties
		const int max_logn = 30;		// max log number of parties
		
		static readonly BigInteger encPrime = NumTheoryUtils.DHPrime1536;
		static readonly BigInteger prime = BigInteger.Parse("730750862221594424981965739670091261094297337857");

		public static void Main(string[] args)
		{
			Debug.Assert(NumTheoryUtils.MillerRabin(prime, 5) == false);		// must be a prime
			//Debug.Assert(BigInteger.ModPow(2, prime, encPrime) == 1);			// check 2^prime mod p = 1

            /*
			for (var i = min_logn; i <= max_logn; i++)
			{
				var n = BigInteger.Pow(2, i);
				TestQuorumMpc(n, prime, prime, seed);
			}*/

            testThings2();

			Console.ReadLine();
		}


        private static void testThings()
        {
            var mpcSim = new SyncSimController<SyncParty<AllToAllGeneration>>(0);
            var parties = mpcSim.AddNewParties(5);

            foreach (var party in parties)
            {
                party.Protocol = new AllToAllGeneration(party, mpcSim.PartyIds, prime);
            }

            mpcSim.Run();
        }

        private static void testThings2()
        {
            int prime = 29;
            int fromParties = 5;
            int toParties = 15;
            int fromDeg = fromParties / 3;
            int toDeg = toParties / 3;

            var mpcSim = new SyncSimController<SyncParty<QuorumShareRenewal>>(0);
            var parties = mpcSim.AddNewParties(fromParties + toParties);

            IList<int> quorumFrom = new List<int>(), quorumTo = new List<int>();

            for (int i = 0; i < fromParties; i++)
            {
                quorumFrom.Add(i);
            }

            for (int i = fromParties; i < fromParties + toParties; i++)
            {
                quorumTo.Add(i);
            }

            var shares = ShamirSharing.Share(new Zp(prime, 26), fromParties, fromDeg);
            
            for (int i = 0; i < parties.Count; i++)
            {
                var party = parties[i];
                if (i < shares.Count)
                {
                    party.Protocol = new QuorumShareRenewal(party, mpcSim.PartyIds, quorumFrom, quorumTo, prime, shares[i]);
                }
                else
                {
                    party.Protocol = new QuorumShareRenewal(party, mpcSim.PartyIds, quorumFrom, quorumTo, prime);
                }
            }

            mpcSim.Run();


            var newShares = new List<Zp>();
            for (int i = fromParties; i < fromParties + toParties; i++)
            {
                newShares.Add(parties[i].Protocol.Share);
            }


            var result = ShamirSharing.Recombine(newShares, toDeg, prime);
            Console.WriteLine("result: " + result);
        }

        private static void TestQuorumMpc(BigInteger n, BigInteger maxInput, BigInteger prime, int seed)
        {
            var N = (int)BigInteger.Log(n, 2);
            //var numParties = 2 * N;		// two quorums only
            var numParties = N;     // one quorums only

            var mpcSim = new SyncSimController<SyncParty<CryptoMpc>>(seed);
            var parties = mpcSim.AddNewParties(numParties);

            foreach (var party in parties)
            {
                var randInput = new BigZp(prime, StaticRandom.Next(maxInput));
                party.Protocol = new CryptoMpc(party, mpcSim.PartyIds, randInput, seed);
            }
            //Console.WriteLine(numParties + " parties initialized. Running simulation...\n");

            // run the simulator
            var elapsedTime = Timex.Run(() => mpcSim.Run());

            //var realProduct = new BigZp(prime, 1);
            //var d1 = N;
            //var d2 = N + 1;

            //Console.WriteLine("Input parties are " + d1 + " and " + d2 + "\n");
            //realProduct = mpcSim.Parties[d1].Protocol.Input *
            //	mpcSim.Parties[d2].Protocol.Input * mpcSim.Parties[d2].Protocol.Input + 
            //	mpcSim.Parties[d1].Protocol.Input;

            //Console.WriteLine("");
            //for (int i = 0; i < N; i++)
            //	Console.WriteLine("P" + mpcSim.Parties[i].Id + ", Res\t = " + mpcSim.Parties[i].Protocol.Result);

            //Console.WriteLine("\nResult\t = " + realProduct + "\n");

            //Console.WriteLine("# parties    = " + n);
            //Console.WriteLine("Quorum size  = " + N);
            //Console.WriteLine("# msgs sent  = " + mpcSim.SentMessageCount);
            //Console.WriteLine("# bits sent  = " + (mpcSim.SentByteCount * 8).ToString("0.##E+00"));
            //Console.WriteLine("Key size     = " + NumTheoryUtils.GetBitLength(prime) + " bits");
            //Console.WriteLine("Seed         = " + seed + "\n");
            //Console.WriteLine("Elapsed time = " + elapsedTime.ToString("hh':'mm':'ss'.'fff") + "\n");
            Console.WriteLine(n + " \t " + mpcSim.SentMessageCount + " \t " + (mpcSim.SentByteCount * 8));
        }

        private static void TestCryptoMpc_eVSS(int n, BigInteger maxInput, BigInteger prime, int seed)
		{
			// Create an MPC network, add parties, and init them with random inputs
			var mpcSim = new SyncSimController<SyncParty<CryptoMpc>>(seed);
			var parties = mpcSim.AddNewParties(n);

			foreach (var party in parties)
			{
				var randInput = new BigZp(prime, StaticRandom.Next(maxInput));
				party.Protocol = new CryptoMpc(party, mpcSim.PartyIds, randInput, seed);
			}
			Console.WriteLine(n + " parties initialized. Running simulation...\n");

			// run the MPC network
			var elapsedTime = Timex.Run(() => mpcSim.Run());

			var realProduct = new BigZp(prime, 1);
			var d1 = (int)(2 * Math.Log(n, 2));
			var d2 = d1 + 1;

			realProduct *= mpcSim.Parties[d1].Protocol.Input;
			realProduct *= mpcSim.Parties[d2].Protocol.Input;
			realProduct *= mpcSim.Parties[d2].Protocol.Input;
			realProduct *= mpcSim.Parties[d1].Protocol.Input;
			realProduct *= mpcSim.Parties[d2].Protocol.Input;
			realProduct += mpcSim.Parties[d1].Protocol.Input;

			Console.WriteLine("");
			for (int i = 0; i < Math.Log(n, 2); i++)
				Console.WriteLine("P" + mpcSim.Parties[i].Id + ", Res\t = " + mpcSim.Parties[i].Protocol.Result);

			Console.WriteLine("\nResult\t = " + realProduct + "\n");
			Console.WriteLine("# parties    = " + n);
			Console.WriteLine("# msgs sent  = " + mpcSim.SentMessageCount);
			Console.WriteLine("# bits sent  = " + (mpcSim.SentByteCount * 8).ToString("0.##E+00"));
			Console.WriteLine("Key size     = " + NumTheoryUtils.GetBitLength(prime) + " bits");
			Console.WriteLine("Seed         = " + seed + "\n");
			Console.WriteLine("Elapsed time = " + elapsedTime.ToString("hh':'mm':'ss'.'fff") + "\n");
		}

		private static void TestCryptoMpc_DL(int n, int maxInput, BigInteger prime, BigInteger encPrime, int seed)
		{
			// Initialize a discrete-event simulator
			//var des = new ConcurrentSimulator();
			var des = new SequentialEventSimulator();

			// Create an MPC network, add parties, and init them with random inputs
			var mpcNet = new AsyncSimController<AsyncParty<DlCryptoMpc>>(des, seed);
			var parser = new BigParser(FunctionType.Sum, n, prime);
			parser.Parse();

			var parties = mpcNet.AddNewParties(n);
			var dlCrypto = new DiscreteLogCrypto(2, encPrime);

			foreach (var party in parties)
			{
				var randInput = new BigZp(prime, StaticRandom.Next(0, maxInput));

				party.Protocol = new DlCryptoMpc(party, parser.Circuit,
					mpcNet.PartyIds, randInput, null, dlCrypto);
			}
			Console.WriteLine(n + " players initialized. Running simulation...");
			
			// run the MPC network
			var startTime = DateTime.Now.TimeOfDay;
			mpcNet.Run();
			var endTime = DateTime.Now.TimeOfDay;
			var elapsedTime = endTime - startTime;

			var realSum = new BigZp(prime);
			foreach (var player in mpcNet.Parties)
				realSum += (player.Protocol as IMpcProtocol<BigZp>).Input;

			var hasError = false;
			foreach (var party in mpcNet.Parties)
			{
				var p = party.Protocol as IMpcProtocol<BigZp>;
				//Console.WriteLine("P" + party.Id + " input = " + p.Input + "\t\tRes = " + p.Result);
				if (p.Result != realSum)
					hasError = true;
			}

			Console.WriteLine("\nSum result   = " + realSum);
			if (hasError)
			{
				var prevColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("ERROR: Some or all player outputs do not match the real sum!\n");
				Console.ForegroundColor = prevColor;
			}

            // write the log to the console
			Console.WriteLine("# parties    = " + n);
			Console.WriteLine("# msgs sent  = " + mpcNet.SentMessageCount);
			Console.WriteLine("# bits sent  = " + (mpcNet.SentByteCount * 8).ToString("0.##E+00"));
			Console.WriteLine("# rounds     = " + mpcNet.SimulationTime);
			Console.WriteLine("DH key size  = " + NumTheoryUtils.GetBitLength(encPrime) + " bits");
			Console.WriteLine("Seed         = " + seed + "\n");
			Console.WriteLine("Elapsed time = " + elapsedTime.ToString("hh':'mm':'ss'.'fff") + "\n");
		}

		private static void TestAnonymousMpc(int n, int numSlots, int maxInput, int numQuorums, 
			int quorumSize, bool byzantineCase, int prime, int seed)
		{
			// initialize a discrete-event simulator
			var des = new SequentialEventSimulator();

			var anonymMpc = new MaskzNetwork(des, seed);

			var rand = new Random(seed);
			var inputs = new Zp[n];
			for (int i = 0; i < n; i++)
				inputs[i] = new Zp(prime, rand.Next(0, maxInput));

			anonymMpc.Init(n, numQuorums, numSlots, quorumSize,
				QuorumBuildingMethod.RandomSampler,
				byzantineCase ? AdversaryModel.Byzantine : AdversaryModel.HonestButCurious,
				inputs, prime);

			Console.WriteLine(n + " players initialized. Running simulation...");

			// run the simulation
			var startTime = DateTime.Now.TimeOfDay;
			anonymMpc.Run();

			var endTime = DateTime.Now.TimeOfDay;
			var elapsedTime = endTime - startTime;

			var realSum = new Zp(prime);
			foreach (var player in anonymMpc.Parties)
				realSum += player.Input;

			var hasError = false;
			foreach (var player in anonymMpc.Parties)
			{
				Console.WriteLine("Player " + player.Id + " input is " + player.Input + ". His result is " + player.Result);
				if (player.Result != realSum)
					hasError = true;
			}

			if (hasError)
			{
				var prevColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("ERROR: Some or all player outputs do not match the real sum!\n");
				Console.ForegroundColor = prevColor;
			}

			Console.WriteLine("\nReal sum: " + realSum);
			Console.WriteLine("# of players: " + n);
			Console.WriteLine("# of quorums: " + numQuorums);
			Console.WriteLine("# of slots: " + numSlots);
			Console.WriteLine("# of messages sent: " + anonymMpc.SentMessageCount);
			Console.WriteLine("Quorum size: " + quorumSize);
			Console.WriteLine("Started at " + startTime);
			Console.WriteLine("Finished at " + endTime);
			Console.WriteLine("Elapsed time: " + elapsedTime);
			Console.Read();
		}
	}
}