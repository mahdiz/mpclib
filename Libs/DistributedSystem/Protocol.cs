using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public long ProtocolId { get; protected set; }

        public object RawResult { get; protected set; }

        public Protocol(Party me, SortedSet<int> partyIds, long protocolId)
        {
            Me = me;
            PartyIds = partyIds;
            NumParties = PartyIds.Count;
            IsCompleted = false;
            ProtocolId = protocolId;
        }

        public abstract void Start();
        public abstract void HandleMessage(int fromId, Msg msg);
        
        public void Send(int toId, Msg msg, int delay = 0)
        {
            Me.Send(this, toId, msg, delay);
        }

        /// <summary>
        /// Sends the i-th message to the i-th party.
        /// </summary>
        public void Send(Protocol protocol, ICollection<Msg> msgs, int delay = 0)
        {
            Me.Send(this, msgs, delay);
        }

        public void Send(ICollection<Msg> msgs, ICollection<int> recipients, int delay = 0)
        {
            Me.Send(this, msgs, recipients, delay);
        }

        public void Multicast(Msg msg, IEnumerable<int> toIds, int delay = 0)
        {
            Me.Multicast(this, msg, toIds, delay);
        }

        public void Broadcast(Msg msg, int delay = 0)
        {
            Me.Broadcast(this, msg, delay);
        }

        public void ExecuteSubProtocol(Protocol subProtocol)
        {
            Me.ExecuteSubProtocol(this, subProtocol);
        }

        public void ExecuteSubProtocols(IEnumerable<Protocol> subProtocols)
        {
            Me.ExecuteSubProtocols(this, subProtocols);
        }
    }


    public abstract class Protocol<T> : Protocol where T : class
    {
        public Protocol(Party me, SortedSet<int> partyIds, long protocolId)
            : base(me, partyIds, protocolId)
        {
        }

        public T Result
        {
            get
            {
                return RawResult as T;
            }
            protected set
            {
                RawResult = value;
            }
        }
    }

    public class SubProtocolCompletedMsg : Msg
    {

        public SubProtocolCompletedMsg(SortedDictionary<long, object> result)
            : base(MsgType.SubProtocolCompleted)
        {
            Result = result;
        }

        public SortedDictionary<long, object> Result;

        private List<object> ResultListInternal;
        public List<object> ResultList
        {
            get
            {
                if (ResultListInternal == null)
                    ResultListInternal = Result.Values.ToList();
                return ResultListInternal;
            }
        }

        public object SingleResult
        {
            get
            {
                Debug.Assert(ResultList.Count == 1);
                return ResultList[0];
            }
        }
    }

    public class NopProtocol : Protocol
    {
        public NopProtocol(Party me, object result, long protocolId)
            : base(me, new SortedSet<int>(), protocolId)
        {
            IsCompleted = true;
            RawResult = result;
        }

        public override void Start()
        {
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
        }
    }
}
