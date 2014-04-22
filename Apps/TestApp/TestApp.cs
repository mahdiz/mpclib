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
using MpcLib.Simulation;
using MpcLib.Simulation.Des;

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
		const int seed = 0;
		const int min_logn = 2;		// min log number of parties
		const int max_logn = 2;		// max log number of parties
		
		static readonly BigInteger encPrime = NumTheoryUtils.DHPrime1536;
		static readonly BigInteger prime = BigInteger.Parse("730750862221594424981965739670091261094297337857");

		public static void Main(string[] args)
		{
			var randGen = new Random();		// random seed generator
			
			Debug.Assert(NumTheoryUtils.MillerRabin(prime, 5) == false);		// must be a prime
			//Debug.Assert(BigInteger.ModPow(2, prime, encPrime) == 1);			// check 2^prime mod p = 1

			for (int n = (int)Math.Pow(2, min_logn); n <= Math.Pow(2, max_logn); n++)
			{
				int numQuorums = n;
				int quorumSize = (int)Math.Log(n, 2);
				int partyMaxInput = int.MaxValue;
				TestCryptoMpc_eVSS(n, partyMaxInput, prime, 1234);
			}
			Console.ReadLine();
		}

		private static void TestCryptoMpc_eVSS(int n, int maxInput, BigInteger prime, int seed)
		{
			// Initialize a discrete-event simulator
			//var des = new ConcurrentEventSimulator();
			//var des = new SequentialEventSimulator();

			// Create an MPC network, add parties, and init them with random inputs
			var mpcSim = new SyncSimController<Entity<CryptoMpc>>(seed);
			var parser = new BigParser(FunctionType.Sum, n, prime);
			parser.Parse();

			var parties = mpcSim.AddNewEntities(n);

			foreach (var party in parties)
			{
				var randInput = new BigZp(prime, StaticRandom.Next(0, maxInput));

				party.Protocol = new CryptoMpc(party, parser.Circuit,
					mpcSim.EntityIds, randInput, null, seed);
			}
			Console.WriteLine(n + " players initialized. Running simulation...");

			// run the MPC network
			var elapsedTime = Timex.Run(() => mpcSim.Run());

			var realSum = new BigZp(prime);
			foreach (var player in mpcSim.Entities)
				realSum += (player.Protocol as IMpcProtocol<BigZp>).Input;

			var hasError = false;
			foreach (var party in mpcSim.Entities)
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

			Console.WriteLine("# parties    = " + n);
			Console.WriteLine("# msgs sent  = " + mpcSim.SentMessageCount);
			Console.WriteLine("# bits sent  = " + (mpcSim.SentByteCount * 8).ToString("0.##E+00"));
			//Console.WriteLine("# rounds     = " + mpcSim.SimulationTime);
			Console.WriteLine("DH key size  = " + NumTheoryUtils.GetBitLength(encPrime) + " bits");
			Console.WriteLine("Seed         = " + seed + "\n");
			Console.WriteLine("Elapsed time = " + elapsedTime.ToString("hh':'mm':'ss'.'fff") + "\n");
		}

		private static void TestCryptoMpc_DL(int n, int maxInput, BigInteger prime, BigInteger encPrime, int seed)
		{
			// Initialize a discrete-event simulator
			//var des = new ConcurrentSimulator();
			var des = new SequentialEventSimulator();

			// Create an MPC network, add parties, and init them with random inputs
			var mpcNet = new AsyncSimController<Entity<DlCryptoMpc>>(des, seed);
			var parser = new BigParser(FunctionType.Sum, n, prime);
			parser.Parse();

			var parties = mpcNet.AddNewEntities(n);
			var dlCrypto = new DiscreteLogCrypto(2, encPrime);

			foreach (var party in parties)
			{
				var randInput = new BigZp(prime, StaticRandom.Next(0, maxInput));

				party.Protocol = new DlCryptoMpc(party, parser.Circuit,
					mpcNet.EntityIds, randInput, null, dlCrypto);
			}
			Console.WriteLine(n + " players initialized. Running simulation...");
			
			// run the MPC network
			var startTime = DateTime.Now.TimeOfDay;
			mpcNet.Run();
			var endTime = DateTime.Now.TimeOfDay;
			var elapsedTime = endTime - startTime;

			var realSum = new BigZp(prime);
			foreach (var player in mpcNet.Entities)
				realSum += (player.Protocol as IMpcProtocol<BigZp>).Input;

			var hasError = false;
			foreach (var party in mpcNet.Entities)
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
			foreach (var player in anonymMpc.Entities)
				realSum += player.Input;

			var hasError = false;
			foreach (var player in anonymMpc.Entities)
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