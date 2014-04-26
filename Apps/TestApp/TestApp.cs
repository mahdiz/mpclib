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
		const int seed = 1234;
		const int min_logn = 6;		// min log number of parties
		const int max_logn = 6;		// max log number of parties
		
		static readonly BigInteger encPrime = NumTheoryUtils.DHPrime1536;
		static readonly BigInteger prime = BigInteger.Parse("730750862221594424981965739670091261094297337857");

		public static void Main(string[] args)
		{
			Debug.Assert(NumTheoryUtils.MillerRabin(prime, 5) == false);		// must be a prime
			//Debug.Assert(BigInteger.ModPow(2, prime, encPrime) == 1);			// check 2^prime mod p = 1

			for (int n = (int)Math.Pow(2, min_logn); n <= Math.Pow(2, max_logn); n++)
			{
				int numQuorums = n;
				int quorumSize = (int)Math.Log(n, 2);
				TestCryptoMpc_eVSS(n, prime - 1, prime, seed);
			}
			Console.ReadLine();
		}

		private static void TestCryptoMpc_eVSS(int n, BigInteger maxInput, BigInteger prime, int seed)
		{
			// Create an MPC network, add parties, and init them with random inputs
			var mpcSim = new SyncSimController<SyncParty<CryptoMpc>>(seed);
			var parser = new BigParser(FunctionType.Sum, n, prime);
			parser.Parse();

			var parties = mpcSim.AddNewParties(n);

			foreach (var party in parties)
			{
				var randInput = new BigZp(prime, StaticRandom.Next(maxInput));

				party.Protocol = new CryptoMpc(party, parser.Circuit,
					mpcSim.PartyIds, randInput, seed);
			}
			Console.WriteLine(n + " parties initialized. Running simulation...\n");

			// run the MPC network
			var elapsedTime = Timex.Run(() => mpcSim.Run());

			var realProduct = new BigZp(prime, 1);
			//foreach (var player in mpcSim.Parties)
			//	realProduct += (player.Protocol as IMpcProtocol<BigZp>).Input;

			realProduct *= mpcSim.Parties[12].Protocol.Input;
			realProduct *= mpcSim.Parties[13].Protocol.Input;
			realProduct *= mpcSim.Parties[13].Protocol.Input;
			realProduct *= mpcSim.Parties[12].Protocol.Input;
			realProduct *= mpcSim.Parties[13].Protocol.Input;
			realProduct += mpcSim.Parties[12].Protocol.Input;

			Console.WriteLine("");
			//var hasError = false;
			for (int i = 0; i < 6; i++)
			{
				Console.WriteLine("P" + mpcSim.Parties[i].Id + ", Res = " + mpcSim.Parties[i].Protocol.Result);

				//if (party.Protocol.Result != realProduct)
				//	hasError = true;
			}

			Console.WriteLine("\nResult  = " + realProduct);
			//if (hasError)
			//{
			//	var prevColor = Console.ForegroundColor;
			//	Console.ForegroundColor = ConsoleColor.Red;
			//	Console.WriteLine("ERROR: Some or all player outputs do not match the real sum!\n");
			//	Console.ForegroundColor = prevColor;
			//}

			Console.WriteLine("# parties    = " + n);
			Console.WriteLine("# msgs sent  = " + mpcSim.SentMessageCount);
			Console.WriteLine("# bits sent  = " + (mpcSim.SentByteCount * 8).ToString("0.##E+00"));
			//Console.WriteLine("# rounds     = " + mpcSim.SimulationTime);
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