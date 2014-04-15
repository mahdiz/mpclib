// Mahdi Zamani
// University of New Mexico
// zamani@cs.MpcLib.edu

#pragma once

using namespace System;
#include "Ctxt.h"

namespace FheLib
{
	public ref class CtxtWrapper
	{
	private:
		Ctxt *cipher;

	public:
		CtxtWrapper(Ctxt *c)
		{
			assert(c != NULL);
			cipher = c;
		}

		const Ctxt *getCipher()
		{
			return cipher;
		}

		void Add(CtxtWrapper ^op)
		{
			cipher->addCtxt(*(op->cipher));
		}

		void Multiply(CtxtWrapper ^op)
		{
			(*cipher) *= *(op->cipher);
		}

		void Negate()
		{
			cipher->negate();
		}

		static CtxtWrapper ^operator +=(CtxtWrapper ^op1, CtxtWrapper ^op2)
		{
			op1->cipher->addCtxt(*(op2->cipher));
			return op1;
		}
	};
}
