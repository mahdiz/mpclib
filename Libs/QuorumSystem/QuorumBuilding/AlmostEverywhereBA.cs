using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using MpcLib.Common;
using MpcLib.Common.StochasticUtils;

namespace MpcLib.DistributedSystem.QuorumSystem.QuorumBuilding
{
	internal class AlmostEverywhereProtocol : AsyncProtocol, IQuorumBuilder
	{
		public event QbFinishHandler QbFinished;
		public float C { get; private set; }
		public override ProtocolIds Id { get { return ProtocolIds.AE; } }
		private readonly RandomUtils randUtils = new RandomUtils();

		public AlmostEverywhereProtocol(AsyncParty e, ReadOnlyCollection<int> processorIds, StateKey stateKey)
			: this(e, processorIds, null, stateKey)
		{
		}

		public AlmostEverywhereProtocol(AsyncParty e, ReadOnlyCollection<int> processorIds, SafeRandom randGen, StateKey stateKey)
			: base(e, processorIds, stateKey)
		{
			if (randGen == null)
				randUtils = new RandomUtils();
			else
				randUtils = new RandomUtils(randGen);

			var cStr = ConfigurationManager.AppSettings["SampleListSizeFactor"];
			C = cStr == null ? 1 : float.Parse(cStr);
		}

		public override void Run()
		{
			SetupCandidateList();
		}

		private void SetupCandidateList()
		{
			int n = PartyIds.Count();

			var sampleList = randUtils.PickRandomElements(
				PartyIds.ToList(), (int)(C * Math.Sqrt(n) * Math.Log(n, 2)));

			int candStr = AlmostEverywhere();
			Broadcast(sampleList, new QsMessage<int>() { Data = candStr });

			throw new NotImplementedException();
		}

		private int AlmostEverywhere()
		{
			throw new NotImplementedException();
		}
	}
}