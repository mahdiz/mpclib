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

        private ShareAdditionProtocol Sap;

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

        public override void Start()
        {
            // secret-share my input
            //    Share(Input);
            var parties = new List<int>();
            for (var i = 0; i < NumParties; i++)
            {
                parties.Add(i);
            }
            Sap = new ShareAdditionProtocol(this, parties, Input, Prime, Seed, PolyCommit);

            Sap.Start();

        }

        public override void Receive(int fromId, Msg msg)
        {
            Sap.HandleMessage(fromId, msg);

            if (Sap.IsCompleted())
            {
                Output = Sap.Result;
            }

            return;
        }
    }
}
