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

        public LeastSignificantBitProtocol(Party me, Quorum quorum, Share<BigZp> share)
            : base(me, quorum)
        {
            Share = share;
            Prime = Share.Value.Prime;
        }

        public override void Start()
        {
            ExecuteSubProtocol(new RandomBitwiseGenProtocol(Me, Quorum, Prime, Prime), 0);
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;

            Console.WriteLine("LSB Done Stage " + completedMsg.Tag);

            switch (completedMsg.Tag)
            {
                case 0:
                    BitwiseRand = completedMsg.Result as List<Share<BigZp>>;
                    ExecuteSubProtocol(new BitCompositionProtocol(Me, Quorum, BitwiseRand, Prime), 1);
                    break;
                case 1:
                    Rand = completedMsg.Result as Share<BigZp>;
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, Rand, Share), 2);
                    break;
                case 2:
                    PaddedShare = completedMsg.Result as Share<BigZp>;
                    ExecuteSubProtocol(new ReconstructionProtocol(Me, Quorum, PaddedShare), 3);
                    break;
                case 3:
                    RevealedPadded = completedMsg.Result as BigZp;
                    BigZp lowBit = new BigZp(Prime, RevealedPadded.Value.IsEven ? 0 : 1);
                    ExecuteSubProtocol(new SharedBitXor(Me, Quorum, new Share<BigZp>(lowBit, true), BitwiseRand[0]), 4);
                    break;
                case 4:
                    X = completedMsg.Result as Share<BigZp>;

                    var bitwiseRevealedPadded = NumTheoryUtils.GetBitDecomposition(RevealedPadded.Value, Prime, BitwiseRand.Count);

                    var bitwiseRevealedPaddedShares = new List<Share<BigZp>>();
                    foreach (var bit in bitwiseRevealedPadded)
                    {
                        bitwiseRevealedPaddedShares.Add(new Share<BigZp>(bit, true));
                    }

                    ExecuteSubProtocol(new BitwiseLessThanProtocol(Me, Quorum, bitwiseRevealedPaddedShares, BitwiseRand), 5);
                    break;
                case 5:
                    Y = completedMsg.Result as Share<BigZp>;
                    ExecuteSubProtocol(new SharedBitXor(Me, Quorum, X, Y), 6);
                    break;
                case 6:
                    Result = completedMsg.Result as Share<BigZp>;
                    IsCompleted = true;
                    break;
            }

        }
    }
}
