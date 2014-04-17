using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpcLib.Common.FiniteField;
using MpcLib.MpcProtocols.Bgw.Vss;

namespace MpcLib.MpcProtocols.Bgw
{
	public class VerifShareMessage : ShareMsg<Zp>
	{
		public readonly int PlayerToVerify;

		/// <summary>
		/// Indicates whether the sender had received a bad polynomial. This is not a part of the key.
		/// </summary>
		public readonly bool ReceivedGoodPoly;

		public VerifShareMessage(BgwShare s, int playerToVerify, bool receivedGoodPoly)
			: base(s, Stage.VerificationReceive)
		{
			PlayerToVerify = playerToVerify;
			ReceivedGoodPoly = receivedGoodPoly;
		}
	}
}