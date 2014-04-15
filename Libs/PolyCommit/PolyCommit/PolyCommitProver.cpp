#include "PolyCommitProver.h"

#ifdef TEST_PROVER
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

    int deg = argc > 1 ? atoi(argv[1]) : 5;

    PolyCommitParams p;
    cin >> p;
    ZZ_p::init(p.get_order());

    ZZ_pX f = random_ZZ_pX(deg+1);

    // Commit to a polynomial
    PolyCommitter com(&p, f);
    G1 C = com.get_C();
    cout << C;

    // Open the commitment
    cout << f;

    // Pick a different polynomial and commit to it
    ZZ_pX f2 = random_ZZ_pX(deg+1);
    PolyCommitter com2(&p, f2);
    G1 C2 = com2.get_C();
    G1 C2f = com2.get_C_fast();
    if (C2 == C2f) {
	cerr << "MATCH!\n";
    } else {
	cerr << "MISMATCH!\n";
	C2.dump(stderr, "C2 ", 10);
	C2f.dump(stderr, "C2f", 10);
    }
    cout << C2;

    // Compute a witness to its opening at 1
    ZZ_p i = to_ZZ_p(1);
    ZZ_p fi = eval(f2, i);
    G1 w = com2.createwitness(i);
    cout << i << " " << fi << "\n" << w;
    
// TEST BATCH VERIFICATION    
    // Create two polynomials and commit to them
	ZZ_pX f3 = random_ZZ_pX(deg+1);
	PolyCommitter com3(&p, f3);
    G1 C3 = com3.get_C();
    cout << C3;
	ZZ_pX f4 = random_ZZ_pX(deg+1);
	PolyCommitter com4(&p, f4);
    G1 C4 = com4.get_C();
    cout << C4;
    vector<PolyCommitter> polys;
    polys.push_back(com3);
    polys.push_back(com4);
    
    // Compute a witness to their openings at 1
    i = to_ZZ_p(3);
    ZZ_p fi3 = eval(f3, i);
    ZZ_p fi4 = eval(f4, i);
    vector<G1> w34 = PolyCommitter::createwitness(polys, i);
//    G1 w3 = com3.createwitness(i);
//    G1 w4 = com4.createwitness(i);
    //cout << i << " " << fi3 << "\n" << w3 << fi4 << "\n" << w4;
    cout << i << " " << fi3 << "\n" << w34[0] << fi4 << "\n" << w34[1];
    return 0;
}
#endif

#ifdef TIME_PROVER
#include <iostream>
#include <fstream>
#include <sys/time.h>

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
    int niter = argc > 2 ? atoi(argv[2]) : 100;

    PolyCommitParams p;
    cin >> p;
    ZZ_p::init(p.get_order());

    struct timeval st, et;
    gettimeofday(&st, NULL);

    for(int iter=0;iter<niter;++iter) {
	ZZ_pX f = random_ZZ_pX(deg+1);
	PolyCommitter com(&p, f);
	G1 C = com.get_C();
    }

    gettimeofday(&et, NULL);
    long dt = (et.tv_sec-st.tv_sec)*1000000 + (et.tv_usec-st.tv_usec);
    double avgt = (double)dt / (double)niter / 1000.0;
    cout << "norm: " << dt << " us / " << niter << " = " << avgt << " ms\n";

    gettimeofday(&st, NULL);

    for(int iter=0;iter<niter;++iter) {
	ZZ_pX f = random_ZZ_pX(deg+1);
	PolyCommitter com(&p, f);
	G1 C = com.get_C_fast();
    }

    gettimeofday(&et, NULL);
    dt = (et.tv_sec-st.tv_sec)*1000000 + (et.tv_usec-st.tv_usec);
    avgt = (double)dt / (double)niter / 1000.0;
    cout << "fast: " << dt << " us / " << niter << " = " << avgt << " ms\n";

    return 0;
}
#endif
