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

    public interface IBitProtocolFactory
    {
        Protocol<Share<BigZp>> GetBitProtocol(Share<BigZp> bitShareA, Share<BigZp> bitShareB);
    }

    public class BitwiseOperationProtocol : QuorumProtocol<List<Share<BigZp>>>
    {
        IList<Share<BigZp>> BitSharesA, BitSharesB;
        private IBitProtocolFactory ProtocolFactory;

        public BitwiseOperationProtocol(Party me, Quorum quorum, IList<Share<BigZp>> bitSharesA, IList<Share<BigZp>> bitSharesB, IBitProtocolFactory bitProtocolFactory)
            : base(me, quorum)
        {
            Debug.Assert(bitSharesA.Count == bitSharesB.Count);

            BitSharesA = bitSharesA;
            BitSharesB = bitSharesB;
            ProtocolFactory = bitProtocolFactory;
        }

        public override void Start()
        {
            List<Protocol> bitProtocols = new List<Protocol>();

            for (int i = 0; i < BitSharesA.Count; i++)
            {
                bitProtocols.Add(ProtocolFactory.GetBitProtocol(BitSharesA[i], BitSharesB[i]));
            }

            ExecuteSubProtocols(bitProtocols);
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg.Type == MsgType.SubProtocolCompleted);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;
            Result = completedMsg.ResultList.Cast<Share<BigZp>>().ToList();
            IsCompleted = true;
        }
    }

    public class PrefixOperationProtocol : QuorumProtocol<List<Share<BigZp>>>
    {
        private IList<Share<BigZp>> SharedBits;
        private IBitProtocolFactory ProtocolFactory;

        private int WhichBit;

        public PrefixOperationProtocol(Party me, Quorum quorum, IList<Share<BigZp>> sharedBits, IBitProtocolFactory bitProtocolFactory)
            : base(me, quorum)
        {
            SharedBits = sharedBits;
            ProtocolFactory = bitProtocolFactory;
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;
            Result.Add((Share<BigZp>)completedMsg.SingleResult);

            WhichBit--;
            ExecuteNextIfNeeded();
        }

        public override void Start()
        {

            // we need to iterate over the bits in the opposite order. build the result list in the wrong order then reverse it at the end
            Result = new List<Share<BigZp>>();

            // the first bit of the prefix is the same as the shared bit
            Result.Add(SharedBits[SharedBits.Count - 1]);

            WhichBit = SharedBits.Count - 2;

            ExecuteNextIfNeeded();
        }

        private void ExecuteNextIfNeeded()
        {
            if (WhichBit >= 0)
                ExecuteSubProtocol(ProtocolFactory.GetBitProtocol(SharedBits[WhichBit], Result[Result.Count - 1]));
            else
            {
                Result.Reverse();
                IsCompleted = true;
            }
        }
    }

    public class SharedBitOr : QuorumProtocol<Share<BigZp>>
    {
        public class ProtocolFactory : IBitProtocolFactory
        {
            Party Me;
            Quorum Quorum;
            public ProtocolFactory(Party me, Quorum quorum)
            {
                Me = me;
                Quorum = quorum;
            }

            public Protocol<Share<BigZp>> GetBitProtocol(Share<BigZp> bitShareA, Share<BigZp> bitShareB)
            {
                return new SharedBitOr(Me, Quorum, bitShareA, bitShareB);
            }
        }

        private Share<BigZp> BitA, BitB, BitSum, BitProd;
        private int Stage;
        public SharedBitOr(Party me, Quorum quorum, Share<BigZp> bitA, Share<BigZp> bitB)
            : base(me, quorum)
        {
            BitA = bitA;
            BitB = bitB;
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg.Type == MsgType.SubProtocolCompleted);
            
            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;

            switch (Stage)
            {
                case 0:
                    BitProd = (Share<BigZp>)completedMsg.SingleResult;
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, BitA, BitB));
                    break;
                case 1:
                    BitSum = (Share<BigZp>)completedMsg.SingleResult;
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, BitSum, new Share<BigZp>(BitProd.Value.AdditiveInverse, BitProd.IsPublic)));
                    break;
                case 2:
                    Result = (Share<BigZp>)completedMsg.SingleResult;
                    IsCompleted = true;
                    break;
            }

            Stage++;
        }

        public override void Start()
        {
            Stage = 0;
            ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, BitA, BitB));
            
        }
    }

    public class SharedBitXor : QuorumProtocol<Share<BigZp>>
    {
        public class ProtocolFactory : IBitProtocolFactory
        {
            Party Me;
            Quorum Quorum;
            public ProtocolFactory(Party me, Quorum quorum)
            {
                Me = me;
                Quorum = quorum;
            }

            public Protocol<Share<BigZp>> GetBitProtocol(Share<BigZp> bitShareA, Share<BigZp> bitShareB)
            {
                return new SharedBitXor(Me, Quorum, bitShareA, bitShareB);
            }
        }

        private Share<BigZp> BitA, BitB, BitSum, BitProd;
        private int Stage;
        public SharedBitXor(Party me, Quorum quorum, Share<BigZp> bitA, Share<BigZp> bitB)
            : base(me, quorum)
        {
            BitA = bitA;
            BitB = bitB;
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg.Type == MsgType.SubProtocolCompleted);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;

            switch (Stage)
            {
                case 0:
                    BitProd = (Share<BigZp>)completedMsg.SingleResult;
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, BitA, BitB));
                    break;
                case 1:
                    BitSum = (Share<BigZp>)completedMsg.SingleResult;
                    var tempShare = new Share<BigZp>(new BigZp(BitProd.Value.Prime, 2) * (BitProd.Value.AdditiveInverse), BitProd.IsPublic);
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, BitSum, tempShare));
                    break;
                case 2:
                    Result = (Share<BigZp>)completedMsg.SingleResult;
                    IsCompleted = true;
                    break;
            }
            Stage++;
        }

        public override void Start()
        {
            Stage = 0;
            ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, BitA, BitB));
        }
    }

    public class SharedBitAnd : QuorumProtocol<Share<BigZp>>
    {
        public class ProtocolFactory : IBitProtocolFactory
        {
            Party Me;
            Quorum Quorum;
            public ProtocolFactory(Party me, Quorum quorum)
            {
                Me = me;
                Quorum = quorum;
            }

            public Protocol<Share<BigZp>> GetBitProtocol(Share<BigZp> bitShareA, Share<BigZp> bitShareB)
            {
                return new SharedBitAnd(Me, Quorum, bitShareA, bitShareB);
            }
        }

        Share<BigZp> BitA, BitB;
        public SharedBitAnd(Party me, Quorum quorum, Share<BigZp> bitA, Share<BigZp> bitB)
            : base(me, quorum)
        {
            BitA = bitA;
            BitB = bitB;
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg.Type == MsgType.SubProtocolCompleted);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;
            Result = (Share<BigZp>) completedMsg.SingleResult;
            IsCompleted = true;
        }

        public override void Start()
        {
            ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, BitA, BitB));
        }
    }
}
