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
        public ulong ProtocolId { get; protected set; }

        public object RawResult { get; protected set; }

        public Protocol(Party me, SortedSet<int> partyIds, ulong protocolId)
        {
            Me = me;
            PartyIds = partyIds;
            NumParties = PartyIds.Count;
            IsCompleted = false;
            ProtocolId = protocolId;
        }

        public abstract void Start();
        public abstract void HandleMessage(int fromId, Msg msg);

        public virtual void Teardown() { }
        
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
        public Protocol(Party me, SortedSet<int> partyIds, ulong protocolId)
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

        public SubProtocolCompletedMsg(SortedDictionary<ulong, object> result, IList<ulong> submissionOrder)
            : base(MsgType.SubProtocolCompleted)
        {
            Result = result;
            SubmissionOrder = submissionOrder;
        }

        public SortedDictionary<ulong, object> Result;
        private IList<ulong> SubmissionOrder;

        private List<object> ResultListInternal;
        public List<object> ResultList
        {
            get
            {
                if (ResultListInternal == null)
                    ResultListInternal = SubmissionOrder.Select(id => Result[id]).ToList();
                return ResultListInternal;
            }
        }

        public object SingleResult
        {
            get
            {
                Debug.Assert(Result.Count == 1);
                return Result[SubmissionOrder[0]];
            }
        }
    }

    public class LoopbackProtocol : Protocol
    {
        public LoopbackProtocol(Party me, object result, ulong protocolId)
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

    public class NopProtocol : Protocol
    {
        private int NopCount;

        public NopProtocol(Party me, ulong protocolId, int nopCount)
            : base(me, new SortedSet<int>(), protocolId)
        {
            NopCount = nopCount;
        }

        public override void Start()
        {
            Send(Me.Id, new Msg(MsgType.NextRound));
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            NopCount--;
            if (NopCount <= 0)
                IsCompleted = true;
            else
                Send(Me.Id, new Msg(MsgType.NextRound));
        }
    }
}
