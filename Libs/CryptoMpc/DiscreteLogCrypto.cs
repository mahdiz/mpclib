using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MpcLib.DistributedSystem.Mpc.Crypto
{
	/// <summary>
	/// A fast implementation of the discrete logarithm cryptosystem.
	/// Let p be a prime and x be the plaintext. The cipher c is defined 
	/// as c = g^x mod p, where g is a generator modulo p.
	/// Fast modular exponentiation is performed by caching g^(2^i) in an 
	/// array of size log(p), where 0 <= i <= logp . The exponentiation 
	/// is simply done by multiplying the elements of the array that
	/// correspond to the ones in the binary representation of x.
	/// </summary>
	public class DiscreteLogCrypto
	{
		private readonly BigInteger prime;
		private readonly BigInteger[] cache;

		public BigInteger Prime { get { return prime; } }

		public DiscreteLogCrypto(int g, BigInteger p)
		{
			prime = p;
			var n = (int)BigInteger.Log(p, 2);
			cache = new BigInteger[n];

			cache[0] = g;
			for (int i = 1; i < n; i++)
				cache[i] = (cache[i - 1] * cache[i - 1]) % p;
		}

		public BigInteger Encrypt(BigInteger x)
		{
			BigInteger r = 1, rem;
			for (int i = 0; x >= 1; i++)
			{
				x = BigInteger.DivRem(x, 2, out rem);
				if (rem == 1)
					r = BigInteger.Multiply(r, cache[i]) % prime;
			}
			return r;
		}
	}
}
