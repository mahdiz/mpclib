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
        private MpsProtocol Protocol;
        public List<BigZp> Results
        {
            get
            {
                if (Protocol.IsCompleted)
                    return Protocol.Result;
                else
                    return null;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return Protocol.IsCompleted;
            }
        }
        
        public MpsParty(int numParties, BigZp input)
        {
            SortedSet<int> parties = new SortedSet<int>();
            for (int i = 0; i < numParties; i++)
                parties.Add(i);

            Protocol = new MpsProtocol(this, parties, ProtocolIdGenerator.GenericIdentifier(0), input, input.Prime);
        }

        public override void Start()
        {
            RegisterProtocol(null, Protocol);
            Protocol.Start();
        }
    }
}
