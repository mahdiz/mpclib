using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.DistributedSystem
{
    public class QuorumGenerator
    {
        public static List<SortedSet<int>> GenerateQuorums(SortedSet<int> parties, int quorumCount, int countPerQuorum, BigInteger seed)
        {
            // ensure that every party will be in a quorum
            Debug.Assert(quorumCount * countPerQuorum >= parties.Count);

            List<SortedSet<int>> quorums = new List<SortedSet<int>>();

            int intSeed = seed.GetHashCode();
            //   Random randGen = new Random(intSeed);
            Random randGen = new Random(3);
            Dictionary<int, int> partyPositions = new Dictionary<int, int>();
            int pos = 0;
            foreach (int party in parties)
                partyPositions[pos++] = party;

            do
            {
                quorums.Clear();
                for (int i = 0; i < quorumCount; i++)
                {
                    quorums.Add(GenerateQuorum(randGen, countPerQuorum, parties.Count, partyPositions));
                }
            } while (!AreAllInQuorum(quorums, parties.Count));

            return quorums;
        }


        private static SortedSet<int> GenerateQuorum(Random randGen, int size, int numParties, Dictionary<int, int> partyPositions)
        {
            // we want deterministic quroum generation using C#'s built in random class
            SortedSet<int> quorum = new SortedSet<int>();

            for (int i = 0; i < size; i++)
            {
                int next = partyPositions[randGen.Next() % numParties];
                if (quorum.Contains(next))
                    i--;
                else
                    quorum.Add(next);
            }

            return quorum;
        }

        private static bool AreAllInQuorum(List<SortedSet<int>> quorums, int partyCount)
        {
            SortedSet<int> parties = new SortedSet<int>();
            foreach (var q in quorums)
                foreach (var p in q)
                    parties.Add(p);

            return parties.Count == partyCount;
        }
    }
}
