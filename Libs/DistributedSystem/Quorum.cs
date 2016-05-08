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

        private Dictionary<Quorum, ushort> TwoQuorumProtocolIds;

        private SortedSet<uint> UsedIds = new SortedSet<uint>();
        public readonly ushort QuorumNumber;
        private uint WhichProtocol = 0;
        
        public SortedSet<int> Members { get; private set; }
        
        public Quorum(int quorumNumber)
        {
            Debug.Assert(quorumNumber <= ushort.MaxValue);
            Members = new SortedSet<int>();
            QuorumNumber = (ushort) quorumNumber;
            TwoQuorumProtocolIds = new Dictionary<Quorum, ushort>();
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

        public void ReleaseId(ulong id)
        {
            UsedIds.Remove(ProtocolIdGenerator.GetIntraQuorumProtocolNumber(id));
        }

        public ulong GetNextProtocolId()
        {
            Debug.Assert(UsedIds.LongCount() < uint.MaxValue);
            while (UsedIds.Contains(WhichProtocol))
                WhichProtocol++;

            return ProtocolIdGenerator.QuorumProtocolIdentifier(QuorumNumber, WhichProtocol++);
        }

        public ulong GetNextTwoProtocolId(Quorum other, bool incrementOther = true)
        {
            if (this == other)
                return GetNextProtocolId();

            if (!TwoQuorumProtocolIds.ContainsKey(other))
                TwoQuorumProtocolIds[other] = 0;

            // if we are failing this assert, we need to expand the range
            Debug.Assert(TwoQuorumProtocolIds[other] <= ushort.MaxValue);

            ulong retId = ProtocolIdGenerator.TwoQuorumProtocolIdentifier(QuorumNumber, other.QuorumNumber, TwoQuorumProtocolIds[other]);
            TwoQuorumProtocolIds[other]++;

            // increment the id in the other quorum (necessary because a party may be part of both quorums)
            if (incrementOther)
               other.GetNextTwoProtocolId(this, false);

            return retId;
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
