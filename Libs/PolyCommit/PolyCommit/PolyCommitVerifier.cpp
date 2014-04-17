#include "PolyCommitVerifier.h"

#define LENGTH_OF_RANDOMNESS 20 // this gives about a 1-in-a-million error probability

using namespace MpcLib::Commitments::PolyCommitment;

bool verifyPoly(const PolyCommitParams *p, const G1 &C, const ZZ_pX &f)
{
    PolyCommitter com(p, f);
    G1 Cver = com.get_C();
    return (Cver == C);
}

bool verifyVec(const PolyCommitParams *p, const G1 &C, const vec_ZZ_p &v)
{
    ZZ_pX f = PolyCommitter::poly_from_vec(v, p->get_t() + 1);;
    return verifyPoly(p, C, f);
}

bool verifyEval(const PolyCommitParams *p, const G1 &C, const ZZ_p &i,
    const ZZ_p &fi, const G1 &witness)
{
    const Pairing &e = p->get_pairing();
    const G1 &g = p->get_galphai(0);
    const G2 &ghat = p->get_ghatalphai(0);
    const G2 &ghatalpha = p->get_ghatalphai(1);
    Zr ir = to_Zr(e, i);
    Zr fir = to_Zr(e, fi);
    GT LHS = e(C, ghat);
    GT RHS = e(witness, ghatalpha/(ghat^ir)) * e(g, ghat^fir);
    return (LHS == RHS);
}

bool verifyEval(const PolyCommitParams &p, const vector<G1> &C, const ZZ_p &i,
    const vec_ZZ_p &fi, const vector<G1> &witness)
{
    const Pairing &e = p.get_pairing();
    const G1 &g = p.get_galphai(0);
    const G2 &ghat = p.get_ghatalphai(0);
    const G2 &ghatalpha = p.get_ghatalphai(1);
    Zr ir = to_Zr(e, i);
    G1 C_prime(e, 1);
    G1 witness_prime(e, 1);
    Zr fir = to_Zr(e, to_ZZ_p(0));
    for (size_t i = 0; i < C.size(); i++)
	{
		long rnd = RandomBits_long(LENGTH_OF_RANDOMNESS);
		Zr t = to_Zr(e, to_ZZ_p(rnd));
		C_prime *= C[i]^t;
		witness_prime *= witness[i]^t;
		fir += t * to_Zr(e, fi[i]);
	}
    GT LHS = e(C_prime, ghat);
    GT RHS = e(witness_prime, ghatalpha/(ghat^ir)) * e(g, ghat^fir);
    return (LHS == RHS);
}

#ifdef TEST_VERIFIER
#include <iostream>
#include <fstream>

int main(int argc, char **argv)
{
    // Initialize the prng with some randomness from the kernel
    unsigned char randbuf[1024];
    ifstream urand("/dev/urandom");
    urand.read((char *)randbuf, sizeof(randbuf));
    urand.close();
    ZZ randzz = ZZFromBytes(randbuf, sizeof(randbuf));
    SetSeed(randzz);

    PolyCommitParams p;
    cin >> p;
    ZZ_p::init(p.get_order());

    ZZ_pX f;
    G1 C(p.get_pairing());
    // Receive the commitment
    cin >> C;

    // Receive the opening
    cin >> f;
    // Verify the opening
    bool res = verifyPoly(&p, C, f);
    cout << "Verify open  = " << res << "\n";

    // Receive the commitment to the second poly
    cin >> C;
    // Receive the claimed (i, f(i))
    ZZ_p i, fi;
    cin >> i >> fi;
    // Eat the newline
    char nl;
    cin.read(&nl, 1);
    // Receive the witness
    G1 w(p.get_pairing());
    cin >> w;

    // Verify the witness
    res = verifyEval(&p, C, i, fi, w);
    cout << "Verify eval  = " << res << "\n";

// TEST BATCH VERIFICATION
	vector<G1> C_vec;
	vector<ZZ_p> fi_vec;
	vector<G1> w_vec;
    cin >> C;
	C_vec.push_back(C);
    cin >> C;
	C_vec.push_back(C);
    
	cin >> i >> fi;
	G1 w3(p.get_pairing());
    // Eat the newline
    cin.read(&nl, 1);
    cin >> w3;
	fi_vec.push_back(fi);	
	w_vec.push_back(w3);	
	cin >> fi;
	G1 w4(p.get_pairing());
    // Eat the newline
    cin.read(&nl, 1);
    cin >> w4;
	fi_vec.push_back(fi);
	w_vec.push_back(w4);	
 
    return 0;
}
#endif
