using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.Circuits
{
    public class SortingNetwork
    {
        public static bool isPowerOfTwo(ulong x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }
    }

    public static class SortingNetworkFactory
    {
        public static PermutationNetwork CreateButterflyTournament(int wireCount)
        {
            Debug.Assert(SortingNetwork.isPowerOfTwo((uint)wireCount));

            PermutationNetwork pn = new PermutationNetwork(wireCount);

            if (wireCount == 1)
                return pn;

            pn.AppendNetwork(CreateButterflyTournamentRound(wireCount), 0);

            // recursively construct the butterfly
            if (wireCount > 2)
            {
                pn.AppendNetwork(CreateButterflyTournament(wireCount / 2), 0);
                pn.AppendNetwork(CreateButterflyTournament(wireCount / 2), wireCount / 2);
            }

            return pn;
        }

        public static PermutationNetwork CreateButterflyTournamentRound(int wireCount)
        {
            Debug.Assert(SortingNetwork.isPowerOfTwo((uint)wireCount));

            PermutationNetwork pn = new PermutationNetwork(wireCount);

            // we want to join every wire to its corresponding wire on the other half using a compare and swap gate
            for (int i = 0; i < wireCount / 2; i++)
            {
                pn.AppendGate(new CompareAndSwapGate(), new int[] { i, i + wireCount / 2 });
            }

            return pn;
        }

        public static PermutationNetwork CreateBitonicSplit(int wireCount, bool invertOrder)
        {
            Debug.Assert(SortingNetwork.isPowerOfTwo((uint)wireCount));

            PermutationNetwork pn = new PermutationNetwork(wireCount);

            for (int i = 0; i < wireCount / 2; i++)
            {
                pn.AppendGate(new CompareAndSwapGate(), new int[] { i, i + wireCount / 2 });
                if (invertOrder)
                    pn.AppendGate(PermutationGateFactory.CreateSwapGate(), new int[] { i, i + wireCount / 2 });
            }

            return pn;
        }

        public static PermutationNetwork CreateBitonicMerge(int wireCount, bool invertOrder)
        {
            Debug.Assert(SortingNetwork.isPowerOfTwo((uint)wireCount));

            PermutationNetwork pn = new PermutationNetwork(wireCount);

            pn.AppendNetwork(CreateBitonicSplit(wireCount, invertOrder), 0);

            if (wireCount > 2)
            {
                pn.AppendNetwork(CreateBitonicMerge(wireCount / 2, invertOrder), 0);
                pn.AppendNetwork(CreateBitonicMerge(wireCount / 2, invertOrder), wireCount / 2);
            }

            return pn;
        }

        public static PermutationNetwork CreateBitonicSort(int wireCount, bool invertOrder)
        {
            Debug.Assert(SortingNetwork.isPowerOfTwo((uint)wireCount));

            PermutationNetwork pn = new PermutationNetwork(wireCount);

            if (wireCount >= 4)
            {
                pn.AppendNetwork(CreateBitonicSort(wireCount / 2, false), 0);
                pn.AppendNetwork(CreateBitonicSort(wireCount / 2, true), wireCount / 2);
            }

            pn.AppendNetwork(CreateBitonicMerge(wireCount, invertOrder), 0);

            return pn;
        }

        public static PermutationNetwork CreateBinaryTreeInsertion(int wireCount)
        {
            // assume that the first input wire is the location of the unsorted element

            Debug.Assert(SortingNetwork.isPowerOfTwo((uint)wireCount));

            PermutationNetwork pn = new PermutationNetwork(wireCount);

            if (wireCount == 1)
            {
                return pn;
            }

            pn.AppendGate(new CompareAndSwapGate(), new int[] { 0, wireCount / 2 });

            if (wireCount > 2)
            {
                pn.AppendNetwork(CreateBinaryTreeInsertion(wireCount / 2), 0);
                pn.AppendNetwork(CreateBinaryTreeInsertion(wireCount / 2), wireCount / 2);
            }

            return pn;
        }
        
    }
    
}
