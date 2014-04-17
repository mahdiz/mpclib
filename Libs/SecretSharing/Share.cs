using MpcLib.Common;
using MpcLib.Common.FiniteField;

namespace MpcLib.SecretSharing
{
	public class Share<T> : ISizable where T : ISizable
	{
		public readonly T Value;

		public Share(T value)
		{
			Value = value;
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
}