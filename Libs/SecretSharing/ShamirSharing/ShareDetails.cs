using System.Collections.Generic;
using MpcLib.Common.FiniteField;

namespace MpcLib.DistributedSystem.SecretSharing
{
	public class ShareDetails
	{
		public IList<Zp> RandomPolynomial { get; set; }

		public IList<Zp> CreatedShares { get; set; }

		public ShareDetails(IList<Zp> randomPolynomial, IList<Zp> createdShares)
		{
			RandomPolynomial = randomPolynomial;
			CreatedShares = createdShares;
		}
	}
}