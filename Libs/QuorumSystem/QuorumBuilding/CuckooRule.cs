using System;
using System.Collections.ObjectModel;
using MpcLib.Common;

namespace MpcLib.DistributedSystem.QuorumSystem.QuorumBuilding
{
	internal class CuckooRule : Protocol//, IQuorumBuilder
	{
		public event QbFinishHandler QbFinished;
		public override ProtocolIds Id { get { return ProtocolIds.Cuckoo; } }

		public CuckooRule(Entity e, ReadOnlyCollection<int> processorIds, StateKey stateKey)
			: base(e, processorIds, stateKey)
		{
		}

		public override void Run()
		{
			// Is the network still small?  If so, then we bootstrap

			// If not, then we do a cuckoo join operation
			throw new NotImplementedException();
		}
	}
}