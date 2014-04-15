using System.Collections.Generic;

namespace Unm.DistributedSystem.SecretSharing
{
	public class MultStepBCaseShareBundle : ShareList<MultStepBCaseShare>
	{
		public MultStepBCaseShareBundle()
			: base(ShareType.MULT_STEP_BCASE_BUNDLE)
		{
		}

		public MultStepBCaseShareBundle(IList<MultStepBCaseShare> bCaseShareBundle)
			: base(ShareType.MULT_STEP_BCASE_BUNDLE, bCaseShareBundle)
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