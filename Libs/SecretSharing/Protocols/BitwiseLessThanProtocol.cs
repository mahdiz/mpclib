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
    public class BitwiseLessThanProtocol : QuorumProtocol<Share<BigZp>>
    {

        private List<Share<BigZp>> BitSharesA, BitSharesB;

        private List<Share<BigZp>> C, D, E, y;

        public BitwiseLessThanProtocol(Party me, Quorum quorum, List<Share<BigZp>> aBitShares, List<Share<BigZp>> bBitShares)
            : base(me, quorum)
        {
            Debug.Assert(aBitShares.Count == bBitShares.Count);

            BitSharesA = aBitShares;
            BitSharesB = bBitShares;
        }

        public override void Start()
        {
            ExecuteSubProtocol(new BitwiseOperationProtocol(Me, Quorum, BitSharesA, BitSharesB, new SharedBitXor.ProtocolFactory(Me, Quorum)), 0);
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;

          //  Console.WriteLine("BLT Done Stage " + completedMsg.Tag);

            switch (completedMsg.Tag)
            {
                case 0:
                    C = completedMsg.Result as List<Share<BigZp>>;
                    ExecuteSubProtocol(new PrefixOperationProtocol(Me, Quorum, C, new SharedBitOr.ProtocolFactory(Me, Quorum)), 1);
                    return;
                case 1:
                    D = completedMsg.Result as List<Share<BigZp>>;
                    ExecuteSubtractionStep();
                    return;
                case 2:
                    E = completedMsg.Result as List<Share<BigZp>>;
                    E.Add(D[D.Count - 1]); // add the high order bit since it didn't do subtraction
                    ExecuteSubProtocol(new BitwiseOperationProtocol(Me, Quorum, BitSharesB, E, new ShareMultiplicationProtocol.ProtocolFactory(Me, Quorum)), 3);
                    return;
                case 3:
                    y = completedMsg.Result as List<Share<BigZp>>;
                    // use the prefix protocol with addition, to add all shares together, and then grab the lowest order value
                    ExecuteSubProtocol(new PrefixOperationProtocol(Me, Quorum, y, new ShareAdditionProtocol.ProtocolFactory(Me, Quorum)), 4);
                    return;
                case 4:
                    Result = (completedMsg.Result as List<Share<BigZp>>)[0];
                    IsCompleted = true;
                    return;
            }
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
                    DUpper[i] = new Share<BigZp>(DUpper[i].Value.AdditiveInverse, DUpper[i].IsPublic); // make this a subtraction
                }

                ExecuteSubProtocol(new BitwiseOperationProtocol(Me, Quorum, DLower, DUpper, new ShareAdditionProtocol.ProtocolFactory(Me, Quorum)), 2);
            }
            else
            {
                // only 1 bit in input. no subtraction necessary
                ExecuteSubProtocol(new NopProtocol(Me, new List<Share<BigZp>>()), 2);
            }
        }

    }
}
