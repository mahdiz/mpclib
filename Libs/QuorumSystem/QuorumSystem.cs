using MpcLib.Simulation;
using MpcLib.Simulation.Des;

namespace MpcLib.DistributedSystem.QuorumSystem
{
	/// <summary>
	/// Represents a quorum-based distributed system.
	/// </summary>
	public abstract class QuorumSystem<T> : AsyncSimController<T>
		where T : AsyncParty, new()
	{
		public QuorumSystem(EventSimulator s, int seed)
			: base(s, seed)
		{
		}
	}
}