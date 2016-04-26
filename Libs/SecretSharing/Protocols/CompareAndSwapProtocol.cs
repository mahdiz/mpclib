using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpcLib.DistributedSystem;
using MpcLib.Common.FiniteField;
using System.Diagnostics;
using System.Numerics;

namespace MpcLib.SecretSharing
{
    public class CompareAndSwapProtocol : QuorumProtocol<Tuple<Share<BigZp>, Share<BigZp>>>
    {
        private BigInteger Prime;
        private Share<BigZp> ShareA, ShareB;
        private Share<BigZp> Comp, InvComp;

        private Share<BigZp> ShareAComp, ShareAInvComp, ShareBComp, ShareBInvComp;

        private Share<BigZp> LesserShare, GreaterShare;

        public CompareAndSwapProtocol(Party me, Quorum quorum, Share<BigZp> shareA, Share<BigZp> shareB)
            : base(me, quorum)
        {
            Debug.Assert(shareA.Value.Prime == shareB.Value.Prime);
            ShareA = shareA;
            ShareB = shareB;
            Prime = shareA.Value.Prime;
        }

        public override void Start()
        {
            ExecuteSubProtocol(new ShareLessThanProtocol(Me, Quorum, ShareA, ShareB), 0);
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            var completedMsg = (SubProtocolCompletedMsg)msg;

            switch (completedMsg.Tag)
            {
                case 0:
                    Comp = (Share<BigZp>)completedMsg.Result;
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, 
                        BigZpShareFactory.CreateConstantShare(Prime, 1), 
                        BigZpShareFactory.ShareAdditiveInverse(Comp)), 1);
                    break;
                case 1:
                    InvComp = (Share<BigZp>)completedMsg.Result;
                    ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, Comp, ShareA), 2);
                    break;
                case 2:
                    ShareAComp = (Share<BigZp>)completedMsg.Result;
                    ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, InvComp, ShareA), 3);
                    break;
                case 3:
                    ShareAInvComp = (Share<BigZp>)completedMsg.Result;
                    ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, Comp, ShareB), 4);
                    break;
                case 4:
                    ShareBComp = (Share<BigZp>)completedMsg.Result;
                    ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, InvComp, ShareB), 5);
                    break;
                case 5:
                    ShareBInvComp = (Share<BigZp>)completedMsg.Result;
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, ShareAComp, ShareBInvComp), 6);
                    break;
                case 6:
                    LesserShare = (Share<BigZp>)completedMsg.Result;
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, ShareAInvComp, ShareBComp), 7);
                    break;
                case 7:
                    GreaterShare = (Share<BigZp>)completedMsg.Result;
                    Result = new Tuple<Share<BigZp>, Share<BigZp>>(LesserShare, GreaterShare);
                    IsCompleted = true;
                    break;
            }
        }
    }
}
