using MpcLib.Common;
namespace MpcLib.DistributedSystem
{
	/// <summary>
	/// Represents a message sent/received to/from network parties.
	/// </summary>
	public abstract class Msg : ISizable
	{
		public virtual int SenderId { get; set; }
		public abstract int StageKey { get; }
		public ProtocolIds ProtocolId = ProtocolIds.NotSet;

#if DEBUG && SIM
		private static ulong idGen = 0;

		/// <summary>
		/// Simulation-wide unique ID.
		/// </summary>
		public readonly ulong Id;

		public override string ToString()
		{
			return "From=" + SenderId + ", Protocol=" + ProtocolId;
		}

#endif

		public Msg()
		{
			SenderId = -1;

#if DEBUG && SIM
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