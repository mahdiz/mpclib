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
        private Share<BigZp> W, X, Y, XY;
        private BigInteger Prime;

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
            ExecuteSubProtocol(new LessThanHalfPrime(Me, Quorum, ShareA), 0);
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            var completedMsg = msg as SubProtocolCompletedMsg;

            switch (completedMsg.Tag)
            {
                case 0:
                    W = (Share<BigZp>)completedMsg.Result;
                    ExecuteSubProtocol(new LessThanHalfPrime(Me, Quorum, ShareB), 1);
                    break;
                case 1:
                    X = (Share<BigZp>)completedMsg.Result;
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, ShareA, BigZpShareFactory.ShareAdditiveInverse(ShareB)), 2);
                    break;
                case 2:
                    ShareAMinusB = (Share<BigZp>)completedMsg.Result;
                    ExecuteSubProtocol(new LessThanHalfPrime(Me, Quorum, ShareAMinusB), 3);
                    break;
                case 3:
                    Y = (Share<BigZp>)completedMsg.Result;
                    ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, X, Y), 4);
                    break;
                case 4:
                    XY = (Share<BigZp>)completedMsg.Result;
                    ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, BigZpShareFactory.CreateConstantShare(Prime, 2), XY), 5);
                    break;
                case 5:
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, Y, BigZpShareFactory.ShareAdditiveInverse((Share<BigZp>)completedMsg.Result)), 6);
                    break;
                case 6:
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, X, (Share<BigZp>)completedMsg.Result), 7);
                    break;
                case 7:
                    ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, W, (Share<BigZp>)completedMsg.Result), 8);
                    break;
                case 8:
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, XY, (Share<BigZp>)completedMsg.Result), 9);
                    break;
                case 9:
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, BigZpShareFactory.ShareAdditiveInverse(Y), (Share<BigZp>)completedMsg.Result), 10);
                    break;
                case 10:
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, BigZpShareFactory.ShareAdditiveInverse(X), (Share<BigZp>)completedMsg.Result), 11);
                    break;
                case 11:
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, BigZpShareFactory.CreateConstantShare(Prime, 1), (Share<BigZp>)completedMsg.Result), 12);
                    break;
                case 12:
                    Result = (Share<BigZp>)completedMsg.Result;
                    IsCompleted = true;
                    break;
            }
        }
    }

    public class LessThanHalfPrime : QuorumProtocol<Share<BigZp>>
    {
        private Share<BigZp> Share;

        public LessThanHalfPrime(Party me, Quorum Quorum, Share<BigZp> share)
            : base(me, Quorum)
        {
            Share = share;
        }

        public override void Start()
        {
            ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, Share, new Share<BigZp>(new BigZp(Share.Value.Prime, 2), true)), 0);
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            var completedMsg = msg as SubProtocolCompletedMsg;

            switch (completedMsg.Tag)
            {
                case 0:
                    ExecuteSubProtocol(new LeastSignificantBitProtocol(Me, Quorum, (Share<BigZp>)completedMsg.Result), 1);
                    break;
                case 1:
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, 
                        BigZpShareFactory.CreateConstantShare(Share.Value.Prime, 1),
                        BigZpShareFactory.ShareAdditiveInverse((Share<BigZp>)completedMsg.Result)),
                        2);
                    break;
                case 2:
                    Result = (Share<BigZp>)completedMsg.Result;
                    IsCompleted = true;
                    break;

            }
        }
    }
}
