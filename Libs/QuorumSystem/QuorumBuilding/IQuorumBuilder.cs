using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MpcLib.DistributedSystem.QuorumSystem.QuorumBuilding
{
	public interface IQuorumBuilder
	{
		event QbFinishHandler QbFinished;

		void Run();
	}
}
