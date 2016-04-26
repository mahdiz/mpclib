using MpcLib.Common;
using MpcLib.Common.FiniteField;
using System.Numerics;

namespace MpcLib.SecretSharing
{
	public class Share<T> : ISizable where T : ISizable
	{
		public readonly T Value;
        public readonly bool IsPublic;

		public Share(T value, bool isPublic)
		{
			Value = value;
            IsPublic = isPublic;
		}

        public Share(T value)
            : this(value, false)
        {
        }

		public Share()
		{
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public int Size
		{
			get
			{
				return Value.Size;
			}
		}
	}

    public static class BigZpShareFactory
    {
        public static Share<BigZp> CreateConstantShare(BigInteger prime, BigInteger value)
        {
            return new Share<BigZp>(new BigZp(prime, value), true);
        }

        public static Share<BigZp> ShareAdditiveInverse(Share<BigZp> orig)
        {
            return new Share<BigZp>(orig.Value.AdditiveInverse, orig.IsPublic);
        }
    }
    
}