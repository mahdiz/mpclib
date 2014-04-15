using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace MpcLib.Common.FiniteField
{
	/// <summary>
	/// Represents a big finite field.
	/// </summary>
	public class BigZp : ISizable
	{
		private BigInteger num;
		public readonly BigInteger Prime;

		public BigInteger Value
		{
			get
			{
				return num;
			}

			set
			{
				num = Modulo(value);
			}
		}

		public BigZp AdditiveInverse
		{
			get
			{
				return new BigZp(Prime, -1 * this.num);
			}
		}

		/// <summary>
		/// Multiplicative inverse of the number modulo Prime.
		/// </summary>
		public BigZp MultipInverse
		{
			get
			{
				return new BigZp(Prime, NumTheoryUtils.MultiplicativeInverse(num, Prime));
			}
		}

		public BigZp(BigInteger prime, BigInteger num)
		{
			this.Prime = prime;
			this.num = Modulo(num, prime);
		}

		public BigZp(long prime, long num)
		{
			this.Prime = prime;
			this.num = Modulo(num, prime);
		}

		public BigZp(BigInteger prime)
		{
			this.Prime = prime;
		}

		public BigZp(BigZp toCopy)
		{
			this.Prime = toCopy.Prime;
			this.num = Modulo(toCopy.num, this.Prime);
		}

		public static BigZp operator +(BigZp zp1, BigZp zp2)
		{
			Debug.Assert(zp1.Prime == zp2.Prime, "Finite fields do not match!");
			return new BigZp(zp1.Prime, zp1.num + zp2.num);
		}

		public static BigZp operator -(BigZp zp1, BigZp zp2)
		{
			Debug.Assert(zp1.Prime == zp2.Prime, "Finite fields do not match!");
			return new BigZp(zp1.Prime, zp1.num - zp2.num);
		}

		public static BigZp operator *(BigZp zp1, BigZp zp2)
		{
			Debug.Assert(zp1.Prime == zp2.Prime, "Finite fields do not match!");
			return new BigZp(zp1.Prime, zp1.num * zp2.num);
		}

		public static BigZp operator /(BigZp zp1, BigZp zp2)
		{
			Debug.Assert(zp1.Prime == zp2.Prime, "Finite fields do not match!");
			return zp1 * zp2.MultipInverse;
		}

		public static BigZp operator +(BigZp zp1, BigInteger i)
		{
			return new BigZp(zp1.Prime, zp1.num + i);
		}

		public static BigZp operator ++(BigZp zp1)
		{
			zp1.num = Modulo(zp1.num + 1, zp1.Prime);
			return zp1;
		}

		public static bool operator <(BigZp zp1, BigInteger i)
		{
			return zp1.num < i;
		}

		public static bool operator >(BigZp zp1, BigInteger i)
		{
			return zp1.num > i;
		}

		public static bool operator ==(BigZp zp1, BigZp zp2)
		{
			if (Object.ReferenceEquals(zp1, zp2))
				return true;

			if (((object)zp1 == null) || ((object)zp2 == null))
				return false;

			return zp1.num == zp2.num;
		}

		public static bool operator !=(BigZp zp1, BigZp zp2)
		{
			return !(zp1 == zp2);
		}

		public BigZp Add(BigZp operand)
		{
			num = Modulo(num + operand.Value);
			return this;
		}

		public BigZp ConstAdd(BigZp operand)
		{
			var temp = new BigZp(this);
			temp.num = Modulo(num + operand.Value);
			return temp;
		}

		public BigZp AddListContent(IList<BigZp> zpList)
		{
			foreach (BigZp zp in zpList)
				Add(zp);

			return this;
		}

		public BigZp Sub(BigZp operand)
		{
			num = Modulo(num - operand.Value);
			return this;
		}

		public BigZp ConstSub(BigZp operand)
		{
			var temp = new BigZp(this);
			temp.num = Modulo(num - operand.Value);
			return temp;
		}

		public BigZp Div(BigZp operand)
		{
			if (operand.num == 0)
				throw new ArgumentException("Cannot divide by zero!");

			return this * operand.MultipInverse;
		}

		public BigZp ConstDvide(BigZp operand)
		{
			if (operand.num == 0)
				throw new System.ArgumentException("Cannot divide by zero!");

			return this * operand.MultipInverse;
		}

		public BigZp Mul(BigZp operand)
		{
			num = Modulo(num * operand.Value);
			return this;
		}

		public BigZp ConstMul(BigZp operand)
		{
			var temp = new BigZp(this);
			temp.num = Modulo(num * operand.Value);
			return temp;
		}

		public BigZp Calculate(BigZp operand, Operation operation)
		{
			switch (operation)
			{
				case Operation.Add:
					return Add(operand);
				case Operation.Mul:
					return Mul(operand);
				case Operation.Sub:
					return Sub(operand);
				case Operation.Div:
					return Div(operand);
				default:
					throw new Exception("Unknown operation: " + operation);
			}
		}

		public BigInteger Modulo(BigInteger i)
		{
			return Modulo(i, Prime);
		}

		public static BigInteger Modulo(BigInteger n, BigInteger prime)
		{
			n = n % prime;
			return n < 0 ? n + prime : n;
		}

		public override string ToString()
		{
			return Convert.ToString(num);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is BigZp))
				return false;
			BigZp compare = (BigZp)obj;
			return Value == compare.Value && Prime == compare.Prime;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public int Size
		{
			get
			{
				return num.ToByteArray().Length;
			}
		}
	}
}