using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using MpcLib.NtlWrapper;
using System.Diagnostics;

namespace MpcLib.Common.FiniteField
{
	/// <summary>
	/// Implements some number-theoretic utility functions.
	/// </summary>
	public static class NumTheoryUtils
	{
		// MODP Diffie-Hellman Groups (see RFCs 2409 and 3526)
		public static readonly BigInteger DHPrime768 = BigInteger.Parse("1552518092300708935130918131258481755631334049434514313202351194902966239949102107258669453876591642442910007680288864229150803718918046342632727613031282983744380820890196288509170691316593175367469551763119843371637221007210577919");
		public static readonly BigInteger DHPrime1536 = BigInteger.Parse("2410312426921032588552076022197566074856950548502459942654116941958108831682612228890093858261341614673227141477904012196503648957050582631942730706805009223062734745341073406696246014589361659774041027169249453200378729434170325843778659198143763193776859869524088940195577346119843545301547043747207749969763750084308926339295559968882457872412993810129130294592999947926365264059284647209730384947211681434464714438488520940127459844288859336526896320919633919");
		public static readonly BigInteger DHPrime2048 = BigInteger.Parse("32317006071311007300338913926423828248817941241140239112842009751400741706634354222619689417363569347117901737909704191754605873209195028853758986185622153212175412514901774520270235796078236248884246189477587641105928646099411723245426622522193230540919037680524235519125679715870117001058055877651038861847280257976054903569732561526167081339361799541336476559160368317896729073178384589680639671900977202194168647225871031411336429319536193471636533209717077448227988588565369208645296636077250268955505928362751121174096972998068410554359584866583291642136218231078990999448652468262416972035911852507045361090559");

		/// <summary>
		/// Performs the Extended Euclidean algorithm and returns gcd(a,b).
		/// The function also returns integers x and y such that ax + by = gcd(a,b).
		/// If gcd(a,b) = 1, then x is a multiplicative inverse of "a mod b" and
		/// y is a multiplicative inverse of "b mod a".
		/// </summary>
		/// <returns>
		/// An array of three integers:
		/// The first element is gcd(a,b).
		/// x is an inverse of a mod b.
		/// y is an inverse of b mod a.
		/// </returns>
		public static int ExtendedEuclidean(int a, int b, out int x, out int y)
		{
			long xl = 0;
			long yl = 0;
			var r = (int)ExtendedEuclidean((long)a, (long)b, out xl, out yl);
			x = (int)xl;
			y = (int)yl;
			return r;
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
		/// x is an inverse of a mod b.
		/// y is an inverse of b mod a.
		/// </returns>
		public static long ExtendedEuclidean(long a, long b, out long x, out long y)
		{
			if (b == 0)
			{
				x = 1;
				y = 0;
				return a;
			}
			else
			{
				var g = ExtendedEuclidean(b, a % b, out x, out y);
				var tmp = y;
				y = x - a / b * y;
				x = tmp;
				return g;
			}
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
		/// x is an inverse of a mod b.
		/// y is an inverse of b mod a.
		/// </returns>
		public static BigInteger ExtendedEuclidean(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
		{
			if (b == 0)
			{
				x = 1;
				y = 0;
				return a;
			}
			else
			{
				var g = ExtendedEuclidean(b, a % b, out x, out y);
				var tmp = y;
				y = x - a / b * y;
				x = tmp;
				return g;
			}
		}

		/// <summary>
		/// Returns the multiplicative inverse of a modulo m.
		/// </summary>
		public static int MultiplicativeInverse(int a, int m)
		{
			return (int)MultiplicativeInverse((long)a, (long)m);
		}

		/// <summary>
		/// Returns the multiplicative inverse of a modulo m.
		/// </summary>
		public static long MultiplicativeInverse(long a, long m)
		{
			long x = 0, y = 0;
			ExtendedEuclidean(a, m, out x, out y);
			return x < 0 ? x + m : x;
		}

		/// <summary>
		/// Returns the multiplicative inverse of a modulo m.
		/// </summary>
		public static BigInteger MultiplicativeInverse(BigInteger a, BigInteger m)
		{
			BigInteger x = 0, y = 0;
			ExtendedEuclidean(a, m, out x, out y);
			return x < 0 ? x + m : x;
		}

		/// <summary>
		/// Implements the Miller-Rabin algorithm for fast compositeness test. 
		/// Returns true if n is a composite number. 
		/// If n is prime, returns false with high probability (this probability increases with k).
		/// </summary>
		/// <returns>true if n is a composite number. If n is prime, returns false with high probability (k increases this probability).</returns>
		public static bool MillerRabin(BigInteger n, int k)
		{
			var s = 0;
			var d = n - 1;
			while ((d & 1) == 0)
			{
				d >>= 1;
				s++;
			}

			if (s == 0)
				return true;		// it's a composite

			var rng = new RNGCryptoServiceProvider();
			for (int i = 0; i < k; i++)
			{
				var a = NextRandomBigInt(rng, 2, n - 1);

				var x = BigInteger.ModPow(a, d, n);
				if (x == 1 || x == n - 1)
					continue;

				int j;
				for (j = 1; j < s; j++)
				{
					x = BigInteger.ModPow(x, 2, n);
					if (x == 1)
						return true;		// it's a composite
					if (x == n - 1)
						break;
				}
				if (j == s)
					return true;	// it's a composite
			}
			return false;	// it's a prime with high probability
		}

        public static BigInteger ModSqrRoot(BigInteger val, BigInteger prime)
        {
            return NtlFunctionality.ModSqrRoot(val, prime);
        }

        /// <summary>
        /// Returns a random big integer in the specified range. This is based on a simple probabilistic algorithm.
        /// </summary>
        /// <param name="minValue">Inclusive lower bound.</param>
        /// <param name="maxValue">Exclusive upper bound.</param>
        /// <returns></returns>
        public static BigInteger NextRandomBigInt(RNGCryptoServiceProvider rng, BigInteger minValue, BigInteger maxValue)
		{
			BigInteger n;
			var len = maxValue.ToByteArray().Length;
			var buffer = new byte[len];

			do
			{
				rng.GetBytes(buffer);
				n = new BigInteger(buffer);
			}
			while (n < minValue || n >= maxValue);
			return n;
		}

		/// <summary>
		/// Performs a O(sqrt(|n|)) primality test, where |n| is the bit-length of the input.
		/// </summary>
		public static bool IsPrime(int n)
		{
			int temp;
			for (int i = 2; i <= (int)Math.Sqrt(n); i++)
			{
				temp = n / i;
				if (n == (temp * i))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Returns the inverse of n modulo p.
		/// </summary>
		public static BigInteger CalcInverse(BigInteger n, BigInteger prime)
		{
			BigInteger x, y;
			ExtendedEuclidean(n, prime, out x, out y);
			return x < 0 ? x + prime : x;
		}

		/// <summary>
		/// Returns an array containing inverses of all field elements.
		/// </summary>
		public static int[] GetFieldInverse(int prime)
		{
			int x, y;
			var invArr = new int[prime];
			for (int i = 0; i < prime; i++)
			{
				ExtendedEuclidean(i, prime, out x, out y);
				invArr[i] = x < 0 ? x + prime : x;
			}
			return invArr;
		}

		public static int GetFieldMinimumPrimitive(int prime)
		{
			int w_i;
			bool cond;
			var fieldElements = new bool[prime];

			for (int p = 2; p < prime; p++)
			{
				w_i = 1;
				cond = true;
				for (int i = 1; i < prime; i++)
				{
					fieldElements[i] = false;
				}

				for (int i = 1; i < prime; i++)
				{
					var v = (p * w_i) % prime;
					w_i = v < 0 ? v + prime : v;
					if (fieldElements[w_i])
					{
						cond = false;
						break;
					}
					fieldElements[w_i] = true;
				}
				if (cond)
					return p;
			}
			throw new ArgumentException("Cannot find field primitive for a field from a non-prime number.");
		}

		/// <summary>
		/// Performs modular exponentiation.
		/// </summary>
		public static int ModPow(int powerBase, int exp, int prime)
		{
			int p = 1;
			for (int i = 0; i < exp; i++)
			{
				var v = (powerBase * p) % prime;
				p = v < 0 ? v + prime : v;
			}
			return p;
		}

        /// <summary>
        /// Discrete logarithm encryption.
        /// </summary>
        /// <param name="x">Value to be encrypted.</param>
        /// <param name="p">Prime modulus. Should be large enough (> 1024 bits) to ensure security.</param>
        public static BigInteger DLEncrypt(BigZp x, BigInteger p)
		{
			return BigInteger.ModPow(2, x.Value, p);
		}

		/// <summary>
		/// Returns the actual number of bits of a byte array by ignoring trailing zeros in the binary representation.
		/// </summary>
		public static int GetBitLength(byte[] a)
		{
			bool done = false;
			int k = 0;
			for (int i = a.Length - 1; i >= 0; i--)
			{
				var b = a[i];
				for (int j = 0; j < 8; j++)
				{
					if ((b & 1) == 1)
					{
						done = true;
						break;
					}

					b = (byte)(b >> 1);
					k++;
				}
				if (done)
					break;
			}
			return (a.Length * 8) - k;
		}
        
		/// <summary>
		/// Returns the actual number of bits of a byte array by ignoring trailing zeros in the binary representation.
		/// </summary>
		public static int GetBitLength(BigInteger a)
		{
			return GetBitLength(a.ToByteArray());
		}

        public static int GetBitLength2(BigInteger a)
        {
            var bytes = a.ToByteArray();
            Array.Reverse(bytes);

            int zeros = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    if ((bytes[i] & (1 << j)) > 0)
                    {
                        return bytes.Length * 8 - zeros;
                    }
                    zeros++;
                }
            }

            return 0;
        }

        // Little-endian
        public static List<BigZp> GetBitDecomposition(BigInteger a, BigInteger prime)
        {
            List<BigZp> result = new List<BigZp>();

            var aBytes = a.ToByteArray() ;
      //      Array.Reverse(aBytes);

            int bits = GetBitLength2(a);
           
            for (int i = 0; i < bits; i++)
            {
                result.Add(new BigZp(prime, (aBytes[i / 8] >> (i % 8)) & 0x1));
            }

            return result;
        }

        public static List<BigZp> GetBitDecomposition(BigInteger a, BigInteger prime, int length)
        {
            List<BigZp> result = GetBitDecomposition(a, prime);
            Debug.Assert(result.Count <= length);

            for (int i = result.Count; i < length; i++)
            {
                // pad with zeroes
                result.Add(new BigZp(prime, 0));
            }

            return result;
        }
	}
}
