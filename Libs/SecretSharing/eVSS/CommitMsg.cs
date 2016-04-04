using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpcLib.Commitments.PolyCommitment;
using MpcLib.DistributedSystem;

namespace MpcLib.SecretSharing.eVSS
{
	public class CommitMsg : Msg
	{
		public readonly MG Commitment;

		public CommitMsg(MG commitment)
		{
            Type = MsgType.Commit;
			Commitment = commitment;
		}
	}
}
