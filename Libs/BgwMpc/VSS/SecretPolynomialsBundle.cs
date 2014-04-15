using System.Collections.Generic;

namespace MpcLib.DistributedSystem.Mpc.Bgw.Vss
{
	public class SecretPolynomialsBundle : ShareList<SecretPolynomials>
	{
		public SecretPolynomialsBundle(IList<SecretPolynomials> secretPolysList)
			: base(BgwShareType.ZPS_BUNDLE, secretPolysList)
		{
		}

		public SecretPolynomialsBundle()
			: base(BgwShareType.ZPS_BUNDLE)
		{
		}

		public override SecretPolynomials NewInstrance
		{
			get
			{
				return new SecretPolynomials();
			}
		}
	}
}