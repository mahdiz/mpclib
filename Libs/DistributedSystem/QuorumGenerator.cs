using System;
using System.Collections.Generic;
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
            List<SortedSet<int>> quorums = new List<SortedSet<int>>();

            int intSeed = seed.GetHashCode();
            Random randGen = new Random(intSeed);

            Dictionary<int, int> partyPositions = new Dictionary<int, int>();
            int pos = 0;
            foreach (int party in parties)
                partyPositions[pos++] = party;

            for (int i = 0; i < quorumCount; i++)
            {
                quorums.Add(GenerateQuorum(randGen, countPerQuorum, parties.Count, partyPositions));
            }

            return quorums;
        }


        private static SortedSet<int> GenerateQuorum(Random randGen, int size, int numParties, Dictionary<int, int> partyPositions)
        {
            // we want deterministic quroum generation using C#'s built in random class
            SortedSet<int> quorum = new SortedSet<int>();

            for (int i = 0; i < numParties; i++)
            {
                int next = partyPositions[randGen.Next() % numParties];
                if (quorum.Contains(next))
                    i--;
                else
                    quorum.Add(next);
            }

            return quorum;
        }
    }
}
