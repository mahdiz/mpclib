using System.Collections.Generic;
using MpcLib.Common.FiniteField;
using MpcLib.SecretSharing;

namespace MpcLib.MpcProtocols.Bgw.Vss
{
	public class MultStepVerificationPoly : BgwShare
	{
		internal IList<Zp> RxPolynomial_Renamed;

		public MultStepVerificationPoly()
			: base(BgwShareType.MULT_STEP_VERIFY_POLY)
		{
		}

		public MultStepVerificationPoly(IList<Zp> RxPolynomial)
			: base(BgwShareType.MULT_STEP_VERIFY_POLY)
		{
			this.RxPolynomial_Renamed = RxPolynomial;
		}

		public virtual IList<Zp> RxPolynomial
		{
			get
			{
				return RxPolynomial_Renamed;
			}

			set
			{
				this.RxPolynomial_Renamed = value;
			}
		}

		//public override byte[] writeToByteArray()
		//{
		//    var bs = new BitStream();
		//    bs.writeMessageType(MessageType.MULT_STEP_VERIFY_POLY);
		//    writeToBitStreamNoHeader(bs);
		//    bs.close();
		//    return bs.ByteArray;
		//}

		//public override void loadFromByteArrayNoHeader(BitStream bs, int prime)
		//{
		//    if (bs.readBoolean())
		//        RxPolynomial_Renamed = bs.readList(prime);
		//}

		//public override void writeToBitStreamNoHeader(BitStream bs)
		//{
		//    bs.writeBoolean(RxPolynomial_Renamed != null);
		//    if (RxPolynomial_Renamed != null)
		//        bs.writeList(RxPolynomial_Renamed);
		//}

		public override bool Equals(object obj)
		{
			if (!(obj is MultStepVerificationPoly))
				return false;
			MultStepVerificationPoly second = (MultStepVerificationPoly)obj;
			if (RxPolynomial_Renamed == null && second.RxPolynomial_Renamed != null || RxPolynomial_Renamed != null && second.RxPolynomial_Renamed == null)
				return false;
			if (RxPolynomial_Renamed == null && second.RxPolynomial_Renamed == null)
				return true;
			if (RxPolynomial_Renamed.Count != second.RxPolynomial_Renamed.Count)
				return false;
			for (int i = 0; i < RxPolynomial_Renamed.Count; i++)
			{
				if (!(RxPolynomial_Renamed[i] == second.RxPolynomial_Renamed[i]))
					return false;
			}
			return true;
		}
	}
}