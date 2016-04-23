using MpcLib.Common.StochasticUtils;
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

        public Party()
        {
            Id = idGen++;
            NetSimulator.RegisterParty(this);
            SafeRandGen = new SafeRandom();
        }

        public void Send(int toId, Msg msg, int delay = 0)
        {
            NetSimulator.Send(Id, toId, msg, delay);
        }

        /// <summary>
        /// Sends the i-th message to the i-th party.
        /// </summary>
        public void Send(ICollection<Msg> msgs, int delay = 0)
        {
            Debug.Assert(NetSimulator.PartyCount == msgs.Count, "Not enough recipients/messages to send!");
            NetSimulator.Send(Id, msgs, delay);
        }

        public void Send(ICollection<Msg> msgs, ICollection<int> recipients, int delay = 0)
        {
            NetSimulator.Send(Id, msgs, recipients, delay);
        }

        public void Multicast(Msg msg, IEnumerable<int> toIds, int delay = 0)
        {
            NetSimulator.Multicast(Id, toIds, msg, delay);
        }

        public void Broadcast(Msg msg, int delay = 0)
        {
            NetSimulator.Broadcast(Id, msg, delay);
        }

        /// <summary>
        /// Initiates the party protocol.
        /// </summary>
        public abstract void Start();

        public abstract void Receive(int fromId, Msg msg);

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