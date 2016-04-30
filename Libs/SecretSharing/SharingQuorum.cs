using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpcLib.DistributedSystem;
using MpcLib.Commitments.PolyCommitment;

namespace MpcLib.SecretSharing
{
    public class ByzantineQuorum : Quorum
    {
        public PolyCommit PolyCommit { get; private set; }
        public int Seed { get; private set; }

        public ByzantineQuorum(int quorumId, int seed)
            : base(quorumId)
        {
            SetupByzantine(seed);
        }

        public ByzantineQuorum(int quorumId, int startId, int endId, int seed)
            : base(quorumId, startId, endId)
        {
            SetupByzantine(seed);
        }

        public ByzantineQuorum(int quorumId, ICollection<int> ids, int seed)
            : base(quorumId, ids)
        {
            SetupByzantine(seed);
        }

        private void SetupByzantine(int seed)
        {
            Seed = seed;
            InitPolyCommit();
        }

        private void InitPolyCommit()
        {
            if (Size > 0)
            {
                PolyCommit = new PolyCommit();
                PolyCommit.Setup((int)Math.Ceiling(Size / 3.0), Seed);
            }
        }

        public override void AddMember(int id)
        {
            base.AddMember(id);
            InitPolyCommit();
        }

        public override void RemoveMembers(int id)
        {
            base.RemoveMembers(id);
            InitPolyCommit();
        }

        public override object Clone()
        {
            ByzantineQuorum copy = (ByzantineQuorum)base.Clone();

            copy.PolyCommit = new PolyCommit();
            copy.InitPolyCommit();

            return copy;
        }
    }
}
