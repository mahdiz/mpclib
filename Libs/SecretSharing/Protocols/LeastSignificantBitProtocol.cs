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
    public class LeastSignificantBitProtocol : QuorumProtocol<Share<BigZp>>
    {
        BigInteger Prime;
        Share<BigZp> Share;

        List<Share<BigZp>> BitwiseRand;
        Share<BigZp> Rand;

        Share<BigZp> PaddedShare;
        BigZp RevealedPadded;

        Share<BigZp> X, Y;

        private int Stage;

        public LeastSignificantBitProtocol(Party me, Quorum quorum, Share<BigZp> share)
            : base(me, quorum)
        {
            Share = share;
            Prime = Share.Value.Prime;
        }

        public override void Start()
        {
            Stage = 0;
            ExecuteSubProtocol(new RandomBitwiseGenProtocol(Me, Quorum, Prime, Prime));
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;

            switch (Stage)
            {
                case 0:
                    BitwiseRand = (List<Share<BigZp>>)completedMsg.SingleResult;
                    ExecuteSubProtocol(new BitCompositionProtocol(Me, Quorum, BitwiseRand, Prime));
                    break;
                case 1:
                    Rand = (Share<BigZp>)completedMsg.SingleResult;
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, Rand, Share));
                    break;
                case 2:
                    PaddedShare = (Share<BigZp>)completedMsg.SingleResult;
                    ExecuteSubProtocol(new ReconstructionProtocol(Me, Quorum, PaddedShare));
                    break;
                case 3:
                    RevealedPadded = (BigZp)completedMsg.SingleResult;
                    BigZp lowBit = new BigZp(Prime, RevealedPadded.Value.IsEven ? 0 : 1);
                    ExecuteSubProtocol(new SharedBitXor(Me, Quorum, new Share<BigZp>(lowBit, true), BitwiseRand[0]));
                    break;
                case 4:
                    X = (Share<BigZp>)completedMsg.SingleResult;

                    var bitwiseRevealedPadded = NumTheoryUtils.GetBitDecomposition(RevealedPadded.Value, Prime, BitwiseRand.Count);

                    var bitwiseRevealedPaddedShares = new List<Share<BigZp>>();
                    foreach (var bit in bitwiseRevealedPadded)
                    {
                        bitwiseRevealedPaddedShares.Add(new Share<BigZp>(bit, true));
                    }

                    ExecuteSubProtocol(new BitwiseLessThanProtocol(Me, Quorum, bitwiseRevealedPaddedShares, BitwiseRand));
                    break;
                case 5:
                    Y = (Share<BigZp>)completedMsg.SingleResult;
                    ExecuteSubProtocol(new SharedBitXor(Me, Quorum, X, Y));
                    break;
                case 6:
                    Result = (Share<BigZp>)completedMsg.SingleResult;
                    IsCompleted = true;
                    break;
            }

            Stage++;

        }
    }
}
