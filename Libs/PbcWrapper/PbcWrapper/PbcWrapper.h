// Mahdi Zamani
// University of New Mexico
// zamani@cs.unm.edu

#pragma once

#include "pbc.h"

namespace Pbc
{
	/// <summary>
	/// A C++/CLI wrapper for Pairing-Based Crypto (PBC) libray of Ben Lynn.
	/// </summary>		
	public ref class PbcWrapper
	{
	private:

	public:
		PbcWrapper()
		{
		}

		~PbcWrapper()
		{
		}

		int Init();
	};
}
