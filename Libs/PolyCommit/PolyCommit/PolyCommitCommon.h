#ifndef __POLYCOMMITCOMMON_H__
#define __POLYCOMMITCOMMON_H__

#include <iostream>
#include <string>
#include <vector>
#include <NTL/ZZ_pX.h>
#include <NTL/ZZX.h>
#include <NTL/vec_ZZ.h>
#include <stdexcept>
#include <stdint.h>
#include "PbcWrapper\PbcWapper.h"

#define POLYCOMMIT_MODE_POLY 0
#define POLYCOMMIT_MODE_VEC 1
#define POLYCOMMIT_WINDOW_DEFAULT 12

#define POLYCOMMIT_SYM_DEFPARAMS \
	"type a\n"\
	"q 2149279669255358467807031928884602064965849607417268878942578020380711849468507704854756169974266156283454009766052915551928876758979436718043801993006923\n"\
	"h 2941193476968633928915514572756480069153912946528476089236551260848359597655536307119448058353958150131532\n"\
	"r 730750862221594424981965739670091261094297337857\n"\
	"exp2 159\n"\
	"exp1 135\n"\
	"sign1 1\n"\
	"sign0 1\n"

#define POLYCOMMIT_ASYM_DEFPARAMS \
	"type d\n"\
	"q 34155376728415643485253199952223120817480971620809317\n"\
	"n 34155376728415643485253199767411387617406135340735963\n"\
	"h 14079\n"\
	"r 2425980306017163398341728799446792216592523285797\n"\
	"a 16998822741009851476858453412953856547198847906791629\n"\
	"b 32465094389155591141149001016286335587250188710863434\n"\
	"k 6\n"\
	"nk 1587648945903454419702294240696669428988257567004066667807817123624472417955040581724182593942606411788830338483111739923094122455965212711863453680770344432635484301107088581083365986465995771210066008136141674961980627026805959407325799760861946153887760541539723935858058246026804124025609796222220660565141631296\n"\
	"hk 269761481129543709905880358277500023395314481104084120305961073572355019695601034021874289343986546858568479155991309118249358857985438172361312160865276114790621806863462094233703391155493997120107408194663831963226944\n"\
	"coeff0 26826430320383533291086636892391482089885019715604831\n"\
	"coeff1 10041240416257104004698194348998922958451952509010108\n"\
	"coeff2 6606091442536002616272024969858190106364268669962931\n"\
	"nqr 4719190100394795070479603407875910135476373878231286\n"

#define LENGTH_OF_RANDOMNESS 20 // this gives about a 1-in-a-million error probability

using namespace std;
using namespace NTL;

namespace PolyCommitment
{
	class PolyCommitParams
	{
		friend istream& operator>>(istream &is, PolyCommitParams &params);
		friend ostream& operator<<(ostream &os, PolyCommitParams &params);
		friend class PolyCommitter;

	public:
		Pairing pairing;
		vector<G1> galphai;
		vector<G2> ghatalphai;
		vector<G1> glambdai;
		ZZ order;
		unsigned int t, t_hat;
		G1 h;					//Need a random h
		G1 halpha;				//h^alpha
		bool compute_Lagrange;
		int window_width;

		vector< vector<G1> > precomp_products;
		vector< vector<G1> > lambda_precomp_products;

		PolyCommitParams(unsigned int t = 3, bool compute_Lagrange = false, int window_width = POLYCOMMIT_WINDOW_DEFAULT)
			: compute_Lagrange(compute_Lagrange)
		{
			this->t = t;
			if (t % window_width == window_width - 1)
				this->window_width = window_width - 1;
			else
				this->window_width = window_width;
		}

		void create(const string pairingparams);
		static void create(ostream &os, const string pairingparams, unsigned int t);

		const Pairing &get_pairing() const { return pairing; }
		unsigned int get_t() const { return t; }
		const G1 &get_galphai(int i) const { return galphai[i]; }
		const G2 &get_ghatalphai(int i) const { return ghatalphai[i]; }
		const G1 &get_glambdai(int i) const { return glambdai[i]; }
		const G1 &get_h() const { return h; }
		const G1 &get_halpha() const { return halpha; }
		const ZZ &get_order() const { return order; }
	};

	class PolyCommitter
	{
		friend class PoK_point_Prover;
		friend class PoK_poly_Prover;

		const PolyCommitParams *paramsp;
		
		vec_ZZ_p v;
		bool mode;
		G1 C_internal(const ZZX &expons, const vector< vector<G1> > &precomp_products) const;

	public:
		ZZ_pX f;

		// Commit to a polynomial of degree at most t.
		PolyCommitter(const PolyCommitParams *paramsp, const ZZ_pX &f) :
			paramsp(paramsp), f(f), mode(POLYCOMMIT_MODE_POLY)
		{
			if (deg(f) > (int)paramsp->get_t())
				throw runtime_error("degree of f is too large");
		}

		// Commit to a vector of length at most t+1.
		PolyCommitter(const PolyCommitParams *paramsp, const vec_ZZ_p &v) :
			paramsp(paramsp), v(v), mode(POLYCOMMIT_MODE_VEC)
		{
			int t_max = (int)paramsp->get_t() + 1;

			if (v.length() > t_max)
				throw runtime_error("length of v is too large");

			f = poly_from_vec(v, t_max);
		}

		G1 get_C();
		G1 get_C_fast() const;
		G1 createWitness(const ZZ_p &i, const ZZ_p &blinding_factor = to_ZZ_p(1)) const;
		static vector<G1> createWitness(const vector<PolyCommitter> &commits, const ZZ_p &i);
		static ZZ_pX poly_from_vec(const vec_ZZ_p &v, int length);

		static bool verifyEval(const PolyCommitParams *p,
			const G1 &C, const ZZ_p &i, const ZZ_p &fi, const G1 &witness);

		static bool verifyEval(const PolyCommitParams *p, 
			const vector<G1> &C, const ZZ_p &i, const vec_ZZ_p &fi, const vector<G1> &witness);
	};

	Zr to_Zr(const Pairing &e, const ZZ_p x);
	Zr to_Zr(const Pairing &e, const uint64_t x);
}
#endif
