using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using MpcLib.Common.FiniteField;

namespace MpcLib.SecretSharing
{
	/// <summary>
	/// Implemenets the threshold secret sharing scheme based on Adi Shamir's method on BigIntegers.
	/// </summary>
	public class BigShamirSharing
	{
		/// <summary>
		/// Calculates the shares of a secret with polynomial of degree t and numPlayers players.
		/// </summary>
		public static IList<BigZp> Share(BigZp secret, int numPlayers, int polyDeg)
		{
			IList<BigZp> coeffs;
			return Share(secret, numPlayers, polyDeg, false, out coeffs);
		}

		/// <summary>
		/// Calculates the shares of a secret with polynomial of degree t and numPlayers players.
		/// The method also returns the array of coefficients of the polynomial.
		/// </summary>
		public static IList<BigZp> Share(BigZp secret, int numPlayers, int polyDeg, out IList<BigZp> coeffs)
		{
			return Share(secret, numPlayers, polyDeg, false, out coeffs);
		}

		/// <summary>
		/// Evaluates the shares of secret with polynomial of degree 'polynomDeg' and 'numPlayers' players.
		/// </summary>
		private static IList<BigZp> Share(BigZp secret, int numPlayers, int polyDeg, bool usePrimitiveShare, out IList<BigZp> coeffs)
		{
#if NO_COMPUTATION
			// send some dummy shares
			var shares = new BigZp[numPlayers];
			for (int i = 0; i < numPlayers; i++)
				shares[i] = new BigZp(secret.Prime);
			return shares;
#else
			Debug.Assert(numPlayers > polyDeg, "Polynomial degree cannot be greater than or equal to the number of players!");

			// Create a random polynomial - f(x)
			// Note: Polynomial of degree d has d+1 coefficients
			var randomMatrix = BigZpMatrix.GetRandomMatrix(1, polyDeg + 1, secret.Prime);

			// The free variable in the Random Polynomial (i.e.	f(x)) is the secret
			randomMatrix.SetMatrixCell(0, 0, secret);

			// Polynomial coefficients
			coeffs = randomMatrix.GetMatrixRow(0);

			// Create vanderMonde matrix
			var vanderMonde = BigZpMatrix.GetVandermondeMatrix(polyDeg + 1, numPlayers, secret.Prime);

			// Compute f(i) for the i-th  player
			var sharesArr = randomMatrix.Times(vanderMonde).ZpVector;

			Debug.Assert(sharesArr.Length == numPlayers);
			return sharesArr;
#endif
		}

		/// <summary>
		/// Recombines (interpolate) the secret from secret shares.
		/// </summary>
		public static BigZp Recombine(IList<BigZp> sharedSecrets, int polyDeg,
			BigInteger prime, bool usePrimitiveRecombine)
		{
#if NO_COMPUTATION
			return new BigZp(prime);
#else
			if (sharedSecrets.Count <= polyDeg)
				throw new System.ArgumentException("Polynomial degree cannot be bigger or equal to the number of  shares");

			// calculate the Vandermonde matrix in the field
			// TODO: We can cache this matrix because it is fixed.
			//BigZpMatrix A = null;
			//if (usePrimitiveRecombine)
			//	A = BigZpMatrix.GetSymmetricPrimitiveVandermondeMatrix(polynomDeg + 1, prime).Transpose;
			//else
			//	A = BigZpMatrix.GetShamirRecombineMatrix(polynomDeg + 1, prime);

			//var truncShare = TruncateVector(sharedSecrets.ToArray(), polynomDeg + 1);
			//var solution = A.Solve(truncShare);

			var secret = SimpleRecombine(sharedSecrets, polyDeg, prime);
			//Debug.Assert(simp.Value == solution[0].Value);

			//return solution[0];
			return secret;
#endif
		}

		// Mahdi's recombine method based on Lagrange interpolation for finite fields.
		private static BigZp SimpleRecombine(IList<BigZp> sharedSecrets, int polyDeg, BigInteger prime)
		{
			if (sharedSecrets.Count < polyDeg)
				throw new System.ArgumentException("Polynomial degree cannot be bigger or equal to the number of  shares");

			// find Lagrange basis polynomials free coefficients
			var L = new BigZp[polyDeg + 1];
			for (int i = 0; i < polyDeg + 1; i++)
				L[i] = new BigZp(prime, 1);

			int ix = 0;
			for (var i = new BigZp(prime, 1); i < polyDeg + 2; i++, ix++)
			{
				for (var j = new BigZp(prime, 1); j < polyDeg + 2; j++)
				{
					if (j != i)
					{
						var additiveInverse = j.AdditiveInverse;
						L[ix] = L[ix] * (additiveInverse / (i + additiveInverse));		// note: division done in finite-field
					}
				}
			}

			// find the secret by multiplying each share to the corresponding Lagrange's free coefficient
			var secret = new BigZp(prime, 0);
			for (int i = 0; i < polyDeg + 1; i++)
				secret = secret + (L[i] * sharedSecrets[i]);

			return secret;
		}

		private static BigZp[] TruncateVector(BigZp[] vector, int toSize)
		{
			if (vector.Length < toSize)
				return null;

			var truncVec = new BigZp[toSize];
			for (int i = 0; i < toSize; i++)
				truncVec[i] = new BigZp(vector[i]);

			return truncVec;
		}

		public static BigZp Recombine(IList<BigZp> sharedSecrets, int polyDeg, BigInteger prime)
		{
			return Recombine(sharedSecrets, polyDeg, prime, false);
		}
	}
}