using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MpcLib.DistributedSystem.QuorumSystem.QuorumBuilding
{
	public class StaticSampler : Protocol, IQuorumBuilder
	{
		private int numQuorums;
		private int quorumSize;

		public event QbFinishHandler QbFinished;

		public StaticSampler(Entity e, ReadOnlyCollection<int> entityIds, 
			int numQuorums, int quorumSize, StateKey stateKey)
			: base(e, entityIds, stateKey)
		{
			this.numQuorums = numQuorums;
			this.quorumSize = quorumSize;
		}

		public override void Run()
		{
			var quorumsMap = new Dictionary<int, int[]>();
			int procIndex = 0;

			for (int i = 0; i < numQuorums; i++)
			{
				var quorumMembers = new int[quorumSize];
				for (int j = 0; j < quorumSize; j++)
				{
					quorumMembers[j] = procIndex;
					procIndex = (procIndex + 1) % EntityCount;
				}
				quorumsMap[i] = quorumMembers;
			}

			if (QbFinished != null)
				QbFinished(quorumsMap);
		}
	}
}