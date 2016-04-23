#include <sstream>
#include "MG.h"
#include "PolyCommitCommon.h"
#include "PolyCommitProofs.h"
#include "PcParams.h"

using namespace System;
using namespace System::Diagnostics;
using namespace System::Numerics;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace MpcLib::Common;
using namespace MpcLib::Common::FiniteField;

namespace MpcLib { namespace Commitments { namespace PolyCommitment
{
	public ref class PolyCommit
	{
	private:
		PcParams ^params;

		static String ^toString(const ZZ &a)
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
				} 
				while (!IsZero(b));

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

		static BigInteger toBigInt(const ZZ &zz)
		{
			//long n = NumBytes(zz);

			//unsigned char *buf = new unsigned char[n];
			//BytesFromZZ(buf, zz, n);

			//array<unsigned char>^ arr = gcnew array<unsigned char>(n);
			//Marshal::Copy(IntPtr((void*)buf), arr, 0, n);
			//delete buf;
			return BigInteger::Parse(toString(zz));
		}

		static BigZp ^toBigZp(const ZZ_p &zzp)
		{
			ZZ zz = conv<ZZ>(zzp);
			return gcnew BigZp(toBigInt(zzp.modulus()), toBigInt(zz));
		}

		static ZZ toZZ(BigInteger bi)
		{
			cli::array<unsigned char> ^arr = bi.ToByteArray();
			pin_ptr<unsigned char> ptr = &arr[0];
			return ZZFromBytes(ptr, arr->Length);
		}

		static ZZ_p toZZp(BigZp ^bi)
		{
			Debug::Assert(bi->Prime == toBigInt(ZZ_p::modulus()),
				"Fields are incompatible!");

			return conv<ZZ_p>(toZZ(bi->Value));
		}

		static void toZZpVec(cli::array<BigZp^> ^zpList, vec_ZZ_p &zzpVec)
		{
			zzpVec.SetLength(zpList->Length);
			for (int i = 0; i < zpList->Length; i++)
				zzpVec[i] = toZZp(zpList[i]);
		}

	public:
		PolyCommit()
		{
			params = nullptr;
		}

		/// <summary>
		/// Initializes PolyCommit parameters using a deterministic RNG.
		/// **WARNING** NOT SECURE! Use only for simulations!
		/// </summary>
		void Setup(int t, int seed)
		{
			pbc_random_set_deterministic(seed);

			// Init PolyCommit parameters
			params = gcnew PcParams(t);

			// Initialize the field
			ZZ_p::init(params->p->get_order());
		}

		/// <summary>
		/// Commits to a polynomial and returns the proof and witnesses.
		/// </summary>
		MG ^Commit(cli::array<BigZp^> ^coeffs, cli::array<BigZp^> ^iz,
			cli::array<MG^> ^%witnesses, cli::array<Byte> ^%proof, bool calcProof)
		{
			Debug::Assert(params != nullptr, "PolyCommit not initialized yet.");
			vec_ZZ_p zzpVec;

			// convert the coefficients vector into a polynomial
			toZZpVec(coeffs, zzpVec);
			ZZ_pX poly = conv<ZZ_pX>(zzpVec);

			PolyCommitter pc(params->p, poly);
			G1 C = pc.get_C_fast();		// computes the commitment value

			// compute witnesses
			witnesses = gcnew cli::array<MG^>(iz->Length);
			for (int i = 0; i < iz->Length; i++)
				witnesses[i] = gcnew MG(pc.createWitness(toZZp(iz[i])));

			if (calcProof)
			{
				// Prove knowledge of the polynomial 
				stringstream ss;
				PoK_poly_Prover prover(pc, C);
				prover.FS_proof(ss);

				string s = ss.str();
				proof = gcnew cli::array<Byte>(s.size());
				Marshal::Copy(IntPtr(&s[0]), proof, 0, s.size());
			}
			return gcnew MG(C);
		}

		bool VerifyProof(MG ^commitment, cli::array<Byte> ^proof)
		{
			Debug::Assert(params != nullptr, "PolyCommit not initialized yet.");
			stringstream ss;
			for each (Byte b in proof)
				ss << b;

			PoK_poly_Verifier verifier(params->p, *commitment->g);
			return verifier.FS_verify(ss);
		}

		bool VerifyEval(MG ^commitment, BigZp ^i, BigZp ^fi, MG ^wi)
		{
			Debug::Assert(params != nullptr, "PolyCommit not initialized yet.");
			return PolyCommitter::verifyEval(params->p, *commitment->g, toZZp(i), toZZp(fi), *wi->g);
		}

		static BigZp ^Eval(cli::array<BigZp^> ^coeffs, BigZp ^i)
		{
			vec_ZZ_p zzpVec;

			// convert the coefficients vector into a polynomial
			toZZpVec(coeffs, zzpVec);
			ZZ_pX poly = conv<ZZ_pX>(zzpVec);

			return toBigZp(eval(poly, toZZp(i)));
		}

		//void GenRandVector(vec_ZZ_p &vec, int len)
		//{
		//	vec.SetLength(len);
		//	for (int i = 0; i <= len; i++)
		//		vec[i] = random_ZZ_p();
		//}
	};
}
}
}