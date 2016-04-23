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

        public ByzantineQuorum(int seed)
        {
            SetupByzantine(seed);
        }

        public ByzantineQuorum(int startId, int endId, int seed)
            : base(startId, endId)
        {
            SetupByzantine(seed);
        }

        public ByzantineQuorum(ICollection<int> ids, int seed)
            : base(ids)
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
    }
}
