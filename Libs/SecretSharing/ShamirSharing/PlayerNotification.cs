namespace MpcLib.SecretSharing
{
	public class PlayerNotification
	{
		public const int ConfirmationLen = 1;
		public readonly Confirmation Confirmation;

		public PlayerNotification(Confirmation conf)
		{
			Confirmation = conf;
		}

		public PlayerNotification()
		{
		}

		//public override byte[] writeToByteArray()
		//{
		//    var bs = new BitStream();
		//    writeToBitStreamNoHeader(bs);
		//    bs.close();
		//    return bs.ByteArray;
		//}

		//public override void writeToBitStreamNoHeader(BitStream bs)
		//{
		//    bs.writeMessageType(MessageType.MESSAGE);
		//    Debug.Assert(msg != null);
		//    bs.writeInt((int)msg, CONFIRMATION_LENGTH);
		//}

		//public override void loadFromByteArrayNoHeader(BitStream bs, int prime)
		//{
		//    msg = (Confirmation) bs.readInt(CONFIRMATION_LENGTH);
		//}
	}
}