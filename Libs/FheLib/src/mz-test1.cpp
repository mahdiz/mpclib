// Mahdi Zamani
// University of New Mexico
// zamani@cs.MpcLib.edu

#include <iostream>
#include <ctime>
#include <NTL/ZZX.h>
#include "FHEContext.h"
#include "FHE.h"
#include "EncryptedArray.h"
#include "timing.h"

using namespace NTL_NAMESPACE;

ZZX makeIrredPoly_mz(long p, long d)
{
	assert(d >= 1);
	assert(ProbPrime(p));

	if (d == 1) return ZZX(1, 1); // the monomial X

	zz_pBak bak; bak.save();
	zz_p::init(p);
	return to_ZZX(BuildIrred_zz_pX(d));
}

int main_mz(int argc, char** argv)
{
	const long k = 80;		// security parameter [default=80]
	const long p = 2;		// plaintext base [default=2]
	const long r = 1;		// lifting [default=1]
	const long L = 4;		// number of primes in the modulus chai [default=4]
	const long c = 2;		// number of columns in the key-switching matrices [default=2]
	const long w = 64;		// Hamming weight of secret key [default=64]
	const long d = 1;		// degree of the field extension [default=1]
	const long s = 4;		// minimum number of slots [default=4]

	cout << "Program started...\n";
	clock_t counter = std::clock();

	long m = FindM(k, L, c, p, d, s, 0, true);	// a specific modulus

	// initialize context
	FHEcontext context(m, p, r);

	ZZX G;	// defines the plaintext space
	if (d == 0)
		G = context.alMod.getFactorsOverZZ()[0];
	else
		G = makeIrredPoly_mz(p, d);

	// modify the context, adding primes to the modulus chain
	buildModChain(context, L, c);

	// construct a secret key structure associated with the context
	FHESecKey secretKey(context);

	// an "upcast": FHESecKey is a subclass of FHEPubKey
	const FHEPubKey& publicKey = secretKey;

	// generate a secret key with Hamming weight w
	secretKey.GenSecKey(w);

	// compute key-switching matrices that we need
	addSome1DMatrices(secretKey);

	// construct an Encrypted array object ea that is
	// associated with the given context and the polynomial G
	EncryptedArray ea(context, G);

	// number of plaintext slots
	long nslots = ea.size();

	// PlaintextArray objects associated with the given EncryptedArray ea
	PlaintextArray p0(ea);
	PlaintextArray p1(ea);
	PlaintextArray p2(ea);
	PlaintextArray p3(ea);

	// generate random plaintexts: slots initialized with random elements of Z[X]/(G,p^r)
	p0.random();
	p1.random();
	p2.random();
	p3.random();

	// construct ciphertexts associated with the given public key
	Ctxt c0(publicKey), c1(publicKey), c2(publicKey), c3(publicKey);

	// encrypt each PlaintextArray
	ea.encrypt(c0, publicKey, p0);
	ea.encrypt(c1, publicKey, p1);
	ea.encrypt(c2, publicKey, p2);
	ea.encrypt(c3, publicKey, p3);

	// two random constants
	PlaintextArray const1(ea);
	PlaintextArray const2(ea);

	const1.random();
	const2.random();

	// Perform some simple computations directly on the plaintext arrays
	p1.mul(p0);		// p1 = p1 * p0 (slot-wise modulo G)
	p0.add(const1);	// p0 = p0 + const1
	p2.mul(const2);	// p2 = p2 * const2
	p2.add(p1);		// p2 = p2 + p1
	p1.negate();	// p1 = - p1
	p3.mul(p2);		// p3 = p3 * p2
	p0.sub(p3);		// p0 = p0 - p3

	// Perform the same operations on the ciphertexts
	ZZX const1_poly, const2_poly;
	ea.encode(const1_poly, const1);
	ea.encode(const2_poly, const2);

	// encode const1 and const2 as plaintext polynomials
	c1.multiplyBy(c0);				// c1 = c1 * c0
	c0.addConstant(const1_poly);	// c0 = c0 + const1
	c2.multByConstant(const2_poly); // c2 = c2 * const2
	c2 += c1;						// c2 = c2 + c1
	c1.negate();					// c1 = - c1
	c3.multiplyBy(c2);				// c3 = c3 * c2
	c0 -= c3;						// c0 = c0 - c3

	// Decrypt the ciphertexts and compare
	PlaintextArray pp0(ea);
	PlaintextArray pp1(ea);
	PlaintextArray pp2(ea);
	PlaintextArray pp3(ea);

	ea.decrypt(c0, secretKey, pp0);
	ea.decrypt(c1, secretKey, pp1);
	ea.decrypt(c2, secretKey, pp2);
	ea.decrypt(c3, secretKey, pp3);

	if (!pp0.equals(p0)) cout << "Oops! 0\n";
	if (!pp1.equals(p1)) cout << "Oops! 1\n";
	if (!pp2.equals(p2)) cout << "Oops! 2\n";
	if (!pp3.equals(p3)) cout << "Oops! 3\n";

	clock_t ec = counter + std::clock();
	cout << "Elapsed time: " << ((double) ec)/CLOCKS_PER_SEC << " sec" << endl;
	cout << "Program terminating...";

	return 0;
}