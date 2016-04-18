using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.DistributedSystem
{
    public class Quorum
    {
        public int Size
        {
            get
            {
                return Members.Count;
            }
        }

        public SortedSet<int> Members { get; private set; }
        
        public Quorum()
        {
            Members = new SortedSet<int>();
        }

        public Quorum(int startId, int endId)
        {
            Members = new SortedSet<int>();
            for (int i = startId; i < endId; i++)
            {
                Members.Add(i);
            }
        }

        public Quorum(ICollection<int> ids)
        {
            Members = new SortedSet<int>(ids);
        }

        public void AddMember(int id)
        {
            Members.Add(id);
        }

        public void RemoveMembers(int id)
        {
            Members.Remove(id);
        }

        public bool HasMember(int id)
        {
            return Members.Contains(id);
        }
    }
}
