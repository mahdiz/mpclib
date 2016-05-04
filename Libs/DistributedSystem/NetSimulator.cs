using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using MpcLib.Common.StochasticUtils;
using MpcLib.DistributedSystem;
using MpcLib.Simulation.Des;

namespace MpcLib.DistributedSystem
{
    public delegate void SentHandler(int fromId, int toId, int msgSize);

    /// <summary>
    /// Represents a network simulator.
    /// </summary>
    /// <typeparam name="T">Type of network parties.</typeparam>
    public static class NetSimulator
    {
		public static int PartyCount { get { return parties.Count; } }

		public static IList<int> PartyIds { get { return parties.Keys.ToList(); } }

		public static Dictionary<int, Party>.ValueCollection Parties { get { return parties.Values; } }

        private static Dictionary<int, Party> parties;

        public static event SentHandler MessageSent;

        /// <summary>
        /// Total number of messages sent by all parties in the network.
        /// </summary>
        public static BigInteger SentMessageCount;

        /// <summary>
        /// Total number of bytes sent by all parties in the network.
        /// </summary>
        public static BigInteger SentByteCount;

        public static long RoundCount
        {
            get
            {
                return des.MaxClock;
            }
        }

		public static int idGen = 0;

        /// <summary>
        /// Discrete-event simulator.
        /// </summary>
        private static EventSimulator<EventQueue> des;

        public static void Init(int seed)
		{
			StaticRandom.Init(seed);
            des = new EventSimulator<EventQueue>();
            parties = new Dictionary<int, Party>();
        }

		public static void RegisterParty(Party p)
		{
			parties[p.Id] = p;
		}

        public static Party GetParty(int id)
        {
            return parties[id];
        }

        public static void Run()
        {
            if (parties.Count == 0)
                throw new Exception("At least one party must be added before running the simulation.");

            foreach (var party in parties.Values)
                party.Start();

            des.Run();
        }

        public static void Send(int fromId, int toId, ulong protocolId, Msg msg, int delay = 0)
        {
          //  Console.WriteLine("Adding send from " + fromId + " at " + des.Clock);

            SentMessageCount++;
            SentByteCount += msg.Size;
            des.Schedule(toId, parties[toId].Receive, delay + 1, fromId, protocolId, msg);

            if (MessageSent != null)
                MessageSent(fromId, toId, msg.Size);
        }

        /// <summary>
        /// Sends the i-th message to the i-th party.
        /// </summary>
        public static void Send(int fromId, ulong protocolId, ICollection<Msg> msgs, int delay = 0)
        {
            Debug.Assert(parties.Count == msgs.Count, "Not enough recipients/messages to send!");


            int i = 0;
            var msgIter = msgs.GetEnumerator();
            foreach (var pair in parties)
            {
                msgIter.MoveNext();
                Send(fromId, pair.Key, protocolId, msgIter.Current, delay);
            }
        }

        public static void Send(int fromId, ulong protocolId, ICollection<Msg> msgs, ICollection<int> recipients, int delay = 0)
        {
            Debug.Assert(msgs.Count == recipients.Count, "Not enough recipients/messages to send!");

            var msgIter = msgs.GetEnumerator();
            var recipIter = recipients.GetEnumerator();

            while (msgIter.MoveNext() && recipIter.MoveNext())
            {
                Send(fromId, recipIter.Current, protocolId, msgIter.Current, delay);
            }
        }

        public static void Multicast(int fromId, ulong protocolId, IEnumerable<int> toIds, Msg msg, int delay = 0)
        {
           // Console.WriteLine("Adding multicast from " + fromId + " at " + des.Clock);
            foreach (var toId in toIds)
            {
                des.Schedule(toId, parties[toId].Receive, delay + 1, fromId, protocolId, msg);

                if (MessageSent != null)
                    MessageSent(fromId, toId, msg.Size);
            }

            // Add actual number of message sent in the reliable broadcast protocol based on CKS '2000
            // The party first sends msg to every other party and then parties run the Byzantine agreement 
            // protocol of CKS '2000 to ensure consistency.
            // CKS sends a total of 4*n^2 messages so the total number of messages sent is n + 4n^2.
            // Note: we do not add the term n here as we have already sent the message to each party (see above).
            // See OKS'10 for CKS'00 simulation results
            var nSquared = (int)Math.Pow(toIds.Count(), 2);
            SentMessageCount += 4 * nSquared;

            // For 80 bit security (i.e., RSA 1024), CKS sends 4*n^2 messages, where each message is of at most
            // two RSA signatures (i.e., 2048 bit messages). So, the total number of bits sent is 8192*n^2 (or 1024*n^2 bytes).
            SentByteCount += 1024 * nSquared;
        }

        public static void Broadcast(int fromId, ulong protocolId, Msg msg, int delay = 0)
        {
            Multicast(fromId, protocolId, PartyIds, msg, delay);
        }

        public static void Loopback(int id, ulong protocolId, Msg msg)
        {
            des.Loopback(id, parties[id].Receive, id, protocolId, msg);
        }

        public static void Reset()
        {
            des.Reset();
            Party.Reset();
            parties = new Dictionary<int, Party>();
            SentMessageCount = new BigInteger();
        }
    }
}