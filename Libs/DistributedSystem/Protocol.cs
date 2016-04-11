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
        public IList<int> PartyIds { get; internal set; }
        public int NumParties;

        public Protocol(Party me, IList<int> partyIds)
        {
            Me = me;
            PartyIds = partyIds;
            NumParties = PartyIds.Count;
        }

        public abstract void Start();
        public abstract bool CanHandleMessageType(MsgType type);
        public abstract void HandleMessage(int fromId, Msg msg);
        public abstract bool IsCompleted();
    }
}
