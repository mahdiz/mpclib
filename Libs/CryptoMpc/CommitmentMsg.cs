using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MpcLib.Common.FiniteField;

namespace MpcLib.DistributedSystem.Mpc.Crypto
{
	public class CommitmentMsg : MpcMsg
	{
		public readonly IList<BigInteger> Commitments;

		public CommitmentMsg(IList<BigInteger> commitments, Stage stage)
			: base(stage)
		{
			Commitments = commitments;
		}
	}
}