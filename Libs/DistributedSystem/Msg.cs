using MpcLib.Common;
namespace MpcLib.DistributedSystem
{
    public enum MsgType
    {
        NotSet = 0,
        Share,
        Commit,
        Reconst,

        // For the majority filtering protocol
        Basic,

        // For share multiplication
        RandomizationReceive,

        /// <summary>
        /// A loopback message sent by each party in the synchronous setting to notify the end of the current round.
        /// This is to ensure that the party receives the inputs of "all" honest parties sent in the current round.
        /// </summary>
        NextRound,
        
        // A loopback message that is sent to notify a party that a subprotocol has completed.  The contents of the message should
        // be the result of the subprotocol
        SubProtocolCompleted,
        
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
				return sizeof(MsgType) + sizeof(long);
			}
		}
	}
}