using MpcLib.Common;
namespace MpcLib.DistributedSystem
{
    public enum MsgType
    {
        NotSet = 0,
        Share,
        Commit,
        Reconst,

        /// <summary>
        /// A loopback message sent by each party in the synchronous setting to notify the end of the current round.
        /// This is to ensure that the party receives the inputs of "all" honest parties sent in the current round.
        /// </summary>
        NextRound,
    }

	/// <summary>
	/// Represents an abstract message sent/received to/from network parties.
	/// </summary>
	public class Msg : ISizable
	{
        public MsgType Type;

		public Msg()
		{
		}

        public Msg(MsgType type)
        {
            Type = type;
        }

        public virtual int Size
		{
			get
			{
				return sizeof(MsgType);
			}
		}
	}
}