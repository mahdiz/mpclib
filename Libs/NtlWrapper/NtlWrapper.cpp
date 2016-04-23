// This is the main DLL file.

#include "NtlWrapper.h"

#include <NTL/ZZ_pX.h>
#include <NTL/ZZX.h>
#include <NTL/vec_ZZ.h>

using namespace System::Numerics;
using namespace System::Collections::Generic;

using namespace NTL;

using namespace MpcLib::NtlWrapper;

ZZ NtlConverter::toZZ(BigInteger b)
{
	cli::array<unsigned char> ^arr = b.ToByteArray();
	pin_ptr<unsigned char> ptr = &arr[0];
	return ZZFromBytes(ptr, arr->Length);
}

BigInteger NtlConverter::toBigInteger(ZZ& z)
{
	return BigInteger::Parse(toString(z));
}

String ^ NtlConverter::toString(const ZZ &a)
{
	ZZ b;
	String ^s = "";
	Stack<long> S;
	long r, k;

	b = a;
	k = sign(b);

	if (k == 0)
		s += "0";
	else
	{
		if (k < 0)
		{
			s += "-";
			negate(b, b);
		}

		do
		{
			r = DivRem(b, b, 10);
			S.Push(r);
		} while (!IsZero(b));

		r = S.Pop();
		s += r;

		while (S.Count > 0)
		{
			r = S.Pop();
			s += r;
		}
		return s;
	}
}

BigInteger NtlFunctionality::ModSqrRoot(BigInteger a, BigInteger p)
{
	ZZ sqr = NtlConverter::toZZ(a), prime = NtlConverter::toZZ(p);
	return NtlConverter::toBigInteger(SqrRootMod(sqr, prime));
}
