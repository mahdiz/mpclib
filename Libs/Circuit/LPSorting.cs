using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Based on the paper "A (fairly) Simple Circuit that (usually) Sorts" by Tom Leighton and Greg Plaxton

namespace MpcLib.Circuits
{
    public class LPSortingNetwork : PermutationNetwork
    {
        private const int c = 0; // not sure what to set as.  Log of the constant from the big-O notation
        private const double gamma = 0.822;
        private const int bitonicSortBound = 6;

        int K;

        LPSortingCalculationCache CalculationCache;

        public LPSortingNetwork(int wireCount)
            : base(wireCount)
        {
            Debug.Assert(SortingNetwork.isPowerOfTwo((uint)wireCount));

            K = (int)Math.Log(wireCount, 2);

            CalculationCache = new LPSortingCalculationCache();

            AppendNetwork(new LPSortingNetwork(K, CalculationCache), 0);
        }

        // the initial round
        private LPSortingNetwork(int k, LPSortingCalculationCache calculationCache)
            : base(1 << k)
        {
            CalculationCache = calculationCache;
            K = k;

            if (k <= (int)Math.Floor(gamma * (k)) + c + 2)
            {
                // network too small to be sorted by this method
                AppendNetwork(SortingNetworkFactory.CreateBitonicSort(1 << K, false), 0);
            }
            else
            {
                // Apply Lemma 4.2
                AppendNetwork(CreateTournament(k), 0);
                // Apply Lemma 4.1
                AppendNetwork(CreateBlockCorrectionNetwork(k, (int)Math.Floor(gamma * (k)) + c), 0);

                AppendNetwork(new LPSortingNetwork(K, (int)Math.Floor(gamma * (k)) + c + 2, CalculationCache), 0);
            }
        }

        // the subsequent rounds
        private LPSortingNetwork(int k, int l, LPSortingCalculationCache calculationCache)
            : base(1 << k)
        {
            Console.WriteLine(k + " " + l);
            CalculationCache = calculationCache;

            K = k;
            
            if (l <= (int)Math.Floor(gamma * (l + 2)) + c + 5)
            {
                // if we would get worse by doing the procedure, then finish
                if (k <= l)
                    AppendNetwork(SortingNetworkFactory.CreateBitonicSort(1 << K, false), 0);
                else
                    // Apply Lemma 4.3
                    AppendNetwork(CreateFinalSorter(l), 0);
                
            }
            else
            {
                // Apply Lemma 4.2
                PermutationNetwork tournament = CreateTournament(l + 2);
                // Apply Lemma 4.1
                PermutationNetwork tournamentCorrecter = CreateBlockCorrectionNetwork(l + 2, (int)Math.Floor(gamma * (l + 2)) + c);

                for (int i = 0; i < 1 << (k - (l+2)); i++)
                {
                    AppendNetwork(tournament.Clone() as PermutationNetwork, i * (1 << (l+2)));
                    AppendNetwork(tournamentCorrecter.Clone() as PermutationNetwork, i * (1 << (l + 2)));
                }

                // Apply Lemma 4.4
                PermutationNetwork neighborCorrecter = CreateBlockNeighborSorter(l + 2, l + 1, (int)Math.Floor(gamma * (l + 2)) + c + 2);
                AppendNetwork(neighborCorrecter, 0);

                AppendNetwork(new LPSortingNetwork(k, (int) Math.Floor(gamma * (l + 2)) + c + 5, CalculationCache), 0);
            }
        }

        // Lemma 4.2
        private PermutationNetwork CreateTournament(int sortInaccuracy)
        {
            PermutationNetwork pn = new PermutationNetwork(1 << K);

            int blockSize = 1 << sortInaccuracy;

            // for the permutation rho
            PermutationGate permuteGate = new PermutationGate(GenerateArbitraryPermutation(blockSize));
            
            pn.AppendGate(permuteGate.Copy() as Gate, 0);
            pn.AppendNetwork(SortingNetworkFactory.CreateButterflyTournament(blockSize), 0);
            return pn;
        }
        
        private int[] GenerateArbitraryPermutation(int size)
        {
            // we want deterministic random numbers
            Random rand = new Random(0);

            // use Fisher-Yates shuffle
            int[] ret = new int[size];

            for (int i = 0; i < size; i++)
            {
                ret[i] = i;
            }

            for (int i = size - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                int temp = ret[j];
                ret[j] = ret[i];
                ret[i] = temp;
            }

            return ret;
        }

        // Lemma 4.1
        private PermutationNetwork CreateBlockCorrectionNetwork(int blockBitLength, int unsortedness)
        {
            PermutationNetwork pn = new PermutationNetwork(1 << blockBitLength);

            SortedSet<int> ySet = new SortedSet<int>(CalculationCache.generateY(blockBitLength));
            Debug.Assert(ySet.Count <= 1 << unsortedness);
            // augment Y with arbitrary elements
            int ySize = 1 << (unsortedness + 1);
            int blockSize = 1 << blockBitLength;

            Random rand = new Random(0);
            while (ySet.Count < ySize)
            {
                ySet.Add(rand.Next(blockSize));
            }

            // here our implementation differs from the paper.  The paper first to extract Y then order the X by the permutation pi.
            // Instead, we will order all of the inputs by the permutation pi, then map Y using the permutation pi and move X to the top of the block
            // and Y to the bottom so that we can unshuffle X and add Y.

            int[] pi = CalculationCache.generatePi(blockBitLength);

            pn.AppendGate(new PermutationGate(pi), 0);

            int[] mappedY = new int[ySize];
            int i = 0;
            foreach (var yElem in ySet)
            {
                mappedY[i++] = pi[yElem];
            }

            pn.AppendGate(PermutationGateFactory.CreateSplitGate(blockSize, mappedY, false), 0);

            // we now want to unshuffle X into 2^(l+1) groups and add one element of Y to each group

            pn.AppendGate(PermutationGateFactory.CreateUnshuffleGate(blockSize - ySize, ySize), 0);

            pn.AppendGate(PermutationGateFactory.CreateMultiGroupInserterGate(blockSize, (blockSize / ySize) - 1, ySize), 0);

            var treeInsertion = SortingNetworkFactory.CreateBinaryTreeInsertion(blockSize / ySize);

            // use binary tree insertion to insert the elemnt we just added to each group
            for (int j = 0; j < ySize; j++)
            {
                pn.AppendNetwork(treeInsertion.Clone() as PermutationNetwork, j * blockSize / ySize);
            }

            // now shuffle all of the lists back together
            pn.AppendGate(PermutationGateFactory.CreateShuffleGate(blockSize, ySize), 0);

            return pn;
        }

        // Lemma 4.4
        private PermutationNetwork CreateBlockNeighborSorter(int blockBitLength, int borderBitLength, int intraBlockSortQuality)
        {
            PermutationNetwork pn = new PermutationNetwork(1 << K);

            int blockSize = 1 << blockBitLength;
            int blockCount = 1 << (K - blockBitLength);
            int borderSize = 1 << borderBitLength;

            PermutationNetwork borderSorter = CreateBorderSorter(borderBitLength, intraBlockSortQuality);

            for (int i = 0; i < blockCount - 1; i++)
            {
                pn.AppendNetwork(borderSorter.Clone() as PermutationNetwork, i * blockSize + (blockSize - borderSize));
            }

            return pn;
        }

        private PermutationNetwork CreateBorderSorter(int borderBitLength, int intraBlockSortQuality)
        {
            PermutationNetwork pn = new PermutationNetwork(1 << (borderBitLength + 1));

            int borderSize = 1 << borderBitLength;
            int listCount = (1 << (intraBlockSortQuality + 1));
            int listSize = borderSize / listCount;

            pn.AppendGate(PermutationGateFactory.CreateUnshuffleGate(borderSize, listCount), 0);
            pn.AppendGate(PermutationGateFactory.CreateUnshuffleGate(borderSize, listCount), borderSize);

            // merge the corresponding lists
            for (int i = 0; i < listCount; i++)
            {
                int[] wires = new int[listSize * 2];
                for (int j = 0; i < listSize; i++)
                {
                    wires[j] = i * listSize + j;
                    wires[j + listSize] = wires[j] + borderSize;
                }

                pn.AppendNetwork(SortingNetworkFactory.CreateBitonicMerge(listSize * 2, false), wires);
            }

            // shuffle the lists back into the blocks
            pn.AppendGate(PermutationGateFactory.CreateShuffleGate(borderSize, listCount), 0);
            pn.AppendGate(PermutationGateFactory.CreateShuffleGate(borderSize, listCount), borderSize);

            return pn;
        }

        // Lemma 4.3
        private PermutationNetwork CreateFinalSorter(int blockBitLength)
        {
            PermutationNetwork pn = new PermutationNetwork(1 << K);

            int blockSize = 1 << blockBitLength;
            int blockCount = 1 << (K - blockBitLength);

            // sort each block, alternate orders for bitonic merge coming up
            var bitonicSort = SortingNetworkFactory.CreateBitonicSort(blockSize, false);

            for (int i = 0; i < blockCount; i++)
            {
                pn.AppendNetwork(bitonicSort, i * blockSize);
                if (i % 2 == 1)
                    pn.AppendGate(PermutationGateFactory.CreateInvertGate(blockSize), i * blockSize);
            }

            PermutationNetwork twoBlockMerge = SortingNetworkFactory.CreateBitonicMerge(blockSize * 2, false);

            // merge each block with the one next to it.  alternate ordern for bitonic merge coming up

            var bitonicMerge = SortingNetworkFactory.CreateBitonicMerge(blockSize * 2, false);
            for (int i = 0; i < blockCount; i+=2)
            {
                pn.AppendNetwork(bitonicMerge.Clone() as PermutationNetwork, i * blockSize);
                if ((i / 2) % 2 == 1)
                    pn.AppendGate(PermutationGateFactory.CreateInvertGate(2 * blockSize), i * blockSize);
            }

            // do another round of merges, this time offset by 1
            for (int i = 1; i < blockCount; i+=2)
            {
                pn.AppendNetwork(bitonicMerge.Clone() as PermutationNetwork, i * blockSize);
            }

            // the last block will be inverted, so uninvert it
            pn.AppendGate(PermutationGateFactory.CreateInvertGate(blockSize), (blockCount - 1) * blockSize);

            return pn;
        }
    }
    
    class LPSortingCalculationCache
    {
        public const int MAX_SIZE = 32;
        private const int SMALL = 0;
        private const int MIDDLE = 1;
        private const int BIG = 2;

        // paper only says delta is a "sufficiently small positive constant." God knows what that means.
        private static readonly double delta = .001; 
        
        private double[,][] BetaCache = new double[MAX_SIZE,3][];

        private int[][] PermutationCache = new int[MAX_SIZE][];
        private int[][] BadSortCache = new int[MAX_SIZE][];


        private void PopulateBetaCacheOuter(int length, int whichEval)
        {
            if (BetaCache[length, whichEval] != null)
                return;

            BetaCache[length, whichEval] = new double[1 << length];

            double start = 0;
            switch (whichEval)
            {
                case SMALL: start = GetSmallBetaEval(length); break;
                case MIDDLE: start = .5; break;
                case BIG: start = 1 - GetLargeBetaEval(length); break;
                default:
                    Debug.Assert(false);
                    break;
            }

            PopulateBetaCache(0, 0, start, 0, length, whichEval);
            PopulateBetaCache(0, 1, start, 0, length, whichEval);
        }

        private void PopulateBetaCache(int lastBitstring, int nextBit, double lastEvalValue, int lastLength, int maxLength, int whichEval)
        {
            int nextBitstring = (nextBit << lastLength) | lastBitstring;
            double nextValue;

            if (nextBit == 0)
                nextValue = 1 - Math.Sqrt(1 - lastEvalValue);
            else
                nextValue = Math.Sqrt(lastEvalValue);

            int currentLength = lastLength + 1;

            if (currentLength == maxLength)
            {
                BetaCache[maxLength, whichEval][nextBitstring] = nextValue;
            }
            else
            {
                PopulateBetaCache(nextBitstring, 0, nextValue, currentLength, maxLength, whichEval);
                PopulateBetaCache(nextBitstring, 1, nextValue, currentLength, maxLength, whichEval);
            }
        }

        private static double GetSmallBetaEval(int length)
        {
            return Math.Pow(2, -Math.Pow(1 << length, delta));
        }

        private static double GetLargeBetaEval(int length)
        {
            return 1 - Math.Pow(2, -Math.Pow(1 << length, delta));
        }

        private static double HFunction(double smallBeta, double bigBeta, double deltaEvalPoints)
        {
            return DeltaFunction(smallBeta, bigBeta) / deltaEvalPoints;
        }

        private static double DeltaFunction(double a, double b)
        {
            // what base should I use for the log?
            return Math.Log(b * (1 - a) / ((1 - b) * a));
        }
        
        public int[] generatePi(int length)
        {
            Debug.Assert(length < MAX_SIZE);
            if (PermutationCache[length] != null)
                return PermutationCache[length];

            PopulateBetaCacheOuter(length, MIDDLE);

            int powLength = 1 << length;

            List<Tuple<int, int>> sortList = new List<Tuple<int, int>>();
            for (int i = 0; i < powLength; i++)
            {
                int sortVal = (int)Math.Floor(powLength * BetaCache[length, MIDDLE][i]);
                sortList.Add(new Tuple<int, int>(sortVal, i));
            }

            // sort in ascending order
            sortList.Sort((x, y) =>
            {
                int result = y.Item1.CompareTo(x.Item1);
                return (result == 0) ? y.Item2.CompareTo(x.Item2) : result;
            });

            int[] permutation = new int[powLength];

            for (int i = 0; i < powLength; i++)
            {
                permutation[sortList[i].Item2] = i;
            }
            PermutationCache[length] = permutation;

            return permutation;
        }

        public int[] generateY(int length)
        {
            Debug.Assert(length < MAX_SIZE);
            if (BadSortCache[length] != null)
                return BadSortCache[length];

            List<int> badSortList = new List<int>();

            // we need to figure out when the H function for the bit string is greater than n^(-.178)
            PopulateBetaCacheOuter(length, SMALL);
            PopulateBetaCacheOuter(length, BIG);

            int powLength = 1 << length;

            double threshold = Math.Pow(powLength, -.178);
            double deltaEvalPoints = DeltaFunction(GetSmallBetaEval(length), GetLargeBetaEval(length));
            for (int i = 0; i < powLength; i++)
            {
                double h = HFunction(BetaCache[length, SMALL][i], BetaCache[length, BIG][i], deltaEvalPoints);
                if (h > threshold)
                {
                    badSortList.Add(i);
                }
            }

            BadSortCache[length] = badSortList.ToArray();

            return BadSortCache[length];
        }
    }
}
