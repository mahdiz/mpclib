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
    public class ShareMultiplicationProtocol : QuorumProtocol<Share<BigZp>>
    {
        public class ProtocolFactory : IBitProtocolFactory
        {
            Party Me;
            Quorum Quorum;
            public ProtocolFactory(Party me, Quorum quorum)
            {
                Me = me;
                Quorum = quorum;
            }

            public Protocol<Share<BigZp>> GetBitProtocol(Share<BigZp> bitShareA, Share<BigZp> bitShareB)
            {
                return new ShareMultiplicationProtocol(Me, Quorum, bitShareA, bitShareB);
            }
        }

        private Share<BigZp> Share1, Share2;
        private BigInteger Prime;

        public ShareMultiplicationProtocol(Party me, Quorum quorum, Share<BigZp> share1, Share<BigZp> share2)
            : base(me, quorum)
        {
            Debug.Assert(share1.Value.Prime == share2.Value.Prime);
            Prime = share1.Value.Prime;
            Share1 = share1;
            Share2 = share2;
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            switch (msg.Type)
            {
                case MsgType.SubProtocolCompleted:
                    Result = (msg as SubProtocolCompletedMsg).Result as Share<BigZp>;
                    IsCompleted = true;
                    return;
            }

            Debug.Assert(false);
        }

        public override void Start()
        {
            var newShare = new Share<BigZp>(Share1.Value * Share2.Value, Share1.IsPublic && Share2.IsPublic);
            if (Share1.IsPublic || Share2.IsPublic)
            {
                Result = newShare;
                IsCompleted = true;
            }
            else
                ExecuteSubProtocol(new QuorumShareRenewalProtocol(Me, Quorum, Quorum, newShare, Prime), 4);
        }
    }
}
