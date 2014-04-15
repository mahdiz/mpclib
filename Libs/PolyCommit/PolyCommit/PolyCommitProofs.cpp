#include "PolyCommitProofs.h"
#include "PolyCommitProver.h"
#include "PolyCommitVerifier.h"
#include <string>
#include <fstream>
#include <time.h>
#include <time.h>
#include <openssl/sha.h>
using namespace PolyCommitment;

static string sha256(const string str)
{
	unsigned char hash[SHA256_DIGEST_LENGTH];
	SHA256_CTX sha256;
	SHA256_Init(&sha256);
	SHA256_Update(&sha256, str.c_str(), str.size());
	SHA256_Final(hash, &sha256);

	return string((const char *)hash, SHA256_DIGEST_LENGTH);
}

////////////  PROVER FUNCTIONS BEGIN  /////////////
void PoK_point_Prover::announce(ostream &os)
{
	if (announced)
	{
		throw runtime_error("announce() invoked twice.");
	}

	const PolyCommitParams *p = com.paramsp;
	const Pairing &e = p->get_pairing();
	const G1 &g = p->get_galphai(0);
	const G1 &ghat = p->get_galphai(0);
	const G1 &ghatalpha = p->get_galphai(1);
	Zr ir = to_Zr(e, index);

	// Compute the blinded witness
	gamma = random_ZZ_p();
	//if (gamma.isIdentity(true)) { } // blinding factor is zero!
	witness_prime = com.createWitness(index, gamma);

	announced = true;

	os << witness_prime;
	//    cout << "p:witness_prime=" << witness_prime << "\n";

	// Compute the random Pedersen commitment in GT
	s1 = random_ZZ_p();
	s2 = random_ZZ_p();
	Zr s1r = to_Zr(e, s1);
	Zr s2r = to_Zr(e, s2);
	//    LHS = e(witness_prime^s1r, ghatalpha/(ghat^ir)) * e(g^s2r, ghat);
	LHS = GT::pow2(e, e(witness_prime, ghatalpha / (ghat^ir)), s1r, e(g, ghat), s2r);

	os << LHS;
	//    cout << "p:LHS=" << LHS << "\n";
	os.flush();
}

void PoK_point_Prover::respond(ostream &os, const ZZ_p &chall)
{
	if (!announced)
	{
		throw runtime_error("respond() invoked before announce().");
	}
	if (responded)
	{
		throw runtime_error("respond() invoked twice.");
	}

	ZZ_p fi = eval(com.f, index);

	// Compute the responses
	ZZ_p u1 = s1 - chall / gamma;
	ZZ_p u2 = s2 - fi * chall;

	responded = true;

	//    cout << "p:chall=" << chall << "\n";
	//    cout << "p:u1 = " << u1 << "\n";
	//    cout << "p:u2 = " << u2 << "\n";
	os << u1 << ' ' << u2;
	os.flush();
}

ZZ_p PoK_point_Prover::FS_challenge(const string nonce)
{
	if (!announced)
	{
		throw runtime_error("FS_challenge() invoked before announce().");
	}
	stringstream ss;
	ss << nonce << Cval << index << witness_prime << LHS;
	string FS_hash = sha256(ss.str());
	//    cout << "p:FS_hash(chall)=" << FS_hash << "\n";
	ZZ_p chall = to_ZZ_p(ZZFromBytes((const unsigned char *)FS_hash.c_str(), SHA256_DIGEST_LENGTH));

	return chall;
}

// Polynomial prover

ZZ_p PoK_poly_Prover::FS_index(const G1 &Cval, const string nonce)
{
	stringstream ss;
	ss << nonce << Cval;
	string FS_hash = sha256(ss.str());
	//    cout << "p:FS_hash(idx)=" << FS_hash << "\n";
	ZZ_p index = to_ZZ_p(ZZFromBytes((const unsigned char*)FS_hash.c_str(), SHA256_DIGEST_LENGTH));

	return index;
}

ZZ_p PoK_poly_Prover::FS_index(const string nonce)
{
	return FS_index(Cval, nonce);
}

void PoK_poly_Prover::assert_degree(ostream &os, unsigned int d)
{
	const PolyCommitParams *p = com.paramsp;
	if (deg(com.f) > d)
	{
		throw runtime_error("asserted degree is too low.");
	}
	// left shift of n corresponds to multiplication by x^n
	ZZ_pX f = com.f;
	//    cout << "p:f=" << f << "\n";
	ZZ_pX fd = LeftShift(f, p->get_t() - d);
	//    cout << "p:fd=" << fd << "\n";
	os << PolyCommitter(p, fd).get_C_fast();
}

////////////   PROVER FUNCTIONS END   /////////////


//////////// VERIFIER FUNCTIONS BEGIN /////////////
ZZ_p PoK_point_Verifier::challenge(istream &is)
{
	if (challenged)
	{
		throw runtime_error("[FS_]challenge() invoked twice.");
	}
	// Receive the announcement
	is >> witness_prime >> LHS;
	//    cout << "v:witness_prime=" << witness_prime << "\n";
	//    cout << "v:LHS=" << LHS << "\n";

	// and then pick a random challenge
	chall = random_ZZ_p();

	challenged = true;

	return chall;
}

bool PoK_point_Verifier::verify(istream &is)
{
	if (!challenged)
	{
		throw runtime_error("verify() invoked before challenge() or FS_challenge().");
	}
	const Pairing &e = paramsp->get_pairing();
	const G1 &g = paramsp->get_galphai(0);
	const G1 &ghat = paramsp->get_galphai(0);
	const G1 &ghatalpha = paramsp->get_galphai(1);
	Zr ir = to_Zr(e, index);

	// Receive the responses
	ZZ_p u1, u2;
	is >> u1 >> u2;
	//    cout << "v:chall=" << chall << "\n";
	//    cout << "v:u1 = " << u1 << "\n";
	//    cout << "v:u2 = " << u2 << "\n";

	Zr u1r = to_Zr(e, u1);
	Zr u2r = to_Zr(e, u2);
	Zr challr = to_Zr(e, chall);

	// Check the verification equation
	//    GT RHS = e(witness_prime^u1r, ghatalpha/(ghat^ir)) * e((g^u2r) * (C^challr), ghat);
	//    GT RHS = e(witness_prime^u1r, ghatalpha/(ghat^ir)) * e(G1::pow2(e, g, u2r, C, challr), ghat);
	//    GT RHS = (e(witness_prime, ghatalpha/(ghat^ir))^u1r) * e(G1::pow2(e, g, u2r, C, challr), ghat);
	GT RHS = GT::pow3(e, e(witness_prime, ghatalpha / (ghat^ir)), u1r, e(g, ghat), u2r, e(C, ghat), challr);

	return (LHS == RHS);
}

ZZ_p PoK_point_Verifier::fake(ostream &aos, ostream &ros)
{
	ZZ_p chall = random_ZZ_p();
	fake(aos, ros, chall);

	return chall;
}

void PoK_point_Verifier::fake(ostream &aos, ostream &ros, const ZZ_p &chall)
{
	const Pairing &e = paramsp->get_pairing();
	const G1 g = paramsp->get_galphai(0);
	const G1 ghat = paramsp->get_galphai(0);
	const G1 ghatalpha = paramsp->get_galphai(1);

	this->chall = chall;

	witness_prime = G1(e, true);
	ZZ_p u1 = random_ZZ_p();
	ZZ_p u2 = random_ZZ_p();
	Zr u1r = to_Zr(e, u1);
	Zr u2r = to_Zr(e, u2);
	Zr ir = to_Zr(e, index);
	Zr challr = to_Zr(e, chall);

	LHS = GT::pow3(e, e(witness_prime, ghatalpha / (ghat^ir)), u1r, e(g, ghat), u2r, e(C, ghat), challr);
	aos << witness_prime << LHS;
	aos.flush();

	ros << u1 << ' ' << u2;
	ros.flush();
}

ZZ_p PoK_point_Verifier::FS_challenge(istream &is, const string nonce)
{
	if (challenged)
	{
		throw runtime_error("[FS_]challenge() invoked twice.");
	}
	// Receive the announcement
	is >> witness_prime >> LHS;
	//    cout << "v:witness_prime=" << witness_prime << "\n";
	//    cout << "v:LHS=" << LHS << "\n";

	stringstream ss;
	ss << nonce << C << index << witness_prime << LHS;
	string FS_hash = sha256(ss.str());
	//    cout << "v:FS_hash(chall)=" << FS_hash << "\n";
	chall = to_ZZ_p(ZZFromBytes((const unsigned char*)FS_hash.c_str(), SHA256_DIGEST_LENGTH));

	challenged = true;

	return chall;
}

// Polynomial verifier

bool PoK_poly_Verifier::verify_assert_degree(istream &is, unsigned int d)
{
	const Pairing &e = paramsp->get_pairing();
	const G1 ghat = paramsp->get_galphai(0);
	unsigned int deg = paramsp->get_t() - d;
	const G1 ghatalphad = paramsp->get_galphai(deg);

	G1 C_d(e);
	is >> C_d;

	return (e(C_d, ghat) == e(C, ghatalphad));
}

ZZ_p PoK_poly_Verifier::FS_index(const G1 &C, const string nonce)
{
	stringstream ss;
	ss << nonce << C;
	string FS_hash = sha256(ss.str());
	//    cout << "f:FS_hash(idx)=" << FS_hash << "\n";
	ZZ_p index = to_ZZ_p(ZZFromBytes((const unsigned char*)FS_hash.c_str(), SHA256_DIGEST_LENGTH));

	return index;
}

ZZ_p PoK_poly_Verifier::FS_index(const string nonce)
{
	return FS_index(C, nonce);
}

////////////  VERIFIER FUNCTIONS END  /////////////

#ifdef TEST_ZKPS

static double elapsed(timespec &start, timespec &stop)
{
	long secs, nsecs;
	if (stop.tv_nsec < start.tv_nsec)
	{
		secs = stop.tv_sec - start.tv_sec - 1;
		nsecs = stop.tv_nsec - start.tv_nsec + 1E9;
	}
	else
	{
		secs = stop.tv_sec - start.tv_sec;
		nsecs = stop.tv_nsec - start.tv_nsec;
	}
	return secs + (nsecs * 1E-9);
}

int main(int argc, char **argv)
{
	// Initialize the prng with some randomness from the kernel
	unsigned char randbuf[1024];
	ifstream urand("/dev/urandom");
	urand.read((char *)randbuf, sizeof(randbuf));
	urand.close();
	ZZ randzz = ZZFromBytes(randbuf, sizeof(randbuf));
	SetSeed(randzz);

	int deg = argc > 1 ? atoi(argv[1]) : 5;

	PolyCommitParams p(true);
	cin >> p;
	ZZ_p::init(p.get_order());

#define TRIALS 100
#define REPETITIONS_PER_TRIAL 200
#define SOUNDNESS 40

	vec_ZZ_p u;
	vec_ZZ_p S;
	u.SetLength(deg + 1);
	S.SetLength(deg / 5);
	for (int i=0;i<TRIALS;++i)
	{
		// TEST 1: Time to commit to a random vector
		for (int i=0;i<=deg;i++) { u[i] = random_ZZ_p(); }
		PolyCommitter C1(&p, u);
		struct timespec start, stop;
		clock_gettime(CLOCK_REALTIME, &start);
		for (int i=0;i<REPETITIONS_PER_TRIAL;i++)
		{
			G1 C = C1.get_C_fast();
		}
		clock_gettime(CLOCK_REALTIME, &stop);
		cout << elapsed(start, stop) << "\t";

		// TEST 2: Time to commit to a short vector
		for (int i=1;i<=deg;i++) { u[i] = to_ZZ_p(RandomBits_ZZ(SOUNDNESS)); }
		PolyCommitter C2(&p, u);
		clock_gettime(CLOCK_REALTIME, &start);
		for (int i=0;i<REPETITIONS_PER_TRIAL;i++)
		{
			G1 C = C2.get_C_fast();
		}
		clock_gettime(CLOCK_REALTIME, &stop);
		cout << elapsed(start, stop) << "\t";

		// TEST 3: Time to commit to a set
		for (int i=0;i<deg/5;i++) { S[i] = random_ZZ_p(); }
		PolyCommitter S1(&p, BuildFromRoots(S));
		clock_gettime(CLOCK_REALTIME, &start);
		for (int i=0;i<REPETITIONS_PER_TRIAL;i++)
		{
			G1 C = S1.get_C_fast();
		}
		clock_gettime(CLOCK_REALTIME, &stop);
		cout << elapsed(start, stop) << "\t";

		// TEST 4: Time to prove knowledge of a polynomial
		G1 C = C2.get_C_fast();
		clock_gettime(CLOCK_REALTIME, &start);
		for (int i=0;i<REPETITIONS_PER_TRIAL;i++)
		{
			stringstream ss;
			PoK_poly_Prover prover(C2, C);
			prover.FS_proof(ss);
		}
		clock_gettime(CLOCK_REALTIME, &stop);
		cout << elapsed(start, stop) << "\t";

		// TEST 5: Time to verify the above proof
		stringstream ss;
		PoK_poly_Prover prover1(C2, C);
		prover1.FS_proof(ss);
		string str = ss.str();
		clock_gettime(CLOCK_REALTIME, &start);
		for (int i=0;i<REPETITIONS_PER_TRIAL;i++)
		{
			stringstream ss2;
			ss2.str(str);
			PoK_poly_Verifier verifier(&p, C);
			//ss.seekp(pos);
			if (!verifier.FS_verify(ss2)) { throw runtime_error("FS_verify failed"); }
		}
		clock_gettime(CLOCK_REALTIME, &stop);
		cout << elapsed(start, stop) << "\t";

		// TEST 6: Time to prove that a polynomial has bounded degree
		PoK_poly_Prover prover(S1, S1.get_C_fast());
		clock_gettime(CLOCK_REALTIME, &start);
		for (int i=0;i<REPETITIONS_PER_TRIAL;i++)
		{
			ss.str("");
			prover.assert_degree(ss, deg / 5);
		}
		clock_gettime(CLOCK_REALTIME, &stop);
		cout << elapsed(start, stop) << "\t";

		// TEST 7: Time to verify that a polynomial has bounded degree
		PoK_poly_Verifier verifier(&p, S1.get_C_fast());
		str = ss.str();
		clock_gettime(CLOCK_REALTIME, &start);
		for (int i=0;i<REPETITIONS_PER_TRIAL;i++)
		{
			stringstream ss2;
			ss2.str(str);
			if (!verifier.verify_assert_degree(ss2, deg / 5)) { throw runtime_error("verify_assert_degree failed"); }
		}
		clock_gettime(CLOCK_REALTIME, &stop);
		cout << elapsed(start, stop) << "\n";

		// TEST 8: Time to compute two pairings (for the disjointness test)
		//	const Pairing &e = p.get_pairing();
		//	start_time = clock();
		//	for (int i=0;i<REPETITIONS_PER_TRIAL;i++)
		//	{
		//	   bool b = e(C,C) == e(C,C);
		//	}
		//	cout << (clock()-start_time)/(double)CLOCKS_PER_SEC << "\n";

	}

	return 0;
}

#endif
