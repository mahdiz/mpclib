using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.DistributedSystem
{
    public abstract class Protocol
    {
        public Party Me { get; internal set; }
        public SortedSet<int> PartyIds { get; internal set; }
        public int NumParties;
        public bool IsCompleted { get; protected set; }

        public Protocol(Party me, SortedSet<int> partyIds)
        {
            Me = me;
            PartyIds = partyIds;
            NumParties = PartyIds.Count;
            IsCompleted = false;
        }

        public abstract void Start();
        public abstract bool CanHandleMessageType(MsgType type);
        public abstract void HandleMessage(int fromId, Msg msg);
    }
}
