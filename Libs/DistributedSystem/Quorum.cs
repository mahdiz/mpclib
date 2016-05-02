using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private short QuorumNumber;
        private int WhichProtocol = 0;
        public ulong NextProtocolId
        {
            get
            {
                return ProtocolIdGenerator.QuorumProtocolIdentifier(QuorumNumber, WhichProtocol++);
            }
        }

        public SortedSet<int> Members { get; private set; }
        
        public Quorum(int quorumNumber)
        {
            Debug.Assert(quorumNumber <= short.MaxValue);
            Members = new SortedSet<int>();
            QuorumNumber = (short) quorumNumber;
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

        public int GetPositionOf(int id)
        {
            return GetPositionOf(Members, id);
        }
        
        public static int GetPositionOf(SortedSet<int> ids, int id)
        {
            int i = 0;
            foreach (var qid in ids)
            {
                if (qid == id)
                    return i;
                i++;
            }
            return -1;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Quorum))
                return false;
            Quorum other = (Quorum)obj;

            // should be sufficient
            return other.QuorumNumber == QuorumNumber;
        }

        public override int GetHashCode()
        {
            return QuorumNumber.GetHashCode();
        }
    }
}
