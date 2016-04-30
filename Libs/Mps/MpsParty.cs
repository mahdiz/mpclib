using MpcLib.Commitments.PolyCommitment;
using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem;
using MpcLib.SecretSharing;
using MpcLib.SecretSharing.eVSS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MpcLib.SecretSharing.QuorumShareRenewal;

namespace MpcLib.MultiPartyShuffling
{
    public class MpsParty : Party
    {
        public readonly BigInteger Prime;
        public readonly int NumParties;
        public readonly int PolyDegree;
        public readonly BigZp Input;
        public BigZp Output { get; private set; }
        protected PolyCommit PolyCommit;
        private int Seed;

        private ShareRenewalRound Srr;
        private RandomGenProtocol Sap;
        private QuorumShareRenewalProtocol Qrs;
        
        public MpsParty(int numParties, BigZp input, BigInteger prime, int seed)
        {
            Prime = prime;
            NumParties = numParties;
            Input = input;
            Seed = seed;
            PolyDegree = (int)Math.Ceiling(NumParties / 3.0);
            PolyCommit = new PolyCommit();
            PolyCommit.Setup(PolyDegree, seed);
        }
        /*
        public override void Start()
        {
            var parties = new SortedSet<int>();
            for (int i = 0; i < NumParties; i++)
            {
                parties.Add(i);
            }

            Sap = new ShareAdditionProtocol(this, parties, new BigZp(Prime, 5), Prime, Seed, PolyCommit);
            Sap.Start();
        }
        */

        public override void Start()
        {
        }

        public override void Receive(int fromId, long protocolId, Msg msg)
        {
        }
    }
}
