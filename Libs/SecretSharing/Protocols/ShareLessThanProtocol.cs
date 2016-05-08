using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.SecretSharing
{
    // return (a < b)
    public class ShareLessThanProtocol : QuorumProtocol<Share<BigZp>>
    {
        private Share<BigZp> ShareA, ShareB, ShareAMinusB;
        private Share<BigZp> W, X, Y, XY, XpY;
        private BigInteger Prime;

        private int Stage;

        public ShareLessThanProtocol(Party me, Quorum Quorum, Share<BigZp> shareA, Share<BigZp> shareB)
            : base(me, Quorum)
        {
            Debug.Assert(shareA.Value.Prime == shareB.Value.Prime);

            ShareA = shareA;
            ShareB = shareB;
            Prime = ShareA.Value.Prime;

        }

        public override void Start()
        {
            Stage = 0;
            ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, ShareA, BigZpShareFactory.ShareAdditiveInverse(ShareB)));
        }
        
        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            var completedMsg = msg as SubProtocolCompletedMsg;

            switch (Stage)
            {
                case 0:
                    ShareAMinusB = (Share<BigZp>)completedMsg.SingleResult;

                    ExecuteSubProtocols(new Protocol[]
                    {
                        new LessThanHalfPrime(Me, Quorum, ShareA),
                        new LessThanHalfPrime(Me, Quorum, ShareB),
                        new LessThanHalfPrime(Me, Quorum, ShareAMinusB)
                    });
                    break;
                case 1:
                    W = (Share<BigZp>)completedMsg.ResultList[0];
                    X = (Share<BigZp>)completedMsg.ResultList[1];
                    Y = (Share<BigZp>)completedMsg.ResultList[2];
                    ExecuteSubProtocols(new Protocol[]
                    {
                        new ShareMultiplicationProtocol(Me, Quorum, X, Y),
                        new ShareAdditionProtocol(Me, Quorum, X, Y),
                    });
                    break;
                case 2:
                    XY = (Share<BigZp>)completedMsg.ResultList[0];
                    XpY = (Share<BigZp>)completedMsg.ResultList[1];
                    ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, BigZpShareFactory.CreateConstantShare(Prime, 2), XY));
                    break;
                case 3:
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, XpY, BigZpShareFactory.ShareAdditiveInverse((Share<BigZp>)completedMsg.SingleResult)));
                    break;
                case 4:
                    ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, W, (Share<BigZp>)completedMsg.SingleResult));
                    break;
                case 5:
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, XY, (Share<BigZp>)completedMsg.SingleResult));
                    break;
                case 6:
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, BigZpShareFactory.ShareAdditiveInverse(XpY), (Share<BigZp>)completedMsg.SingleResult));
                    break;
                case 7:
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, BigZpShareFactory.CreateConstantShare(Prime, 1), (Share<BigZp>)completedMsg.SingleResult));
                    break;
                case 8:
                    Result = (Share<BigZp>)completedMsg.SingleResult;
                    IsCompleted = true;
                    break;
            }

            Stage++;
        }
    }

    public class LessThanHalfPrime : QuorumProtocol<Share<BigZp>>
    {
        private Share<BigZp> Share;

        private int Stage;

        public LessThanHalfPrime(Party me, Quorum Quorum, Share<BigZp> share)
            : base(me, Quorum)
        {
            Share = share;
        }

        public override void Start()
        {
            Stage = 0;
            ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, Share, new Share<BigZp>(new BigZp(Share.Value.Prime, 2), true)));
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            var completedMsg = msg as SubProtocolCompletedMsg;

            switch (Stage)
            {
                case 0:
                    ExecuteSubProtocol(new LeastSignificantBitProtocol(Me, Quorum, (Share<BigZp>)completedMsg.SingleResult));
                    break;
                case 1:
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum,
                        BigZpShareFactory.CreateConstantShare(Share.Value.Prime, 1),
                        BigZpShareFactory.ShareAdditiveInverse((Share<BigZp>)completedMsg.SingleResult)));
                    break;
                case 2:
                    Result = (Share<BigZp>)completedMsg.SingleResult;
                    IsCompleted = true;
                    
                    break;
            }

            Stage++;
        }
    }
}
