// Mahdi Zamani
// University of New Mexico
// zamani@cs.MpcLib.edu

#include <iostream>
#include <ctime>
#include <NTL/ZZX.h>
#include "FheWrapper.h"
#include "FHEContext.h"
#include "FHE.h"
#include "timing.h"

using namespace MpcLib::Common::FiniteField;

long FheLib::FheWrapper::Init(long k, long r, long L, long c, long w, long d, long s)
{
	// L is the number of levels (see BGV11)
	//m = FindM(k, L, c, p, d, s, m, false);	// m is a specific modulus

	// initialize context
	context = new FHEcontext(m, p, r);

	ZZX G;	// defines the plaintext space
	if (d == 0)
		G = context->alMod.getFactorsOverZZ()[0];
	else
	{
		if (d == 1)
			G = ZZX(1, 1);	// the monomial X
		else
		{
			zz_pBak bak; bak.save();
			zz_p::init(p);
			G = to_ZZX(BuildIrred_zz_pX(d));
		}
	}

	// modify the context, adding primes to the modulus chain
	buildModChain(*context, L, c);

	// construct a secret key structure associated with the context
	secretKey = new FHESecKey(*context);

	// an upcast FHESecKey is a subclass of FHEPubKey
	publicKey = secretKey;

	// generate a secret key with Hamming weight w
	secretKey->GenSecKey(w);

	// compute key-switching matrices that we need
	addSome1DMatrices(*secretKey);

	// construct an Encrypted array object ea that is
	// associated with the given context and the polynomial G
	ea = new EncryptedArray(*context, G);

	// return the number of slots
	return ea->size();
}

FheLib::CtxtWrapper ^FheLib::FheWrapper::Encrypt(long long x)
{
	assert(ea);		// check object is initialized
	assert(mods->Length <= ea->size());

	vector<long> plaintext = FindMods(x, mods, 12);

	Ctxt *cipher = new Ctxt(*publicKey);
	ea->encrypt(*cipher, *publicKey, plaintext);

	// instantiate ref type on garbage-collected heap
	return gcnew CtxtWrapper(cipher);
}

long long FheLib::FheWrapper::Decrypt(FheLib::CtxtWrapper ^cipher)
{
	assert(ea != NULL);		// check object is initialized

	vector<long> plaintext;
	ea->decrypt(*(cipher->getCipher()), *secretKey, plaintext);
	// note that HELib currently works on int-size numbers (up to 2,147,483,647)

	assert(plaintext[10] == 0 && plaintext[11] == 0, 
		@"FHE parity error: Cipher corrupted! Decrease the number of operations or " + 
		"increase the bootstraping factor L.");

	for (int i = 0; i < mods->Length; i++)
		plaintext[i] %= mods[i];

	return SolveCrt(plaintext, mods);
}

vector<long> FheLib::FheWrapper::FindMods(long long x, cli::array<int> ^ms, int size)
{
	vector<long> v(size);
	for (int i = 0; i < ms->Length; i++)
		v[i] = (int)(x % ms[i]);
	return v;
}

long long FheLib::FheWrapper::SolveCrt(vector<long> v, cli::array<int> ^ms)
{
	long long x = 0, m = 1;
	for (int i = 0; i < ms->Length; i++)
		m *= mods[i];

	for (int i = 0; i < ms->Length; i++)
	{
		long long M = m / mods[i], y = 0;
		y = NumTheoryUtils::MultiplicativeInverse(M, (long long)mods[i]);
		x += v[i] * M * y; 
	}
	return x % m;
}
