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

        private IList<int> Receivers;

        private IList<int> Senders;
        private IList<T> Received;
        private bool scheduledTally;
        private int majorityThreshold;
        private bool performedTally;

        public MajorityFilteringProtocol(Party me, SortedSet<int> partyIds, IList<int> receivers, T value)
            : base(me, partyIds)
        {
            MyValue = value;
            Receivers = receivers;
        }

        public MajorityFilteringProtocol(Party me, SortedSet<int> partyIds, IList<int> senders)
            : base(me, partyIds)
        {
            Senders = new List<int>(senders);
            Received = new List<T>();
            scheduledTally = false;
            performedTally = false;
            majorityThreshold = senders.Count / 2 + 1;
        }

        protected override void HandleMessage(int fromId, Msg msg)
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

                    if (Received.Count >= Math.Ceiling(2.0 * NumParties / 3.0) && !scheduledTally)
                    {
                        Me.Send(Me.Id, new Msg(MsgType.NextRound));
                        scheduledTally = true;
                        Console.WriteLine("Party " + Me.Id + " scheduled tally round");
                    }

                    break;
                case MsgType.NextRound:
                    if (fromId != Me.Id)
                    {
                        Console.WriteLine("Invalid next round message received. Party " + fromId + " seems to be cheating!");
                    }

                    performTally();
                    break;
            }
        }

        public override void Start()
        {
            // if I am a sender, then multicast my value,
            // if i am a receiver do nothing.
            if (MyValue != null)
            {
                Me.Multicast(new BasicMessage<T>(MyValue), Receivers);
            }
            if (!Receivers.Contains(Me.Id))
            {
                IsCompleted = true;
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
