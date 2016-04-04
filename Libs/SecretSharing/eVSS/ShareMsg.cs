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

		public ShareMsg(T share)
            : base(MsgType.Share)
		{
			Share = share;
		}

        public ShareMsg(T share, MsgType type)
        {
            Type = type;
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

	public class ShareWitnessMsg<T> : ShareMsg<T> where T : ISizable
	{
		public readonly MG Witness;

		public ShareWitnessMsg(T share, MG witness)
			: base(share)
		{
			Witness = witness;
		}

		public override int Size
		{
			get
			{
				return base.Size + Witness.Size;
			}
		}
	}
}
