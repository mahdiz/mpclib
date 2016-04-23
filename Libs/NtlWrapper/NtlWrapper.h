// NtlWrapper.h

#include <NTL/ZZ.h>

#pragma once

using namespace System;
using namespace System::Numerics;
using namespace NTL;

namespace MpcLib {
	namespace NtlWrapper {

		public ref class NtlConverter
		{
		public:
			static ZZ toZZ(BigInteger b);
			static BigInteger toBigInteger(ZZ& z);
			static String ^ toString(const ZZ &a);
		};
		
		public ref class NtlFunctionality
		{
		public:
			static BigInteger ModSqrRoot(BigInteger a, BigInteger p);
		};
	}
}
