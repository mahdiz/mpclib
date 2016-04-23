using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpcLib.DistributedSystem;
using MpcLib.Common.FiniteField;

namespace MpcLib.SecretSharing.Protocols
{
    public class ShareAdditionProtocol : QuorumProtocol<BigZp>
    {
        public ShareAdditionProtocol(Party me, Quorum quorum, BigZp share1, BigZp share2)
            : base(me, quorum)
        {
            Result = share1 + share2;
            IsCompleted = true;
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            // do nothing
        }

        public override void Start()
        {
            // do nothing
        }
    }
}
