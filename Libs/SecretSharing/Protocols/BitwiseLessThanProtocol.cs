using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.SecretSharing
{
    /// <summary>
    /// Implements the "Bitwise Less-Than" protocol of Nishide and Ohta (PKC 2007).
    /// Given two bitwise secret-shared values a and b, the parties jointly compute a secret-sharing of the bit (a < b).
    /// </summary>
    public class BitwiseLessThanProtocol : QuorumProtocol<Share<BigZp>>
    {
        private List<Share<BigZp>> BitSharesA, BitSharesB;

        private List<Share<BigZp>> C, D, E, y;

        private int Stage;

        private List<Share<BigZp>> baclone;

        public BitwiseLessThanProtocol(Party me, Quorum quorum, List<Share<BigZp>> aBitShares, List<Share<BigZp>> bBitShares)
            : base(me, quorum)
        {
            Debug.Assert(aBitShares.Count == bBitShares.Count);

            BitSharesA = aBitShares;
            BitSharesB = bBitShares;
            baclone = new List<Share<BigZp>>(BitSharesA);
        }

        public override void Start()
        {
            ExecuteSubProtocol(new BitwiseOperationProtocol(Me, Quorum, BitSharesA, BitSharesB, new SharedBitXor.ProtocolFactory(Me, Quorum)));
            Stage = 0;
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;

            var stageResult = (List<Share<BigZp>>)completedMsg.SingleResult;

            switch (Stage)
            {
                case 0:
                    C = stageResult;
                    ExecuteSubProtocol(new PrefixOperationProtocol(Me, Quorum, C, new SharedBitOr.ProtocolFactory(Me, Quorum)));
                    break;
                case 1:
                    D = stageResult;
                    ExecuteSubtractionStep();
                    break;
                case 2:
                    E = stageResult;
                    E.Add(D[D.Count - 1]); // add the high order bit since it didn't do subtraction
                    ExecuteSubProtocol(new BitwiseOperationProtocol(Me, Quorum, BitSharesB, E, new ShareMultiplicationProtocol.ProtocolFactory(Me, Quorum)));
                    break;
                case 3:
                    y = stageResult;
                    // use the prefix protocol with addition, to add all shares together, and then grab the lowest order value
                    ExecuteSubProtocol(new PrefixOperationProtocol(Me, Quorum, y, new ShareAdditionProtocol.ProtocolFactory(Me, Quorum)));
                    break;
                case 4:
                    Result = stageResult[0];

                    IsCompleted = true;
                    break;
            }

            Stage++;
        }

        private void ExecuteSubtractionStep()
        {
            var DLower = new List<Share<BigZp>>(D);
            var DUpper = new List<Share<BigZp>>(D);

            DLower.RemoveAt(DLower.Count - 1); // remove the highest order bit
            DUpper.RemoveAt(0);                // shift right

            if (DLower.Count > 0)
            {
                for (int i = 0; i < DUpper.Count; i++)
                {
                    DUpper[i] = BigZpShareFactory.ShareAdditiveInverse(DUpper[i]); // make this a subtraction
                }

                ExecuteSubProtocol(new BitwiseOperationProtocol(Me, Quorum, DLower, DUpper, new ShareAdditionProtocol.ProtocolFactory(Me, Quorum)));
            }
            else
            {
                // only 1 bit in input. no subtraction necessary
                ExecuteSubProtocol(new LoopbackProtocol(Me, new List<Share<BigZp>>(), Quorum.GetNextProtocolId()));
            }
        }

    }
}
