#ifndef __POLYCOMMITVERIFIER_H__
#define __POLYCOMMITVERIFIER_H__

#include "PolyCommitCommon.h"

namespace PolyCommitment
{
	bool verifyPoly(const PolyCommitParams &p, const G1 &C, const ZZ_pX &f);
	bool verifyVec(const PolyCommitParams &p, const G1 &C, const vec_ZZ_p &v);
	bool verifyEval(const PolyCommitParams *p, const G1 &C, const ZZ_p &i,
		const ZZ_p &fi, const G1 &witness);

	// verify a batch of evaluations (different polys, same evaluation point)
	bool verifyEval(const PolyCommitParams &p, const vector<G1> &C, const ZZ_p &i,
		const vec_ZZ_p &fi, const vector<G1> &witness);
}

#endif
