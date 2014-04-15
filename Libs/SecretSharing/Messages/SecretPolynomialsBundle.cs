using System.Collections.Generic;

namespace Unm.DistributedSystem.SecretSharing
{
	public class SecretPolynomialsBundle : ShareList<SecretPolynomials>
	{
		public SecretPolynomialsBundle(IList<SecretPolynomials> secretPolysList)
			: base(ShareType.ZPS_BUNDLE, secretPolysList)
		{
		}

		public SecretPolynomialsBundle()
			: base(ShareType.ZPS_BUNDLE)
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