using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpcLib.DistributedSystem;
using MpcLib.Common.FiniteField;
using MpcLib.Commitments.PolyCommitment;
using System.Numerics;
using System.Diagnostics;
using MpcLib.SecretSharing.eVSS;
using MpcLib.SecretSharing.QuorumShareRenewal;

namespace MpcLib.SecretSharing
{
    public class ShareMultiplicationProtocol : QuorumProtocol<BigZp>
    {
        private BigZp Share1, Share2;
        private BigInteger Prime;
        private int Seed;

        public ShareMultiplicationProtocol(Party me, Quorum quorum, BigZp share1, BigZp share2)
            : base(me, quorum)
        {
            Debug.Assert(share1.Prime == share2.Prime);
            Prime = share1.Prime;
            Share1 = share1;
            Share2 = share2;
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            switch (msg.Type)
            {
                case MsgType.SubProtocolCompleted:
                    Result = (msg as SubProtocolCompletedMsg).Result as BigZp;
                    IsCompleted = true;
                    return;
            }

            Debug.Assert(false);
        }

        public override void Start()
        {
            ExecuteSubProtocol(new QuorumShareRenewalProtocol(Me, Quorum, Quorum, Share1 * Share2, Prime), 4);
        }
    }
}
