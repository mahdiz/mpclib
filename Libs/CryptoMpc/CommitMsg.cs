using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MpcLib.Common.FiniteField;
using MpcLib.MpcProtocols;

namespace MpcLib.MpcProtocols.Crypto
{
	public class DlCommitMsg : MpcMsg
	{
		public readonly IList<BigInteger> Commitments;

		public DlCommitMsg(IList<BigInteger> commitments, Stage stage)
			: base(stage)
		{
			Commitments = commitments;
		}
	}
}