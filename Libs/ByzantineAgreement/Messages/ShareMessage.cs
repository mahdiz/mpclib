using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem.SecretSharing;

namespace MpcLib.DistributedSystem.ByzantineAgreement
{
	public class ShareMessage : BroadcastMessage
	{
		public readonly Zp Share;

		public ShareMessage(BroadcastStage stage, Zp share, int k)
			: base(stage, k)
		{
			Share = share;
		}

		public override string ToString()
		{
			return base.ToString() + ", Share=" + Share.ToString();
		}
	}
}