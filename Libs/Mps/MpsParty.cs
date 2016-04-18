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
using MpcLib.BasicProtocols;
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
        private ShareAdditionProtocol Sap;
        private QuorumShareRenewal Qrs;

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
            // secret-share my input
            // Share(Input);
            Quorum from = new Quorum(0, 5);
            Quorum to = new Quorum(5, 25);

            //    BigZp[] inShares = (Input != null) ? new BigZp[] { Input } : null;

            //   Srr = new ShareRenewalRound(this, from, to, Input, Prime, 2, 2, 5);

            Qrs = new QuorumShareRenewal(this, from, to, Input, Prime, 5);

            Qrs.Start();

        }

        public override void Receive(int fromId, Msg msg)
        {
            
            Qrs.HandleMessage(fromId, msg);
            if (Qrs.IsCompleted)
            {
                Output = Qrs.ResultShare;
            }
            
            /*
            Sap.HandleMessage(fromId, msg);
            if (Sap.IsCompleted)
            {
                Output = Sap.Result;
            }
            return;
            */
        }
    }
}
