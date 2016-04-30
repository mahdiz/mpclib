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

        int stage;

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
            stage = 0;
            ExecuteSubProtocol(new ShareLessThanProtocol(Me, Quorum, ShareA, ShareB));
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            var completedMsg = (SubProtocolCompletedMsg)msg;

            switch (stage)
            {
                case 0:
                    Comp = (Share<BigZp>)completedMsg.ResultList[0];
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum,
                        BigZpShareFactory.CreateConstantShare(Prime, 1),
                        BigZpShareFactory.ShareAdditiveInverse(Comp)));
                    break;
                case 1:
                    InvComp = (Share<BigZp>)completedMsg.ResultList[0];

                    ExecuteSubProtocols(new Protocol[]
                    {
                        new ShareMultiplicationProtocol(Me, Quorum, Comp, ShareA),
                        new ShareMultiplicationProtocol(Me, Quorum, InvComp, ShareA),
                        new ShareMultiplicationProtocol(Me, Quorum, Comp, ShareB),
                        new ShareMultiplicationProtocol(Me, Quorum, InvComp, ShareB)
                    });
                    break;
                case 2:
                    {
                        var results = completedMsg.ResultList;
                        ShareAComp = (Share<BigZp>)results[0];
                        ShareAInvComp = (Share<BigZp>)results[1];
                        ShareBComp = (Share<BigZp>)results[2];
                        ShareBInvComp = (Share<BigZp>)results[3];

                        ExecuteSubProtocols(new Protocol[]
                        {
                        new ShareAdditionProtocol(Me, Quorum, ShareAComp, ShareBInvComp),
                        new ShareAdditionProtocol(Me, Quorum, ShareAInvComp, ShareBComp)
                        });
                        break;
                    }
                case 3:
                    {
                        var results = completedMsg.ResultList;
                        LesserShare = (Share<BigZp>)results[0];
                        GreaterShare = (Share<BigZp>)results[1];
                        Result = new Tuple<Share<BigZp>, Share<BigZp>>(LesserShare, GreaterShare);
                        IsCompleted = true;
                        break;
                    }
            }

            stage++;
        }
    }
}
