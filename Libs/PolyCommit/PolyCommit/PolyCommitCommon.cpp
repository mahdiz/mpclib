#include "PolyCommitCommon.h"

#include <winsock2.h>

using namespace std;
using namespace MpcLib::Commitments;
using namespace MpcLib::Commitments::PolyCommitment;

void PolyCommitParams::create(ostream &os, const string pairingparams, unsigned int t)
{
	// Create the Pairing
	Pairing p(pairingparams);
	size_t len = pairingparams.length();
	unsigned char lenbuf[2];
	lenbuf[0] = len >> 8;
	lenbuf[1] = len;
	os.write((const char *)lenbuf, 2);
	os << pairingparams;

	// Create a random generator for G1
	unsigned int t_nbo;  // network byte order
	t_nbo = htonl(t);
	os.write((const char *)&t_nbo, 4);
	G1 g(p, false);
	os << g;

	// Pick a random alpha
	Zr alpha(p, true);

	for (unsigned int i = 0; i < t; ++i)
	{
		g ^= alpha;
		os << g;
	}

	// Create a random generator for G1 -- for h
	G1 h(p, false);
	os << h;

	//  -- for h^alpha

	h ^= alpha;
	os << h;

	// This controls how many entries can be in a single batch opening
	// of values of a single polynomial
	unsigned int t_hat = 1;
	unsigned int t_hat_nbo;  // network byte order

	// Create a random generator for G2
	t_hat_nbo = htonl(t_hat);
	os.write((const char *)&t_hat_nbo, 4);
	G2 ghat(p, false);
	os << ghat;

	for (unsigned int i = 0; i < t_hat; ++i)
	{
		ghat ^= alpha;
		os << ghat;
	}
}

void PolyCommitParams::create(const string pairingparams)
{
	pairing.init(pairingparams);

	// Create a random generator for G1
	G1 g(pairing, false);
	galphai.push_back(g);

	// Pick a random alpha
	Zr alpha(pairing, true);

	G1 g_alpha(g);
	for (int i = 0; i < t; i++)
	{
		g_alpha ^= alpha;
		galphai.push_back(g_alpha);
	}

	// Create a random generator for G1 -- for h
	G1 h(pairing, false);
	this->h = h;
	this->halpha = h ^ alpha;

	unsigned int t_hat = 1;
	G2 ghat(pairing, false);
	ghatalphai.push_back(ghat);

	G2 ghat_alpha(ghat);
	for (int i = 0; i < t_hat; i++)
	{
		ghat_alpha ^= alpha;
		ghatalphai.push_back(ghat_alpha);
	}

	this->t_hat = t_hat;

	// Figure out the order of the pairing groups.  There doesn't seem
	// to be an API call for this?
	Zr zero(pairing);
	Zr one(zero, 1);
	zero -= one;
	string negone = zero.toString();

	// Convert it to little-endian
	int l = negone.length();
	const unsigned char *negonestr = (const unsigned char *)negone.c_str();
	vector<unsigned char> orderm1(l + 1);

	for (int i = 0; i < l; ++i)
		orderm1[i] = negonestr[l - 1 - i];

	orderm1[l] = '\0';
	// Convert it to a ZZ
	this->order = ZZFromBytes(orderm1.data(), l);
	this->order += 1;

	//Save ZZ_p context
	ZZ_pContext savectx;
	savectx.save();

	ZZ_p::init(this->order);

	// Compute the precomputation table
	G1 gzero(pairing, true);
	int windows = t / this->window_width + 1;
	this->precomp_products.resize(windows);

	for (int k = 0; k < windows; ++k)
	{
		this->precomp_products[k].push_back(gzero);
		unsigned int bound = (k == windows - 1) ? (t + 1) % this->window_width : this->window_width;
		for (unsigned int i = 0; i < bound; ++i)
		{
			for (int j = 0; j < (1 << i); ++j)
			{
				precomp_products[k].push_back(
					precomp_products[k][j] * galphai[k*this->window_width + i]);
			}
		}
	}

	// Optionally, compute the Lagrange basis
	if (compute_Lagrange)
	{
		vec_ZZ_p indices;
		for (unsigned int i = 0; i <= t; ++i) { append(indices, to_ZZ_p(i)); }
		ZZ_pX lambda = BuildFromRoots(indices);

		for (unsigned int i = 0; i <= t; ++i)
		{
			ZZ_pX lambdai = lambda / (ZZ_pX(1, 1) - to_ZZ_p(i));
			lambdai /= eval(lambdai, to_ZZ_p(i));
			PolyCommitter C(this, lambdai);
			this->glambdai.push_back(C.get_C_fast());
		}

		// Compute the precomputation table
		this->lambda_precomp_products.resize(windows);
		for (int k = 0; k < windows; k++)
		{
			this->lambda_precomp_products[k].push_back(gzero);
			unsigned int bound = (k == windows - 1) ? (t + 1) % this->window_width : this->window_width;
			for (unsigned int i = 0; i < bound; ++i)
			{
				for (int j = 0; j < (1 << i); ++j)
				{
					this->lambda_precomp_products[k].push_back(
						this->lambda_precomp_products[k][j] *
						this->glambdai[k*this->window_width + i]);
				}
			}
		}
	}

	// Restore context
	savectx.restore();
}

Zr PolyCommitment::to_Zr(const Pairing &e, const ZZ_p x)
{
	long len = NumBytes(rep(x));

	vector<unsigned char> xbytes(len);
	vector<unsigned char> revbytes(len);

	BytesFromZZ(xbytes.data(), rep(x), len);

	for (long i = 0; i < len; ++i)
		revbytes[i] = xbytes[len - 1 - i];

	Zr zr(e, revbytes.data(), len, 0);
	return zr;
}

Zr PolyCommitment::to_Zr(const Pairing &e, const uint64_t x)
{
	long len = sizeof(uint64_t);
	vector<unsigned char> revbytes(len);

	for (long i = 0; i < len; ++i)
		revbytes[i] = (char)(x >> (8 * (len - 1 - i)));

	Zr zr(e, revbytes.data(), len, 0);
	return zr;
}

ostream &PolyCommitment::operator<<(ostream &os, PolyCommitParams &params)
{
	// Write the pairing params
	size_t len = params.pairing.get_pbc_param_t().length();
	unsigned char lenbuf[2];
	lenbuf[0] = len >> 8;
	lenbuf[1] = len;
	os.write((const char *)lenbuf, 2);
	os << params.pairing.get_pbc_param_t();

	// Write the g^\alpha^i
	unsigned int t_nbo;  // network byte order
	t_nbo = htonl(params.t);
	os.write((const char *)&t_nbo, 4);

	for (unsigned int i = 0; i <= params.t; ++i)
		os << params.galphai[i];

	// Write h,h^alpha
	os << params.h << params.halpha;

	// Write the ghat^\alpha^i
	unsigned int t_hat_nbo;  // network byte order
	t_hat_nbo = htonl(params.t_hat);
	os.write((const char *)&t_hat_nbo, 4);

	for (unsigned int i = 0; i <= params.t_hat; ++i)
		os << params.ghatalphai[i];

	return os;
}

istream &PolyCommitment::operator>>(istream &is, PolyCommitParams &params)
{
	unsigned char lenbuf[2];
	is.read((char *)lenbuf, 2);
	size_t len = (lenbuf[0] << 8) + lenbuf[1];

	vector<char> pairingparams(len + 1);

	is.read(pairingparams.data(), len);
	pairingparams[len] = '\0';
	params.pairing.init(pairingparams.data());

	// Read the maximum exponent of g^\alpha^i
	unsigned int t;
	unsigned int t_nbo;  // network byte order
	is.read((char *)&t_nbo, 4);
	t = ntohl(t_nbo);

	// Read the g^\alpha^i
	for (unsigned int i = 0; i <= t; ++i)
	{
		G1 g(params.pairing, true);
		is >> g;
		params.galphai.push_back(g);
	}
	params.t = t;

	if (t % params.window_width == params.window_width - 1)
		params.window_width = params.window_width - 1;

	// Create a random generator for G1 -- for h
	G1 h(params.pairing, true);
	is >> h;
	params.h = h;
	//  -- for h^alpha
	G1 halpha(params.pairing, true);
	is >> halpha;
	params.halpha = halpha;

	// Read the maximum exponent of ghat^\alpha^i
	unsigned int t_hat;
	unsigned int t_hat_nbo;  // network byte order
	is.read((char *)&t_hat_nbo, 4);
	t_hat = ntohl(t_hat_nbo);

	// Read the ghat^\alpha^i
	for (unsigned int i = 0; i <= t_hat; ++i)
	{
		G2 g(params.pairing, true);
		is >> g;
		params.ghatalphai.push_back(g);
	}
	params.t_hat = t_hat;

	// Figure out the order of the pairing groups.  There doesn't seem
	// to be an API call for this?
	Zr zero(params.pairing);
	Zr one(zero, 1);
	zero -= one;
	string negone = zero.toString();

	// Convert it to little-endian
	int l = negone.length();
	const unsigned char *negonestr = (const unsigned char *)negone.c_str();
	vector<unsigned char> orderm1(l + 1);

	for (int i = 0; i < l; ++i)
		orderm1[i] = negonestr[l - 1 - i];

	orderm1[l] = '\0';
	// Convert it to a ZZ
	params.order = ZZFromBytes(orderm1.data(), l);
	params.order += 1;

	//Save ZZ_p context
	ZZ_pContext savectx;
	savectx.save();

	ZZ_p::init(params.order);

	// Compute the precomputation table
	G1 gzero(params.pairing, true);
	int windows = t / params.window_width + 1;
	params.precomp_products.resize(windows);
	for (int k = 0; k < windows; ++k)
	{
		params.precomp_products[k].push_back(gzero);
		unsigned int bound = (k == windows - 1) ? (t + 1) % params.window_width : params.window_width;
		for (unsigned int i = 0; i < bound; ++i)
		{
			for (int j = 0; j < (1 << i); ++j)
			{
				params.precomp_products[k].push_back(
					params.precomp_products[k][j] *
					params.galphai[k*params.window_width + i]);
			}
		}
	}

	// Optionally, compute the Lagrange basis
	if (params.compute_Lagrange)
	{
		vec_ZZ_p indices;
		for (unsigned int i = 0; i <= t; ++i) { append(indices, to_ZZ_p(i)); }
		ZZ_pX lambda = BuildFromRoots(indices);

		for (unsigned int i = 0; i <= t; ++i)
		{
			ZZ_pX lambdai = lambda / (ZZ_pX(1, 1) - to_ZZ_p(i));
			lambdai /= eval(lambdai, to_ZZ_p(i));
			PolyCommitter C(&params, lambdai);
			params.glambdai.push_back(C.get_C_fast());
		}

		// Compute the precomputation table
		params.lambda_precomp_products.resize(windows);
		for (int k = 0; k < windows; k++)
		{
			params.lambda_precomp_products[k].push_back(gzero);
			unsigned int bound = (k == windows - 1) ? (t + 1) % params.window_width : params.window_width;
			for (unsigned int i = 0; i < bound; ++i)
			{
				for (int j = 0; j < (1 << i); ++j)
				{
					params.lambda_precomp_products[k].push_back(
						params.lambda_precomp_products[k][j] *
						params.glambdai[k*params.window_width + i]);
				}
			}
		}
	}

	// Restore context
	savectx.restore();

	return is;
}

ZZ_pX PolyCommitter::poly_from_vec(const vec_ZZ_p &v, int length)
{
	vec_ZZ_p indices;
	for (int i = 0; i < length; ++i) { append(indices, to_ZZ_p(i)); }

	return interpolate(indices, VectorCopy(v, length));
}

G1 PolyCommitter::get_C()
{
	//Save ZZ_p context
	ZZ_pContext savectx;
	savectx.save();

	ZZ_p::init(paramsp->order);
	G1 g(paramsp->pairing);

	if ((mode == POLYCOMMIT_MODE_VEC) && (paramsp->compute_Lagrange))
	{
		int len = v.length();
		for (int i = 0; i < len; ++i)
		{
			ZZ_p cfp = v[i];
			Zr cf = ::to_Zr(paramsp->pairing, cfp);
			g *= ((paramsp->glambdai[i]) ^ cf);
		}
	}
	else
	{
		int d = deg(f);
		for (int i = 0; i <= d; ++i)
		{
			ZZ_p cfp = coeff(f, i);
			Zr cf = ::to_Zr(paramsp->pairing, cfp);
			g *= ((paramsp->galphai[i]) ^ cf);
		}
	}

	// Restore context
	savectx.restore();

	return g;
}

static void divrem2(ZZX &q, ZZX &r, const ZZX &ex)
{
	int d = deg(ex);
	for (int i = 0; i <= d; ++i)
	{
		SetCoeff(q, i, coeff(ex, i) / 2);
		SetCoeff(r, i, coeff(ex, i) % 2);
	}
}

G1 PolyCommitter::C_internal(const ZZX &expons, const vector< vector<G1> > &precomp_products) const
{
	if (IsZero(expons))
		return G1(paramsp->pairing, true);
	
	ZZX q, r;
	divrem2(q, r, expons);
	G1 C = C_internal(q, precomp_products);
	int d = deg(r);

	C = C.square();
	int windows = d / paramsp->window_width + 1;
	for (int k = 0; k < windows; ++k)
	{
		unsigned int index = 0;
		for (int i = 0; i < paramsp->window_width; ++i)
		{
			if (!IsZero(coeff(r, k*paramsp->window_width + i)))
				index += (1 << i);
		}
		C *= precomp_products[k][index];
	}
	return C;
}

G1 PolyCommitter::get_C_fast() const
{
	if ((mode == POLYCOMMIT_MODE_VEC) && (paramsp->compute_Lagrange))
	{
		int len = v.length();
		ZZX expons;
		for (int i = 0; i < len; ++i)
			SetCoeff(expons, i, rep(v[i]));

		return C_internal(expons, paramsp->lambda_precomp_products);
	}
	else
	{
		int d = deg(f);
		ZZX expons;
		for (int i = 0; i <= d; ++i)
			SetCoeff(expons, i, rep(coeff(f, i)));

		return C_internal(expons, paramsp->precomp_products);
	}
}

G1 PolyCommitter::createWitness(const ZZ_p &i, const ZZ_p &blinding_factor) const
{
	ZZ_p fi = eval(f, i);

	// Compute the polynomial psi(x) = (f(x)-f(i))/(x-i)
	ZZ_pX denom(1, 1); // x
	denom -= i;
	ZZ_pX psi;

	if (divide(psi, f - fi, denom) == 0)
		throw runtime_error("x-i doesn't divide f(x)-f(i)?!");

	// Compute w = g^{psi(alpha)}, which is just a commitment to psi
	PolyCommitter psicom(paramsp, blinding_factor * psi);

	return psicom.get_C_fast();
}

vector<G1> PolyCommitter::createWitness(const vector<PolyCommitter> &commits, const ZZ_p &i)
{
	vector<G1> witnesses;
	for (size_t j = 0; j < commits.size(); j++)
		witnesses.push_back(commits[j].createWitness(i));

	return witnesses;
}

bool PolyCommitter::verifyEval(const PolyCommitParams *p, 
	const G1 &C, const ZZ_p &i,	const ZZ_p &fi, const G1 &witness)
{
	const Pairing &e = p->get_pairing();
	const G1 &g = p->get_galphai(0);
	const G2 &ghat = p->get_ghatalphai(0);
	const G2 &ghatalpha = p->get_ghatalphai(1);

	Zr ir = to_Zr(e, i);
	Zr fir = to_Zr(e, fi);

	GT LHS = e(C, ghat);
	GT RHS = e(witness, ghatalpha / (ghat^ir)) * e(g, ghat^fir);

	//const Pairing &e = p->get_pairing();
	//const G1 &g = p->get_galphai(0);
	//const G1 &g_alpha = p->get_galphai(1);

	//Zr ir = to_Zr(e, i);
	//Zr fir = to_Zr(e, fi);

	//GT LHS = e(C, g);
	//GT RHS = e(witness, g_alpha / (g^ir)) * (e(g, g) ^ fir);

	bool r = (LHS == RHS);
	return r;
}

bool PolyCommitter::verifyEval(const PolyCommitParams *p, const vector<G1> &C, const ZZ_p &i,
	const vec_ZZ_p &fi, const vector<G1> &witness)
{
	const Pairing &e = p->get_pairing();
	const G1 &g = p->get_galphai(0);
	const G2 &ghat = p->get_ghatalphai(0);
	const G2 &ghatalpha = p->get_ghatalphai(1);
	Zr ir = to_Zr(e, i);
	G1 C_prime(e, 1);
	G1 witness_prime(e, 1);
	Zr fir = to_Zr(e, to_ZZ_p(0));
	for (size_t i = 0; i < C.size(); i++)
	{
		long rnd = RandomBits_long(LENGTH_OF_RANDOMNESS);
		Zr t = to_Zr(e, to_ZZ_p(rnd));
		C_prime *= C[i] ^ t;
		witness_prime *= witness[i] ^ t;
		fir += t * to_Zr(e, fi[i]);
	}
	GT LHS = e(C_prime, ghat);
	GT RHS = e(witness_prime, ghatalpha / (ghat^ir)) * e(g, ghat^fir);
	return (LHS == RHS);
}