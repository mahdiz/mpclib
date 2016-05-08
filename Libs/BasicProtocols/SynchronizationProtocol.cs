using MpcLib.DistributedSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.BasicProtocols
{
    public class SynchronizationProtocol : Protocol
    {

        IEnumerable<Protocol> Protocols;

        SortedSet<int> PeersCompleted;

        public SynchronizationProtocol(Party me, SortedSet<int> partyIds, IEnumerable<Protocol> protocols, ulong protocolId)
            : base(me, partyIds, protocolId)
        {
            Protocols = protocols;
        }
        
        public override void HandleMessage(int fromId, Msg msg)
        {
            if (msg is SubProtocolCompletedMsg)
            {
                // my subprotocols completed. broadcast that information
                Multicast(new Msg(MsgType.NextRound), PartyIds);
                RawResult = msg;
            } else
            {
                Debug.Assert(msg.Type == MsgType.NextRound);

                PeersCompleted.Add(fromId);
                if (PeersCompleted.Count == PartyIds.Count)
                    IsCompleted = true;
            }
        }

        public override void Start()
        {
            if (Protocols.Any())
                ExecuteSubProtocols(Protocols);
            else
                Multicast(new Msg(MsgType.NextRound), PartyIds);
            PeersCompleted = new SortedSet<int>();
        }
    }
}
