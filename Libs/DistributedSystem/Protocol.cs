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

        public object RawResult { get; protected set; }

        private Protocol SubProtocol;
        private int SubProtocolTag;

        public Protocol(Party me, SortedSet<int> partyIds)
        {
            Me = me;
            PartyIds = partyIds;
            NumParties = PartyIds.Count;
            IsCompleted = false;
        }

        public abstract void Start();
        protected abstract void HandleMessage(int fromId, Msg msg);

        public void MessageHandler(int fromId, Msg msg)
        {
            if (SubProtocol == null)
                HandleMessage(fromId, msg);
            else
            {
                SubProtocol.MessageHandler(fromId, msg);
                CheckSubProtocolCompleted();
            }
        }

        protected void ExecuteSubProtocol(Protocol subProtocol, int tag)
        {
            SubProtocol = subProtocol;
            SubProtocolTag = tag;
            SubProtocol.Start();

            CheckSubProtocolCompleted();
        }

        private void CheckSubProtocolCompleted()
        {
            if (SubProtocol.IsCompleted)
            {
                Me.Send(Me.Id, new SubProtocolCompletedMsg(SubProtocol.RawResult, SubProtocolTag));
                SubProtocol = null;
            }
        }
    }


    public abstract class Protocol<T> : Protocol where T : class
    {
        public Protocol(Party me, SortedSet<int> partyIds)
            : base(me, partyIds)
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

        public SubProtocolCompletedMsg(object result, int tag)
            : base(MsgType.SubProtocolCompleted)
        {
            Result = result;
            Tag = tag;
        }

        public object Result;
        public int Tag;
    }

    public class NopProtocol : Protocol
    {
        public NopProtocol(Party me, object result)
            : base(me, new SortedSet<int>())
        {
            IsCompleted = true;
            RawResult = result;
        }

        public override void Start()
        {
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
        }
    }
}
