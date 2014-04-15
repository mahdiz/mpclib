#pragma once

#include "PolyCommitCommon.h"

namespace PolyCommitment
{
	public ref class MG
	{
	public:
		G1 *g;

		MG(G1 &x)
		{
			g = new G1(x);
		}

		~MG()
		{
			delete g;
		}
	};
}