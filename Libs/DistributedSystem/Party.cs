using MpcLib.Common.StochasticUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MpcLib.DistributedSystem
{
    public delegate void SendHandler(int fromId, int toId, Msg msg);

    /// <summary>
    /// Represents an abstract network party.
    /// </summary>
    public abstract class Party
    {
        private static int idGen;

        /// <summary>
        /// The unique identity of the party.
        /// </summary>
        public int Id { get; internal set; }

        public SafeRandom SafeRandGen { get; private set; }

        protected Dictionary<ulong, Protocol> RegisteredProtocols;
        protected Dictionary<ulong, ulong> ParentProtocols;
        protected Dictionary<ulong, int> ChildProtocolOutstandingCount;
        protected Dictionary<ulong, SortedDictionary<ulong, object>> ChildProtocolCompletedMsg;
        protected Dictionary<ulong, List<ulong>> ChildProtocolSubmissionOrder;

        public Party()
        {
            Id = idGen++;
            NetSimulator.RegisterParty(this);
            SafeRandGen = new SafeRandom();

            RegisteredProtocols = new Dictionary<ulong, Protocol>();
            ParentProtocols = new Dictionary<ulong, ulong>();
            ChildProtocolOutstandingCount = new Dictionary<ulong, int>();
            ChildProtocolCompletedMsg = new Dictionary<ulong, SortedDictionary<ulong, object>>();
            ChildProtocolSubmissionOrder = new Dictionary<ulong, List<ulong>>();
        }

        public void Send(Protocol protocol, int toId, Msg msg, int delay = 0)
        {
            NetSimulator.Send(Id, toId, protocol.ProtocolId, msg, delay);
        }

        /// <summary>
        /// Sends the i-th message to the i-th party.
        /// </summary>
        public void Send(Protocol protocol, ICollection<Msg> msgs, int delay = 0)
        {
            Debug.Assert(NetSimulator.PartyCount == msgs.Count, "Not enough recipients/messages to send!");
            NetSimulator.Send(Id, protocol.ProtocolId, msgs, delay);
        }

        public void Send(Protocol protocol, ICollection<Msg> msgs, ICollection<int> recipients, int delay = 0)
        {   
            NetSimulator.Send(Id, protocol.ProtocolId, msgs, recipients, delay);
        }

        public void Multicast(Protocol protocol, Msg msg, IEnumerable<int> toIds, int delay = 0)
        {
            NetSimulator.Multicast(Id, protocol.ProtocolId, toIds, msg, delay);
        }

        public void Broadcast(Protocol protocol, Msg msg, int delay = 0)
        {
            NetSimulator.Broadcast(Id, protocol.ProtocolId, msg, delay);
        }

        public void RegisterProtocol(Protocol parent, Protocol child)
        {
            if (Id == 1 && child.ProtocolId == 360287978779639809)
                Console.WriteLine("bye2");

            Debug.Assert(parent == null || RegisteredProtocols.ContainsKey(parent.ProtocolId));
            Debug.Assert(!RegisteredProtocols.ContainsKey(child.ProtocolId));

            RegisteredProtocols[child.ProtocolId] = child;
            ChildProtocolOutstandingCount[child.ProtocolId] = 0;

            if (parent != null)
            {
                ParentProtocols[child.ProtocolId] = parent.ProtocolId;
                ChildProtocolOutstandingCount[parent.ProtocolId]++;
                if (!ChildProtocolSubmissionOrder.ContainsKey(parent.ProtocolId))
                    ChildProtocolSubmissionOrder[parent.ProtocolId] = new List<ulong>();
                ChildProtocolSubmissionOrder[parent.ProtocolId].Add(child.ProtocolId);
            }
        }

        public void ExecuteSubProtocol(Protocol current, Protocol child)
        {
            ExecuteSubProtocols(current, new Protocol[] { child });
        }

        public void ExecuteSubProtocols(Protocol current, IEnumerable<Protocol> subProtocols)
        {
            foreach (var subProtocol in subProtocols)
            {
                RegisterProtocol(current, subProtocol);
                subProtocol.Start();
            }

            foreach (var subProtocol in subProtocols)
                CheckCompleted(subProtocol);
        }
        
        /// <summary>
        /// Initiates the party protocol.
        /// </summary>
        public abstract void Start();

        public virtual void Receive(int fromId, ulong protocolId, Msg msg)
        {
            // Console.WriteLine("Receive msg " + Id + " " + protocolId);
            Debug.Assert(RegisteredProtocols.ContainsKey(protocolId));

            Protocol protocol = RegisteredProtocols[protocolId];
            protocol.HandleMessage(fromId, msg);
            CheckCompleted(protocol);
        }

        private void CheckCompleted(Protocol protocol)
        {
            if (!protocol.IsCompleted)
                return;

            RegisteredProtocols.Remove(protocol.ProtocolId);
            protocol.Teardown();

            if (!ParentProtocols.ContainsKey(protocol.ProtocolId))
                return;

            Protocol parent = RegisteredProtocols[ParentProtocols[protocol.ProtocolId]];
            ParentProtocols.Remove(protocol.ProtocolId);

            if (!ChildProtocolCompletedMsg.ContainsKey(parent.ProtocolId))
                ChildProtocolCompletedMsg[parent.ProtocolId] = new SortedDictionary<ulong, object>();

            ChildProtocolOutstandingCount[parent.ProtocolId]--;
            ChildProtocolCompletedMsg[parent.ProtocolId][protocol.ProtocolId] = protocol.RawResult;

            if (ChildProtocolOutstandingCount[parent.ProtocolId] > 0)
                return;

            // all subprotocols are completed
            //Send(parent, Id, new SubProtocolCompletedMsg(ChildProtocolCompletedMsgs[parent.ProtocolId]));
            NetSimulator.Loopback(Id, parent.ProtocolId, new SubProtocolCompletedMsg(ChildProtocolCompletedMsg[parent.ProtocolId], ChildProtocolSubmissionOrder[parent.ProtocolId]));
            ChildProtocolCompletedMsg.Remove(parent.ProtocolId);
            ChildProtocolSubmissionOrder.Remove(parent.ProtocolId);
        }

        public static void Reset()
        {
            idGen = 0;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return "Id=" + Id;
        }
    }
}