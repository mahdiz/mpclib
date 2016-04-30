using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpcLib.DistributedSystem;
using MpcLib.Common.FiniteField;

namespace MpcLib.SecretSharing
{
    public class ShareAdditionProtocol : QuorumProtocol<Share<BigZp>>
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
                return new ShareAdditionProtocol(Me, Quorum, bitShareA, bitShareB);
            }
        }

        public ShareAdditionProtocol(Party me, Quorum quorum, Share<BigZp> share1, Share<BigZp> share2)
            : base(me, quorum)
        {
            Result = new Share<BigZp>(share1.Value + share2.Value, share1.IsPublic && share2.IsPublic);
            IsCompleted = true;
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            // do nothing
        }

        public override void Start()
        {
            // do nothing
        }
    }
}
