using System.Collections.Generic;
using System.Collections.ObjectModel;
using MpcLib.Common;

namespace MpcLib.DistributedSystem.QuorumBuilding
{
	public class StaticSampler : SyncProtocol
	{
		public override ProtocolIds Id { get { return ProtocolIds.StaticSampler; } }

		public StaticSampler(SyncParty p, IList<int> partyIds)
			: base(p, partyIds)
		{
		}

		public List<int[]> CreateQuorums(int numQuorums, int quorumSize)
		{
			var quorums = new List<int[]>();
			int procIndex = 0;

			for (int i = 0; i < numQuorums; i++)
			{
				var qParties = new int[quorumSize];
				for (int j = 0; j < quorumSize; j++)
				{
					qParties[j] = procIndex;
					procIndex = (procIndex + 1) % NumParties;
				}
				quorums.Add(qParties);
			}
			return quorums;
		}
	}
}