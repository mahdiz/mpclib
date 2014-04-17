#pragma once

#include "PolyCommitCommon.h"

namespace MpcLib { namespace Commitments { namespace PolyCommitment
{
	public ref class MG : MpcLib::Common::ISizable
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

		property int Size
		{
			virtual int get()
			{
				return g->getElementSize(true);
			}
		}
	};
} 
} 
}