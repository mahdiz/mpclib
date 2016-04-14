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
        public static List<SortedSet<int>> GenerateQuorums(int numParties, int quorumCount, int countPerQuorum, BigInteger seed)
        {
            List<SortedSet<int>> quorums = new List<SortedSet<int>>();

            int intSeed = seed.GetHashCode();
            Random randGen = new Random(intSeed);

            for (int i = 0; i < quorumCount; i++)
            {
                quorums.Add(GenerateQuorum(randGen, countPerQuorum, numParties));
            }

            return quorums;
        }


        private static SortedSet<int> GenerateQuorum(Random randGen, int size, int numParties)
        {
            // we want deterministic quroum generation using C#'s built in random class
            SortedSet<int> quorum = new SortedSet<int>();

            for (int i = 0; i < numParties; i++)
            {
                int next = randGen.Next() % numParties;
                if (quorum.Contains(next))
                    i--;
                else
                    quorum.Add(next);
            }

            return quorum;
        }
    }
}
