using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem;
using MpcLib.SecretSharing.eVSS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.SecretSharing
{
    public class ReconstructionProtocol : QuorumProtocol<BigZp>
    {
        private Share<BigZp> Share;
        private BigInteger Prime;


        private Dictionary<int, BigZp> ReconstRecv;
        
        public ReconstructionProtocol(Party me, Quorum quorum, Share<BigZp> share)
            : base(me, quorum)
        {
            Share = share;
            Prime = share.Value.Prime;
            ReconstRecv = new Dictionary<int, BigZp>();
        }

        public override void Start()
        {
            if (Share.IsPublic)
            {
                Result = Share.Value;
                IsCompleted = true;
            }
            else
                QuorumBroadcast(new ShareMsg<BigZp>(Share.Value, MsgType.Reconst));
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg.Type == MsgType.Reconst);
            ReconstRecv[fromId] = (msg as ShareMsg<BigZp>).Share;

            if (ReconstRecv.Count == Quorum.Size)
            {
                // reconstruct the output
                var orderedShares = ReconstRecv.OrderBy(p => p.Key).Select(p => p.Value).ToList();
                int polyDegree = (int)Math.Ceiling(Quorum.Size / 3.0) - 1;

                Result = BigShamirSharing.Recombine(orderedShares, polyDegree, Prime);
                IsCompleted = true;
                // Error-correction procedure
                //var xValues = new List<BigZp>();
                //for (int i = 1; i <= reconstRecv.Count; i++)
                //    xValues.Add(new BigZp(Prime, i));

                //var fixedShares = WelchBerlekampDecoder.Decode(xValues, reconstRecv, PolyDegree, PolyDegree, Prime);

                //// interpolate again
                //return BigShamirSharing.Recombine(fixedShares, PolyDegree, Prime);
            }
        }
    }
}
