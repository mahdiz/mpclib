using System.Collections.Generic;
using MpcLib.Common;
using MpcLib.SecretSharing;

namespace MpcLib.MpcProtocols
{
	public class ShareMsg<T> : MpcMsg where T : ISizable
	{
		public readonly Share<T> Share;

		public ShareMsg(Share<T> share, Stage stage)
			: base(stage)
		{
			Share = share;
		}

		public override string ToString()
		{
			return base.ToString() + ", Share=" + Share.ToString();
		}

		public override int Size
		{
			get
			{
				return base.Size + Share.Size;
			}
		}
	}
}