using MpcLib.Common;
using MpcLib.DistributedSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.BasicProtocols
{
    public class MajorityFilteringProtocol<T> : Protocol<T> where T : class, ISizable, IEquatable<T>
    {
        private T MyValue;

        private IEnumerable<int> Receivers;

        private IList<int> Senders;
        private IList<T> Received;
        private bool scheduledTally;
        private int majorityThreshold;

        public MajorityFilteringProtocol(Party me, SortedSet<int> partyIds, IEnumerable<int> receivers, T value, ulong protocolId)
            : base(me, partyIds, protocolId)
        {
            MyValue = value;
            Receivers = receivers;
        }

        public MajorityFilteringProtocol(Party me, SortedSet<int> partyIds, IEnumerable<int> senders, ulong protocolId)
            : base(me, partyIds, protocolId)
        {
            Senders = new List<int>(senders);
            Received = new List<T>();
            scheduledTally = false;
            majorityThreshold = Senders.Count / 2 + 1;
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            switch (msg.Type)
            {
                case MsgType.Basic:
                    // collect the value from this party and remove it from the Senders list
                    // to make sure the party can't send duplicate values
                    if (Senders.Remove(fromId))
                    {
                        Received.Add((msg as BasicMessage<T>).Value);
                    }
                    else
                    {
                        Console.WriteLine("Unexpected majority filter receive from " + fromId);
                    }

                    if (Received.Count >= Math.Ceiling(2.0 * Senders.Count / 3.0) && !scheduledTally)
                    {
                        Send(Me.Id, new Msg(MsgType.NextRound));
                        scheduledTally = true;
            //            Console.WriteLine("Party " + Me.Id + " scheduled tally round");
                    }

                    break;
                case MsgType.NextRound:
                    if (fromId != Me.Id)
                    {
                        Console.WriteLine("Invalid next round message received. Party " + fromId + " seems to be cheating!");
                    }
                    performTally();
                    break;
                case MsgType.SubProtocolCompleted:
                    // NOP completed
                    IsCompleted = true;
                    break;
            }
        }

        public override void Start()
        {
            // if I am a sender, then multicast my value,
            // if i am a receiver do nothing.
            if (MyValue != null)
            {
                Multicast(new BasicMessage<T>(MyValue), Receivers);
            }
            if (Receivers != null && !Receivers.Contains(Me.Id))
            {
                // do a no-op protocol to keep everybody synchronized
                ExecuteSubProtocol(new NopProtocol(Me, ProtocolIdGenerator.NopIdentifier(ProtocolId), 2));
            }
        }

        private void performTally()
        {
            Dictionary<T, int> voteCount = new Dictionary<T, int>();
            foreach (var vote in Received)
            {
                if (!voteCount.ContainsKey(vote))
                    voteCount[vote] = 0;

                voteCount[vote]++;
                if (voteCount[vote] >= majorityThreshold)
                {
                    Result = vote;
                    break;
                }
            }

            if (Result == null)
            {
                Console.WriteLine("No value received a majority of the votes at " + Me.Id);
            }

            IsCompleted = true;
        }
    }
}
