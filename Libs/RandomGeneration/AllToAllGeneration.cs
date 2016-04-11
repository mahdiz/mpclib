using System;
using System.Collections.Generic;
using System.Numerics;
using MpcLib.Commitments.PolyCommitment;
using MpcLib.Common;
using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem;
using MpcLib.SecretSharing.eVSS;
using MpcLib.Common.StochasticUtils;
using MpcLib.SecretSharing;

namespace MpcLib.RandomGeneration.AllToAllGeneration
{
    public class AllToAllGeneration : SyncProtocol
    {
        public override ProtocolIds Id { get { return ProtocolIds.AllToAllRandGen; } }

        public readonly BigInteger Prime;
        private readonly SafeRandom RandGen;
        public BigZp Result { get; private set; }

        public AllToAllGeneration(SyncParty p, IList<int> pIds, BigInteger prime)
            : this(p, pIds, prime, new SafeRandom())
        {   
        }            


        public AllToAllGeneration(SyncParty p, IList<int> pIds, BigInteger prime, SafeRandom randGen)
            : base(p, pIds)
        {
            RandGen = randGen;
            Prime = prime;
        }

        public override void Run()
        {
            Result = GenerateRandom();
        }
        
        public BigZp GenerateRandom() 
        {
            int polyDegree = NumParties / 3;
            
            BigInteger myRandom;

            if (Prime > int.MaxValue)
                myRandom = StaticRandom.Next(Prime);
            else 
                myRandom = StaticRandom.Next((int) Prime);

            IList<BigZp> shares = BigShamirSharing.Share(new BigZp(Prime, myRandom), PartyIds.Count, PartyIds.Count / 3);
            IList<BigShareMsg> sendMsgs = new List<BigShareMsg>();
            foreach (BigZp share in shares)
            {
                sendMsgs.Add(new BigShareMsg(share));
            }

            IList<BigShareMsg> recvMsgs = SendReceive(PartyIds, sendMsgs);

            List<BigZp> receivedShares = new List<BigZp>();

            foreach (BigShareMsg shareMsg in recvMsgs)
            {
                receivedShares.Add(shareMsg.Value);
            }

            /*
            eVSS sharing = new eVSS(Party, PartyIds, Prime, NumParties / 3);
            sharing.Setup(RandGen.Next(int.MinValue, int.MaxValue));
            
            List<BigZp> receivedShares = sharing.ShareVerify(new BigZp(Prime, myRandom), true);
            */

            BigZp combinedShare = new BigZp(Prime);

            for (var i = 0; i < receivedShares.Count; i++) {
                combinedShare += receivedShares[i];
            }

            //BigZp random = sharing.Reconst(combinedShare);
            
            var finalShareMsgs = BroadcastReceive(new BigShareMsg(combinedShare));
            
            finalShareMsgs.Sort(new SenderComparer());
            
            var finalShares = new List<BigZp>();
            foreach (var finalShareMsg in finalShareMsgs)
            {
                finalShares.Add(finalShareMsg.Value);
            }

            BigZp random = BigShamirSharing.Recombine(finalShares, PartyIds.Count / 3, Prime, false);

            return random;
        }
    }

    public class BigShareMsg : Msg
    {
        public readonly BigZp Value;

        public BigShareMsg(BigZp value)
        {
            Value = value;
        }

        public override int StageKey
        {
            get
            {
                return 0;
            }
        }

        public override string ToString()
        {
            return base.ToString() + ", Shares=" + Value.ToString();
        }
    }
}
        
