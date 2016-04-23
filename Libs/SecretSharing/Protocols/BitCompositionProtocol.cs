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
    public class BitCompositionProtocol : QuorumProtocol<BigZp>
    {
        List<BigZp> BitShares;
        BigInteger Prime;

        public BitCompositionProtocol(Party me, Quorum quorum, List<BigZp> bitShares, BigInteger prime)
            : base(me, quorum)
        {
            BitShares = bitShares;
            Prime = prime;
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
        }

        public override void Start()
        {
            Result = new BigZp(Prime);
            BigZp two = new BigZp(Prime, 2);
            for (int i = BitShares.Count - 1; i >= 0; i--)
            {
                Result = Result * two + BitShares[i];
            }

            IsCompleted = true;
        }
    }
}
