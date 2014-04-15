namespace Unm.DistributedSystem.SecretSharing
{
	public class ShareObject : Share
	{
		public readonly Zp SharedSecret;

		public ShareObject(Zp sharedSecret)
			: base(ShareType.SIMPLE_ZP)
		{
			this.SharedSecret = sharedSecret;
		}

		public ShareObject()
			: base(ShareType.SIMPLE_ZP)
		{
		}

		public override string ToString()
		{
			return SharedSecret.ToString();
		}

		//public override byte[] writeToByteArray()
		//{
		//    var bs = new BitStream();
		//    bs.writeMessageType(MessageType.SIMPLE_ZP);
		//    writeToBitStreamNoHeader(bs);
		//    bs.close();
		//    return bs.ByteArray;
		//}

		//public override void writeToBitStreamNoHeader(BitStream bs)
		//{
		//    bs.writeBoolean(sharedSecret != null);
		//    if (sharedSecret != null)
		//        bs.writeInt(sharedSecret.Value, BitStream.LENGTH_OF_SECRET);

		//    bs.writeBoolean(gateIndex >= 0);
		//    if (gateIndex >= 0)
		//        bs.writeInt(gateIndex, BitStream.LENGTH_OF_GATE_INDEX);
		//}

		//public override void loadFromByteArrayNoHeader(BitStream bs, int prime)
		//{
		//    if (bs.readBoolean())
		//    {
		//        int shared = bs.readInt(BitStream.LENGTH_OF_SECRET);
		//var	sharedSecret = new Zp(prime, shared);
		//    }
		//    if (bs.readBoolean())
		//        gateIndex = bs.readInt(BitStream.LENGTH_OF_GATE_INDEX);
		//}
	}
}