// Mahdi Zamani
// University of New Mexico
// zamani@cs.MpcLib.edu

#pragma once

using namespace System;
#include "Ctxt.h"
#include "CtxtWrapper.h"
#include "EncryptedArray.h" 

namespace FheLib
{
	/// <summary>
	/// A C++/CLI wrapper for HELib.
	/// </summary>		
	public ref class FheWrapper
	{
	private:
		FHEcontext *context;
		FHESecKey *secretKey;
		FHEPubKey *publicKey;
		EncryptedArray *ea;
		static const long m = 35;				// 37 for 1 slot, 33 for 10 slots, 35 for 12 slots
		static const long p = 1073741789;		// largest 30-bit prime
		static cli::array<int> ^mods = { 11, 13, 17, 19, 23, 25, 27, 28, 29, 31 };
		// This set pairwise coprime numbers makes it possible to supports 
		// integer operations up to 18,050,444,111,700 (about 2^44)
		// Note that we can multiply at most 6 numbers using this CRT-based 
		// method since 31^6 is the largest power of 31 smaller than 1073741789.
		// 1073741789 is the field size of FHE.
		// With 12 slots and 10 moduli, two slots are used for parity check. For 
		// field size 1073741789, the parity error probability is less than 8.6e-19.
	
	public:
		FheWrapper()
		{
			ea = NULL;
		}

		~FheWrapper()
		{
			if (ea) free(ea);
			if (secretKey) free(secretKey);
			if (context) free(context);
		}

		long Init(long k, long r, long L, long c, long w, long d, long s);
		CtxtWrapper ^Encrypt(long long x);
		long long Decrypt(CtxtWrapper ^cipher);
		static vector<long> FindMods(long long x, cli::array<int> ^ms, int size);

		/// <summary>
		/// Solves the Chinese Remainder Theorem problem.
		/// </summary>		
		static long long SolveCrt(vector<long> v, cli::array<int> ^ms);
	};
}
