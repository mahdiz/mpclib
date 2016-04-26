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
            Result = new List<Share<BigZp>>();
            ExecuteForBit(0);
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg.Type == MsgType.SubProtocolCompleted);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;
            Result.Add(completedMsg.Result as Share<BigZp>);

            if (completedMsg.Tag + 1 < BitSharesA.Count)
                ExecuteForBit(completedMsg.Tag + 1);
            else
                IsCompleted = true;
        }

        private void ExecuteForBit(int bit)
        {
            ExecuteSubProtocol(ProtocolFactory.GetBitProtocol(BitSharesA[bit], BitSharesB[bit]), bit);
        }
    }

    public class PrefixOperationProtocol : QuorumProtocol<List<Share<BigZp>>>
    {
        private IList<Share<BigZp>> SharedBits;
        private IBitProtocolFactory ProtocolFactory;

        public PrefixOperationProtocol(Party me, Quorum quorum, IList<Share<BigZp>> sharedBits, IBitProtocolFactory bitProtocolFactory)
            : base(me, quorum)
        {
            SharedBits = sharedBits;
            ProtocolFactory = bitProtocolFactory;
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;

            Result.Add(completedMsg.Result as Share<BigZp>);
            ExecuteNextIfNeeded(completedMsg.Tag - 1);
        }

        public override void Start()
        {
            // we need to iterate over the bits in the opposite order. build the result list in the wrong order then reverse it at the end
            Result = new List<Share<BigZp>>();

            // the first bit of the prefix is the same as the shared bit
            Result.Add(SharedBits[SharedBits.Count - 1]);

            ExecuteNextIfNeeded(SharedBits.Count - 2);
        }

        private void ExecuteNextIfNeeded(int which)
        {
            if (which >= 0)
                ExecuteSubProtocol(ProtocolFactory.GetBitProtocol(SharedBits[which], Result[Result.Count - 1]), which);
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

        Share<BigZp> BitA, BitB, BitSum, BitProd;
        public SharedBitOr(Party me, Quorum quorum, Share<BigZp> bitA, Share<BigZp> bitB)
            : base(me, quorum)
        {
            BitA = bitA;
            BitB = bitB;
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg.Type == MsgType.SubProtocolCompleted);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;
            
            switch (completedMsg.Tag)
            {
                case 0:
                    BitProd = completedMsg.Result as Share<BigZp>;
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, BitA, BitB), 1);
                    return;
                case 1:
                    BitSum = completedMsg.Result as Share<BigZp>;
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, BitSum, new Share<BigZp>(BitProd.Value.AdditiveInverse, BitProd.IsPublic)), 2);
                    return;
                case 2:
                    Result = completedMsg.Result as Share<BigZp>;
                    IsCompleted = true;
                    return;
            }

            Debug.Assert(false);
        }

        public override void Start()
        {
            ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, BitA, BitB), 0);
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

        Share<BigZp> BitA, BitB, BitSum, BitProd;
        public SharedBitXor(Party me, Quorum quorum, Share<BigZp> bitA, Share<BigZp> bitB)
            : base(me, quorum)
        {
            BitA = bitA;
            BitB = bitB;
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg.Type == MsgType.SubProtocolCompleted);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;

            switch (completedMsg.Tag)
            {
                case 0:
                    BitProd = completedMsg.Result as Share<BigZp>;
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, BitA, BitB), 1);
                    break;
                case 1:
                    BitSum = completedMsg.Result as Share<BigZp>;
                    var tempShare = new Share<BigZp>(new BigZp(BitProd.Value.Prime, 2) * (BitProd.Value.AdditiveInverse), BitProd.IsPublic);
                    ExecuteSubProtocol(new ShareAdditionProtocol(Me, Quorum, BitSum, tempShare), 2);
                    break;
                case 2:
                    Result = completedMsg.Result as Share<BigZp>;
                    IsCompleted = true;
                    break;
            }
        }

        public override void Start()
        {
            ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, BitA, BitB), 0);
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

        protected override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg.Type == MsgType.SubProtocolCompleted);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;

            switch (completedMsg.Tag)
            {
                case 0:
                    Result = completedMsg.Result as Share<BigZp>;
                    IsCompleted = true;
                    break;
            }
        }

        public override void Start()
        {
            ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, BitA, BitB), 0);
        }
    }
}
