using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpcLib.Commitments.PolyCommitment;
using MpcLib.Common;
using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem;

namespace MpcLib.SecretSharing.eVSS
{
	public class ShareMsg<T> : Msg where T : ISizable
	{
		public readonly T Share;
		public readonly MG Witness;

		public ShareMsg(T share, MG witness)
		{
			Share = share;
			Witness = witness;
		}

		public override string ToString()
		{
			return base.ToString() + ", Share=" + Share.ToString();
		}

		public override int Size
		{
			get
			{
				return base.Size + Share.Size + Witness.Size;
			}
		}

		public override int StageKey
		{
			get { return (int)Stage.Share; }
		}
	}
}
