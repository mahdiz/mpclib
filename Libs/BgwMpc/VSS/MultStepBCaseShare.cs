using System;
using MpcLib.Common.FiniteField;

namespace MpcLib.MpcProtocols.Bgw.Vss
{
	public class MultStepBCaseShare : BgwShare
	{
		public MultStepBCaseShare(Zp aShare, Zp bShare, Zp abShare, Zp rShare)
			: base(BgwShareType.MULT_STEP_BCASE)
		{
			AShare = aShare;
			BShare = bShare;
			AbShare = abShare;
			RShare = rShare;
		}

		public MultStepBCaseShare()
			: base(BgwShareType.MULT_STEP_BCASE)
		{
		}

		public virtual Zp AShare { get; set; }

		public virtual Zp AbShare { get; set; }

		public virtual Zp BShare { get; set; }

		public virtual Zp RShare { get; set; }

		//public override byte[] writeToByteArray()
		//{
		//    var bs = new BitStream();
		//    bs.writeMessageType(MessageType.MULT_STEP_BCASE);
		//    writeToBitStreamNoHeader(bs);
		//    bs.close();
		//    return bs.ByteArray;
		//}

		//public override void writeToBitStreamNoHeader(BitStream bs)
		//{
		//    writeSecret(bs, aShare);
		//    writeSecret(bs, bShare);
		//    writeSecret(bs, abShare);
		//    writeSecret(bs, rShare);
		//}

		//public static MultStepBCaseShare readFromBitStreamNoHeader(BitStream bs, int prime)
		//{
		//    var multStepBCaseShare = new MultStepBCaseShare();
		//    multStepBCaseShare.loadFromByteArrayNoHeader(bs, prime);
		//    return multStepBCaseShare;
		//}

		//public override void loadFromByteArrayNoHeader(BitStream bs, int prime)
		//{
		//    aShare = readSecret(bs, prime);
		//    bShare = readSecret(bs, prime);
		//    abShare = readSecret(bs, prime);
		//    rShare = readSecret(bs, prime);
		//}

		//private void writeSecret(BitStream bs, Zp secret)
		//{
		//    bs.writeBoolean(secret != null);
		//    if (secret != null)
		//        bs.writeInt(secret.Value, BitStream.LENGTH_OF_SECRET);
		//}

		//private Zp readSecret(BitStream bs, int prime)
		//{
		//    if (bs.readBoolean())
		//        return new Zp(prime, bs.readInt(BitStream.LENGTH_OF_SECRET));
		//    else
		//        return null;
		//}

		//public override bool Equals(object obj)
		//{
		//    if (!(obj is MultStepBCaseShare))
		//        return false;
		//    MultStepBCaseShare second = (MultStepBCaseShare)obj;
		//    return compareSecrets(aShare, second.aShare) && compareSecrets(bShare, second.bShare) && compareSecrets(abShare, second.abShare) && compareSecrets(rShare, second.rShare);
		//}

		public static MultStepBCaseShare createRandom(int prime) //for testing
		{
			double d = (new Random(1)).NextDouble();
			if (d < 0.25)
				return null;
			var multStepBCaseShare = new MultStepBCaseShare();
			multStepBCaseShare.AShare = new Zp(prime, (int)Math.Floor(d * 20));
			multStepBCaseShare.BShare = new Zp(prime, (int)Math.Floor(d * 40));
			multStepBCaseShare.AbShare = new Zp(prime, (int)Math.Floor(d * 60));
			multStepBCaseShare.RShare = new Zp(prime, (int)Math.Floor(d * 80));
			return multStepBCaseShare;
		}
	}
}