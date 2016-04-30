using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpcLib.DistributedSystem;
using MpcLib.Common.FiniteField;
using System.Numerics;

namespace MpcLib.SecretSharing
{
    public class BitCompositionProtocol : QuorumProtocol<Share<BigZp>>
    {
        List<Share<BigZp>> BitShares;
        BigInteger Prime;

        public BitCompositionProtocol(Party me, Quorum quorum, List<Share<BigZp>> bitShares, BigInteger prime)
            : base(me, quorum)
        {
            BitShares = bitShares;
            Prime = prime;
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
        }

        public override void Start()
        {
            var value = new BigZp(Prime);
            BigZp two = new BigZp(Prime, 2);
            bool isPublic = true;
            for (int i = BitShares.Count - 1; i >= 0; i--)
            {
                value = value * two + BitShares[i].Value;
                isPublic &= BitShares[i].IsPublic;
            }

            Result = new Share<BigZp>(value, isPublic);
            IsCompleted = true;
        }
    }
}
