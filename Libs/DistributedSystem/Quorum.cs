using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.DistributedSystem
{
    public class Quorum : ICloneable
    {
        public int Size
        {
            get
            {
                return Members.Count;
            }
        }

        private int QuorumNumber;
        private long ProtocolId = 0;
        public long NextProtocolId
        {
            get
            {
                return ProtocolId++;
            }
        }

        public SortedSet<int> Members { get; private set; }
        
        public Quorum(int quorumNumber)
        {
            Members = new SortedSet<int>();
            QuorumNumber = quorumNumber;
            SetStartProtocolId();
        }

        public Quorum(int quorumNumber, int startId, int endId)
            : this(quorumNumber)
        {
            for (int i = startId; i < endId; i++)
            {
                Members.Add(i);
            }
        }

        public Quorum(int quorumNumber, ICollection<int> ids)
            : this(quorumNumber)
        {
            Members = new SortedSet<int>(ids);
        }
        
        public virtual object Clone()
        {
            Quorum q = (Quorum) this.MemberwiseClone();

            q.Members = new SortedSet<int>(Members);
            return q;
        }

        private void SetStartProtocolId()
        {
            ProtocolId = QuorumNumber;
            ProtocolId <<= 32;
            ProtocolId += 1;
        }

        public virtual void AddMember(int id)
        {
            Members.Add(id);
        }

        public virtual void RemoveMembers(int id)
        {
            Members.Remove(id);
        }

        public bool HasMember(int id)
        {
            return Members.Contains(id);
        }
    }
}
