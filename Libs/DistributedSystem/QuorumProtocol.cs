using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.DistributedSystem
{
    public abstract class QuorumProtocol<T> : Protocol<T> where T : class
    {
        protected Quorum Quorum;

        public QuorumProtocol(Party me, Quorum quorum)
            : base(me, quorum.Members)
        {
            Quorum = quorum;
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        protected void QuorumBroadcast(Msg msg, int delay = 0)
        {
            Me.Multicast(msg, Quorum.Members, delay);
        }

        protected void QuorumSend(ICollection<Msg> msgs, int delay = 0)
        {
            Me.Send(msgs, Quorum.Members, delay);
        }
    }

    public abstract class MultiQuorumProtocol<T> : Protocol<T> where T : class
    {
        protected Quorum[] Quorums;

        public MultiQuorumProtocol(Party me, Quorum[] quorums)
            : base(me, MergeQuorums(quorums))
        {
            Quorums = quorums;
        }


        private static SortedSet<int> MergeQuorums(Quorum[] quorums)
        {
            var combined = new SortedSet<int>();
            foreach (var quorum in quorums)
            {
                combined.Concat(quorum.Members);
            }
            return combined;
        }

        protected void QuorumSend(ICollection<Msg> msgs, int whichQuorum, int delay = 0)
        {
            Me.Send(msgs, Quorums[whichQuorum].Members, delay);
        }

        protected void QuorumBroadcast(Msg msg, int whichQuorum, int delay = 0)
        {
            Me.Multicast(msg, Quorums[whichQuorum].Members, delay);
        }
    }
}
