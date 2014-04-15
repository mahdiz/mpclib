using MpcLib.Common;
using MpcLib.DistributedSystem.SecretSharing;

namespace MpcLib.DistributedSystem.Mpc.Dkms
{
	public class ShareMsg<T> : DkmsMsg where T : ISizable
	{
		public readonly Share<T> Share;

		public ShareMsg(Share<T> share, DkmsKey key)
			: base(key)
		{
			Share = share;
		}

		public override string ToString()
		{
			return base.ToString() + ", Share=" + Share.ToString();
		}
	}
}