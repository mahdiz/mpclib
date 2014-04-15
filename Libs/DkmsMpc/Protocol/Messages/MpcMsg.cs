namespace MpcLib.DistributedSystem.Mpc.Dkms
{
	public class MpcMsg : DkmsMsg
	{
		/// <summary>
		/// Tells in which gate the receiver of the message is (remember that a player can be in several gates)
		/// </summary>
		public readonly int ToGateId;

		public readonly int AnchorId;
		public readonly Msg InnerMessage;

		//public override int SenderId
		//{
		//    set
		//    {
		//        base.SenderId = value;
		//        if (InnerMessage != null)
		//            InnerMessage.SenderId = (ToGateId << 16) + value;		// TODO: HWSMPC VIRTUAL ADDRESS
		//    }
		//}

		public MpcMsg(int toGateId, int anchorId, Msg innerMessage)
			: base(new MpcKey(toGateId, anchorId))
		{
			ToGateId = toGateId;
			AnchorId = anchorId;
			InnerMessage = innerMessage;
		}

		public override int StageKey
		{
			get { return (int)Stage.Mpc; }
		}
	}
}