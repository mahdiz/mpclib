using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem.SecretSharing;

namespace MpcLib.DistributedSystem.Mpc.Bgw.Vss
{
	public class BgwShare : Share<Zp>
	{
		public readonly BgwShareType Type;

		public BgwShare(BgwShareType type)
		{
			Type = type;
		}

		public BgwShare(Zp s, BgwShareType type)
			: base(s)
		{
			Type = type;
		}

		public BgwShare(Zp s)
			: base(s)
		{
			Type = BgwShareType.SIMPLE_ZP;
		}
	}
}