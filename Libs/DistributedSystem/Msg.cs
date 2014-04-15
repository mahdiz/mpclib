using MpcLib.Common;
namespace MpcLib.DistributedSystem
{
	/// <summary>
	/// Represents a message sent/received to/from network entities.
	/// </summary>
	public abstract class Msg : ISizable
	{
		public virtual int SenderId { get; set; }
		public abstract int StageKey { get; }

#if DEBUG && SIMULATION
		private static ulong idGen = 0;

		/// <summary>
		/// Simulation-wide unique ID.
		/// </summary>
		public readonly ulong Id;

		public override string ToString()
		{
			return "ID=" + Id + ", From=" + SenderId;
		}

#endif

		public Msg()
		{
			SenderId = -1;

#if DEBUG && SIMULATION
			Id = idGen++;
#endif
		}

		public virtual int Size
		{
			get
			{
				return sizeof(int);
			}
		}
	}
}