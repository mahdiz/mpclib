#pragma once

#include "PolyCommitCommon.h"

namespace PolyCommitment
{
	public ref class PcParams
	{
	public:
		PolyCommitParams *p;

		PcParams(int t)
		{
			p = new PolyCommitParams(t - 1, true);
			p->create(string(POLYCOMMIT_SYM_DEFPARAMS));
		}

		~PcParams()
		{
			delete(p);
		}
	};
}