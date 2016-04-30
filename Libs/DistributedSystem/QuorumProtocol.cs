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
            : this(me, quorum, quorum.NextProtocolId)
        {
        }

        public QuorumProtocol(Party me, Quorum quorum, long protocolId)
            : base(me, quorum.Members, protocolId)
        {
            Quorum = quorum;
        }

        protected void QuorumBroadcast(Msg msg, int delay = 0)
        {
            Multicast(msg, Quorum.Members, delay);
        }

        protected void QuorumSend(ICollection<Msg> msgs, int delay = 0)
        {
            Send(msgs, Quorum.Members, delay);
        }
    }

    public abstract class MultiQuorumProtocol<T> : Protocol<T> where T : class
    {
        protected Quorum[] Quorums;

        public MultiQuorumProtocol(Party me, Quorum[] quorums, long protocolId)
            : base(me, MergeQuorums(quorums), protocolId)
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
            Send(msgs, Quorums[whichQuorum].Members, delay);
        }

        protected void QuorumBroadcast(Msg msg, int whichQuorum, int delay = 0)
        {
            Multicast(msg, Quorums[whichQuorum].Members, delay);
        }
    }
}
