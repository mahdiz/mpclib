using System;
using System.Collections.ObjectModel;

namespace MpcLib.DistributedSystem.QuorumSystem.QuorumBuilding
{
	internal class CuckooRule : Protocol//, IQuorumBuilder
	{
		public event QbFinishHandler QbFinished;

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