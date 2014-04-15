#ifndef __POLYCOMMITPROOFS_H__
#define __POLYCOMMITPROOFS_H__

#include <iostream>
#include <NTL/ZZ_p.h>
#include "PBC.h"
#include "PolyCommitCommon.h"
#include <sstream>

using namespace std;
using namespace NTL;

namespace PolyCommitment
{
	class ZK_Prover
	{
	public:
		// Write the announcement to an ostream
		virtual void announce(ostream &os) {}

		// Consume the challenge, and write the response to an ostream
		virtual void respond(ostream &os, const ZZ_p &challenge) {}

		// Create an entire Fiat-Shamir non-interactive proof.  Write it to
		// os.
		void FS_proof(ostream &os, const string nonce = "")
		{
			announce(os);
			ZZ_p challenge = FS_challenge(nonce);
			respond(os, challenge);
		}

	protected:
		// Protected constructor so that you can't actually instantiate the
		// base class
		ZK_Prover() {}

		// Create a Fiat-Shamir non-interactive challenge
		virtual ZZ_p FS_challenge(const string nonce = "") { return ZZ_p::zero(); }
	};

	class ZK_Verifier
	{
	public:
		// Read the announcement from an istream, produce a challenge
		// and save it in private state
		virtual ZZ_p challenge(istream &is) { return ZZ_p::zero(); }

		// Read the response from an istream, produce a boolean indicating
		// acceptance or rejection of the proof
		virtual bool verify(istream &is) { return false; }

		// Consume and verify an entire Fiat-Shamir non-interactive proof.
		// Produce a boolean indicating acceptance or rejection of the
		// proof.
		bool FS_verify(istream &is, const string nonce = "")
		{
			FS_challenge(is, nonce); // Ignore the output
			return verify(is);
		}

		// Create a fake proof.  Write the fake announcement to aos and
		// the fake response to ros.  Return the fake challenge.  This is
		// on the verifier's side, because knowledge of the prover's private
		// data is not required.
		ZZ_p fake(ostream &aos, ostream &ros);

		// As above, but supply the challenge to be used.  If this proof
		// does not support this, throw an exception.
		void fake(ostream &aos, ostream &ros, const ZZ_p &chall);

	protected:
		// Protected constructor so that you can't actually instantiate the
		// base class
		ZK_Verifier() {}

		// Compute the Fiat-Shamir non-interactive challenge and save it in
		// private state
		virtual ZZ_p FS_challenge(istream &is, const string nonce = "") { return ZZ_p::zero(); }

		// The created or computed challenge
		ZZ_p chall;
	};

	class PoK_point_Prover : public ZK_Prover
	{
	public:
		// Constructor takes the public params and the prover's private data
		PoK_point_Prover(const PolyCommitter &C, const ZZ_p &i)
			: com(C), Cval(C.get_C_fast()), index(i), announced(false), responded(false)
		{
		}

		PoK_point_Prover(const PolyCommitter &C, const G1 &Cval, const ZZ_p &i)
			: com(C), Cval(Cval), index(i), announced(false), responded(false)
		{
		}

		// Write the announcement to an ostream
		void announce(ostream &os);

		// Consume the challenge, and write the response to an ostream
		void respond(ostream &os, const ZZ_p &chall);

	protected:
		ZZ_p FS_challenge(const string nonce = "");

		const PolyCommitter com;
		const G1 Cval;
		const ZZ_p index;

	private:
		// Whatever private state you need to keep.  Try to avoid putting
		// any pointers in here.
		bool announced;
		bool responded;
		G1 witness_prime;
		GT LHS;
		ZZ_p gamma;
		ZZ_p s1, s2;
	};

	class PoK_poly_Prover : public PoK_point_Prover
	{
	public:
		// Constructor takes the public params and the prover's private data
		PoK_poly_Prover(const PolyCommitter &C, const string nonce = "")
			: PoK_point_Prover(C, C.get_C_fast(), FS_index(C.get_C_fast(), nonce))
		{
		}

		PoK_poly_Prover(const PolyCommitter &C, const G1 &Cval, const string nonce = "")
			: PoK_point_Prover(C, Cval, FS_index(Cval, nonce))
		{
		}

		// Assert that the committed polynomial has degree at most d.
		void assert_degree(ostream &os, unsigned int d);

	protected:
		ZZ_p FS_index(const string nonce = "");
		static ZZ_p FS_index(const G1 &Cval, const string nonce = "");
	};

	class PoK_point_Verifier : public ZK_Verifier
	{
	public:
		// Constructor takes the public params (but not the prover's private
		// data)
		PoK_point_Verifier(const PolyCommitParams *paramsp, const G1 &C, const ZZ_p &i)
			: paramsp(paramsp), C(C), index(i), witness_prime(paramsp->get_pairing()), LHS(paramsp->get_pairing()), challenged(false)
		{
		}

		// Read the announcement from an istream, produce a challenge
		// and save it in private state
		ZZ_p challenge(istream &is);

		// Read the response from an istream, produce a boolean indicating
		// acceptance or rejection of the proof
		bool verify(istream &is);

		ZZ_p fake(ostream &aos, ostream &ros);

		void fake(ostream &aos, ostream &ros, const ZZ_p &chall);


	protected:
		ZZ_p FS_challenge(istream &is, const string nonce = "");

		// Common inputs
		const PolyCommitParams *paramsp;
		G1 C;
		ZZ_p index;

	private:
		// The prover's announcement
		G1 witness_prime;
		GT LHS;

		// The challenge
		bool challenged;
		ZZ_p chall;
	};

	class PoK_poly_Verifier : public PoK_point_Verifier
	{
	public:
		// Constructor takes the public params and the prover's private data
		PoK_poly_Verifier(const PolyCommitParams *paramsp, const G1 &C, const string nonce = "")
			: PoK_point_Verifier(paramsp, C, FS_index(C, nonce))
		{
		}

		// Verifies an assertion that the committed  polynomial has degree at most d.
		bool verify_assert_degree(istream &is, unsigned int d);

	protected:
		ZZ_p FS_index(const string nonce = "");
		static ZZ_p FS_index(const G1 &C, const string nonce = "");
	};
}
#endif
