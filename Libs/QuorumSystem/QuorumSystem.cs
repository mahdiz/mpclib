using MpcLib.Simulation.Des;
namespace MpcLib.DistributedSystem.QuorumSystem
{
	/// <summary>
	/// Represents a quorum-based distributed system.
	/// </summary>
	public abstract class QuorumSystem<T> : DistributedSystem<T>
		where T : Entity, new()
	{
		public QuorumSystem(Simulator s, int seed)
			: base(s, seed)
		{
		}
	}
}