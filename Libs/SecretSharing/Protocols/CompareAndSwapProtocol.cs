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
    public class CompareAndSwapProtocol : QuorumProtocol<Share<BigZp>[]>
    {
        private BigInteger Prime;
        private Share<BigZp> ShareA, ShareB;
        private Share<BigZp> Comp;

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
                    ExecuteSubProtocol(new ConditionalSwapProtocol(Me, Quorum, ShareA, ShareB, Comp));
                    break;
                case 1:
                    Result = (Share<BigZp>[])completedMsg.SingleResult;
                    IsCompleted = true;
                    break;
            }

            stage++;
        }
    }

    public class TaggedCompareAndSwapProtocol : QuorumProtocol<Tuple<Share<BigZp>, Share<BigZp>>[]>
    {
        private BigInteger Prime;
        private Share<BigZp> CompShareA, CompShareB, TagShareA, TagShareB;
        private Share<BigZp> Comp;

        int stage;

        public TaggedCompareAndSwapProtocol(Party me, Quorum quorum, Tuple<Share<BigZp>, Share<BigZp>> taggedShareA, Tuple<Share<BigZp>, Share<BigZp>> taggedShareB)
            : base(me, quorum)
        {
            Debug.Assert(taggedShareA.Item1.Value.Prime == taggedShareB.Item1.Value.Prime);
            CompShareA = taggedShareA.Item1;
            CompShareB = taggedShareB.Item1;
            TagShareA = taggedShareA.Item2;
            TagShareB = taggedShareB.Item2;
            Prime = CompShareA.Value.Prime;
        }

        public override void Start()
        {
            stage = 0;
            ExecuteSubProtocol(new ShareLessThanProtocol(Me, Quorum, CompShareA, CompShareB));
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            var completedMsg = (SubProtocolCompletedMsg)msg;

            switch (stage)
            {
                case 0:
                    Comp = (Share<BigZp>)completedMsg.ResultList[0];
                    ExecuteSubProtocols(new Protocol[]
                    {
                        new ConditionalSwapProtocol(Me, Quorum, CompShareA, CompShareB, Comp),
                        new ConditionalSwapProtocol(Me, Quorum, TagShareA, TagShareB, Comp)
                    });
                    break;
                case 1:
                    var results = completedMsg.ResultList.Cast<Share<BigZp>[]>().ToArray();
                    Result = new Tuple<Share<BigZp>, Share<BigZp>>[]
                    {
                        new Tuple<Share<BigZp>, Share<BigZp>>(results[0][0], results[1][0]),
                        new Tuple<Share<BigZp>, Share<BigZp>>(results[0][1], results[1][1])
                    };

                    IsCompleted = true;
                    break;
            }

            stage++;
        }
    }

    public class ConditionalSwapProtocol : QuorumProtocol<Share<BigZp>[]>
    {

        private Share<BigZp> ShareA, ShareB, SwapBit, InvSwapBit;
        private BigInteger Prime;
        private int Stage;

        public ConditionalSwapProtocol(Party me, Quorum quorum, Share<BigZp> shareA, Share<BigZp> shareB, Share<BigZp> swapBit)
            : base(me, quorum)
        {
            Debug.Assert(shareA.Value.Prime == shareB.Value.Prime);
            ShareA = shareA;
            ShareB = shareB;
            SwapBit = swapBit;
            Prime = ShareA.Value.Prime;
        }


        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            var completedMsg = (SubProtocolCompletedMsg)msg;

            switch (Stage)
            {
                case 0:
                    InvSwapBit = (Share<BigZp>)completedMsg.ResultList[0];
                    ExecuteSubProtocols(new Protocol[]
                    {
                            new ShareMultiplicationProtocol(Me, Quorum, SwapBit, ShareA),
                            new ShareMultiplicationProtocol(Me, Quorum, InvSwapBit, ShareA),
                            new ShareMultiplicationProtocol(Me, Quorum, SwapBit, ShareB),
                            new ShareMultiplicationProtocol(Me, Quorum, InvSwapBit, ShareB)
                    });
                    break;
                case 1:
                    var results = completedMsg.ResultList;
                    ExecuteSubProtocols(new Protocol[]
                    {
                            new ShareAdditionProtocol(Me, Quorum, (Share<BigZp>)results[0], (Share<BigZp>)results[3]),
                            new ShareAdditionProtocol(Me, Quorum, (Share<BigZp>)results[1], (Share<BigZp>)results[2])
                    });
                    break;
                case 2:
                    var lesser = (Share<BigZp>)completedMsg.ResultList[0];
                    var greater = (Share<BigZp>)completedMsg.ResultList[1];
                    Result = new Share<BigZp>[] { lesser, greater };
                    IsCompleted = true;
                    break;
            }
            Stage++;
        }

        public override void Start()
        {
            Stage = 0;
            ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum,
                        BigZpShareFactory.CreateConstantShare(Prime, 1),
                        BigZpShareFactory.ShareAdditiveInverse(SwapBit)));
        }
    }
}

