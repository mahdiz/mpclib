using System;
using System.Collections.Generic;
using System.Numerics;

namespace MpcLib.Common.FiniteField
{
	/// <summary>
	/// Implements a static internal cache for improving runtime performance of secret sharing.
	/// </summary>
	public class StaticCache
	{
		/// <summary>
		/// Maps a prime to the corresponding array of inverses.
		/// </summary>
		private static Dictionary<int, int[]> fieldInverses = new Dictionary<int, int[]>();

		public static int[] GetFieldInverse(int prime)
		{
			if (fieldInverses.ContainsKey(prime))
				return fieldInverses[prime];

			// Mahdi: extended euclidean is used for better performance.
			var invArr = new int[prime];
			for (int i = 0; i < prime; i++)
			{
				var res = ExtendedEuclidean(i, prime);
				invArr[i] = res[1] < 0 ? res[1] + prime : res[1];
			}
			fieldInverses[prime] = invArr;
			return invArr;
		}

		/// <summary>
		/// Performs the Extended Euclidean algorithm and returns gcd(a,b).
		/// The function also returns integers x and y such that ax + by = gcd(a,b).
		/// If gcd(a,b) = 1, then x is a multiplicative inverse of "a mod b" and
		/// y is a multiplicative inverse of "b mod a".
		/// </summary>
		/// <returns>
		/// An array of three integers:
		/// The first element is gcd(a,b).
		/// The second element is the inverse of a mod b (may be negative).
		/// The third element is the inverse of b mod a (may be negative).
		/// </returns>
		private static int[] ExtendedEuclidean(int a, int b)
		{
			int[] res = new int[3];
			int q;

			if (b == 0)
			{
				res[0] = a;
				res[1] = 1;
				res[2] = 0;
			}
			else
			{
				q = a / b;
				res = ExtendedEuclidean(b, a % b);
				int temp = res[1] - res[2] * q;
				res[1] = res[2];
				res[2] = temp;
			}
			return res;
		}

        private static Dictionary<Tuple<BigInteger, int>, IList<BigZp>> VandermondeInvCache = new Dictionary<Tuple<BigInteger, int>, IList<BigZp>>();

        public static IList<BigZp> GetVandermondeInvColumn(BigInteger prime, int size)
        {
            Tuple<BigInteger, int> t = new Tuple<BigInteger, int>(prime, size);
            if (VandermondeInvCache.ContainsKey(t))
                return VandermondeInvCache[t];

            var inv = BigZpMatrix.GetVandermondeMatrix(size, size, prime).Inverse.GetMatrixColumn(0);
            VandermondeInvCache[t] = inv;
            return inv;
        }
    }
}