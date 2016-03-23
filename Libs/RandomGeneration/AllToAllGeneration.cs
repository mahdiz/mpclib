using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using MpcLib.Commitments.PolyCommitment;
using MpcLib.Common;
using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem;

namespace MpcLib.RandomGeneration.AllToAllGeneration
{
    public class AllToAllGeneration : SyncProtocol
    {
        public readonly BigInteger Prime;
        private readonly SafeRandom RandGen;

        public AllToAllGeneration(SyncParty p, ILis<int> pIds, BigInteger prime)
            : this(p, pIds, prime, null)
        {   
        }            


        public AllToAllGeneration(SyncParty p, ILis<int> pIds, BigInteger prime, SafeRandom randGen)
            : base(p, pIds)
        {
            if (randomGen != null) 
                RandGen = randGen;
            else 
                RandGen = new SafeRandom();

            Prime = prime;
        }
        
        public BigZp GenerateRandom() 
        {
            int polyDegree = NumParties / 3;
            
            BigInteger myRandom;

            if (Prime > int.MaxValue)
                myRandom = StaticRandom.Next(Prime);
            else 
                myRandom = new BigInteger(StaticRandom.Next((int) Prime));

            
            eVSS sharing = new eVSS(Party, PartyIds, Prime, NumParties / 3);

            List<BigZp> receivedShares = sharing.ShareVerify(myRandom, true);

            BigZp combinedShare = new BigZp();

            for (var i = 0; i < receivedShares.Count; i++) {
                combinedShare += receivedShares[i];
            }

            BigZp random = sharing.Reconst(combinedShare);

            return random;
        }
    }
}
        
