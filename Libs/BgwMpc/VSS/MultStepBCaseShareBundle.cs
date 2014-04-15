using System.Collections.Generic;
using MpcLib.DistributedSystem.SecretSharing;

namespace MpcLib.DistributedSystem.Mpc.Bgw.Vss
{
	public class MultStepBCaseShareBundle : ShareList<MultStepBCaseShare>
	{
		public MultStepBCaseShareBundle()
			: base(BgwShareType.MULT_STEP_BCASE_BUNDLE)
		{
		}

		public MultStepBCaseShareBundle(IList<MultStepBCaseShare> bCaseShareBundle)
			: base(BgwShareType.MULT_STEP_BCASE_BUNDLE, bCaseShareBundle)
		{
		}

		public override MultStepBCaseShare NewInstrance
		{
			get
			{
				return new MultStepBCaseShare();
			}
		}
	}
}