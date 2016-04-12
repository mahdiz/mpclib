using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MpcLib.Common.FiniteField;

namespace MpcLib.SecretSharing
{
	/// <summary>
	/// Implemenets the threshold secret sharing scheme based on Adi Shamir's method.
	/// </summary>
	public class ShamirSharing
	{
		/// <summary>
		/// Calculates the shares of a secret with polynomial of degree t and numPlayers players.
		/// </summary>
		public static IList<Zp> Share(Zp secret, int numPlayers, int polyDeg)
		{
			IList<Zp> coeffs;
			return Share(secret, numPlayers, polyDeg, false, out coeffs);
		}

		/// <summary>
		/// Calculates the shares of a secret with polynomial of degree t and numPlayers players.
		/// The method also returns the array of coefficients of the polynomial.
		/// </summary>
		public static IList<Zp> Share(Zp secret, int numPlayers, int polyDeg, out IList<Zp> coeffs)
		{
			return Share(secret, numPlayers, polyDeg, false, out coeffs);
		}

		/// <summary>
		/// Evaluates the shares of secret with polynomial of degree 'polynomDeg' and 'numPlayers' players.
		/// </summary>
		private static IList<Zp> Share(Zp secret, int numPlayers, int polynomDeg, bool usePrimitiveShare, out IList<Zp> coeffs)
		{
#if NO_COMPUTATION
			// send some dummy shares
			var shares = new Zp[numPlayers];
			for (int i = 0; i < numPlayers; i++)
				shares[i] = new Zp(secret.Prime);
			return shares;
#else
			Debug.Assert(numPlayers > polynomDeg, "Polynomial degree cannot be greater than or equal to the number of players!");

			// Create a random polynomial - f(x)
			// Note: Polynomial of degree d has d+1 coefficients
			var randomMatrix = ZpMatrix.GetRandomMatrix(1, polynomDeg + 1, secret.Prime);

			// The free variable in the Random Polynomial (i.e.	f(x)) is the secret
			randomMatrix.SetMatrixCell(0, 0, secret);

			// Polynomial coefficients
			coeffs = randomMatrix.GetMatrixRow(0);

			// Create vanderMonde matrix
			ZpMatrix vanderMonde;
			if (usePrimitiveShare)
				vanderMonde = ZpMatrix.GetPrimitiveVandermondeMatrix(polynomDeg + 1, numPlayers, secret.Prime);
			else
				vanderMonde = ZpMatrix.GetVandermondeMatrix(polynomDeg + 1, numPlayers, secret.Prime);

			// Compute f(i) for the i-th  player
			var sharesArr = randomMatrix.Times(vanderMonde).ZpVector;
            Debug.Assert(sharesArr != null);
			Debug.Assert(sharesArr.Length == numPlayers);
			return sharesArr;
#endif
		}

		/// <summary>
		/// Evaluates the shared secrets of secret with polynom of degree t and numberOfPlayers players.
		/// </summary>
		public static ShareDetails DetailedShare(Zp secret,
			int numPlayers, int polynomDeg)
		{
			if (numPlayers <= polynomDeg)
				throw new ArgumentException("Polynomial degree cannot be bigger or equal to the number of  players");

			// Creating the Random Polynomial - f(x)
			var randomMatrix = ZpMatrix.GetRandomMatrix(1, polynomDeg + 1, secret.Prime);

			// The free variable in the Random Polynomial( f(x) ) is the seceret
			randomMatrix.SetMatrixCell(0, 0, secret);

			// Create vanderMonde matrix
			var vanderMonde = ZpMatrix.GetPrimitiveVandermondeMatrix(polynomDeg + 1, numPlayers, secret.Prime);

			// Compute f(i) for the i-th  player
			var sharesArr = randomMatrix.Times(vanderMonde).ZpVector;

			var details = new ShareDetails(randomMatrix.GetMatrixRow(0), sharesArr);
			return details;
		}

		public static IList<Zp> PrimitiveShare(Zp secret, int numPlayers, int polynomDeg)
		{
			IList<Zp> coeffs;
			return Share(secret, numPlayers, polynomDeg, true, out coeffs);
		}

		/// <summary>
		/// Recombines (interpolate) the secret from secret shares.
		/// </summary>
		public static Zp Recombine(IList<Zp> sharedSecrets, int polyDeg,
			int prime, bool usePrimitiveRecombine)
		{
#if NO_COMPUTATION
			return new Zp(prime);
#else
			if (sharedSecrets.Count <= polyDeg)
				throw new System.ArgumentException("Polynomial degree cannot be bigger or equal to the number of  shares");

			// calculate the Vandermonde matrix in the field
			// TODO: We can cache this matrix because it is fixed.
			//ZpMatrix A = null;
			//if (usePrimitiveRecombine)
			//	A = ZpMatrix.GetSymmetricPrimitiveVandermondeMatrix(polynomDeg + 1, prime).Transpose;
			//else
			//	A = ZpMatrix.GetShamirRecombineMatrix(polynomDeg + 1, prime);

			//var truncShare = TruncateVector(sharedSecrets.ToArray(), polynomDeg + 1);
			//var solution = A.Solve(truncShare);

			var secret = SimpleRecombine(sharedSecrets, polyDeg, prime);
			//Debug.Assert(simp.Value == solution[0].Value);

			//return solution[0];
			return secret;
#endif
		}

		// Mahdi's recombine method based on Lagrange interpolation for finite fields.
		public static Zp SimpleRecombine(IList<Zp> sharedSecrets, int polyDeg, int prime)
		{
			if (sharedSecrets.Count < polyDeg)
				throw new System.ArgumentException("Polynomial degree cannot be bigger or equal to the number of  shares");

			// find Lagrange basis polynomials free coefficients
			var L = new Zp[polyDeg + 1];
			for (int i = 0; i < polyDeg + 1; i++)
				L[i] = new Zp(prime, 1);

			int ix = 0;
			for (var i = new Zp(prime, 1); i < polyDeg + 2; i++, ix++)
			{
				for (var j = new Zp(prime, 1); j < polyDeg + 2; j++)
				{
					if (j != i)
					{
						var additiveInverse = j.AdditiveInverse;
						L[ix] = L[ix] * (additiveInverse / (i + additiveInverse));		// note: division done in finite-field
					}
				}
			}

			// find the secret by multiplying each share to the corresponding Lagrange's free coefficient
			var secret = new Zp(prime, 0);
			for (int i = 0; i < polyDeg + 1; i++)
				secret = secret + (L[i] * sharedSecrets[i]);

			return secret;
		}

		public static Zp Recombine(IList<Zp> sharedSecrets, int polynomDeg, int prime)
		{
			return Recombine(sharedSecrets, polynomDeg, prime, false);
		}

		/// <summary>
		/// Creates a random poynomial Qj(x), for the j player, and creates a list of elements,
		/// such that the i-th element is Qj(i)
		/// </summary>
		public static IList<Zp> GetRandomizedShares(int numPlayers,
			int polynomDeg, int prime)
		{
			// polynomial q(x) free element must be zero so it won't effect the final result
			var polyfreeElem = new Zp(prime, 0);
			return Share(polyfreeElem, numPlayers, polynomDeg);
		}

		/// <summary>
		/// Creates a random poynomial Qj(x) ,for the j player,and creates a list of elements,
		/// such that the i-th element is Qj(i)
		/// </summary>
		public static IList<Zp> GetRandomizedSharesByzantineCase(int numPlayers,
			int polynomDeg, int prime)
		{
			//polynomial q(x) free element must be zero so it won't effect the final result
			var polyfreeElem = new Zp(prime, 0);
			return PrimitiveShare(polyfreeElem, numPlayers, polynomDeg);
		}

		/// <summary>
		/// The i-th user gets a Qj(i) List from users, each user j calculated Qj(i) - the j element in the List
		/// </summary>
		public static Zp CalculateRandomShare(Zp myShare, IList<Zp> polyUpdate)
		{
			var newShare = new Zp(myShare);
			newShare.AddListContent(polyUpdate);
			return newShare;
		}

		private static Zp[] TruncateVector(Zp[] vector, int toSize)
		{
			if (vector.Length < toSize)
				return null;

			var truncVec = new Zp[toSize];
			for (int i = 0; i < toSize; i++)
				truncVec[i] = new Zp(vector[i]);

			return truncVec;
		}

		public static bool CheckSharedSecrets(Zp secret, int numberOfPlayers,
			int polynomDeg, int prime)
		{
			return secret.Equals(Recombine(Share(secret, numberOfPlayers, polynomDeg), polynomDeg, prime));
		}
	}
}