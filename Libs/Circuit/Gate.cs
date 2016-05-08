using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.Circuits
{
    public abstract class Gate
    {
        public const int NO_RANK = -1;

        public int InputCount { get; protected set; }
        public int OutputCount { get; protected set; }

        public int TopologicalRank = NO_RANK;

        public Gate(int inputCount, int outputCount)
        {
            InputCount = inputCount;
            OutputCount = outputCount;
        }

        public InputGateAddress GetLocalInputAddress(int port)
        {
            return new InputGateAddress(this, port);
        }

        public OutputGateAddress GetLocalOutputAddress(int port)
        {
            return new OutputGateAddress(this, port);
        }

        public abstract Gate Copy();

        public override string ToString()
        {
            return "Gate Number: " + TopologicalRank;
        }
    }

    public abstract class GateAddress
    {
        public readonly Gate Gate;
        public readonly int Port;

        public GateAddress(Gate gate, int port)
        {
            Gate = gate;
            Port = port;
        }

        public override string ToString()
        {
            return "(" + Gate.TopologicalRank + ", " + Port + ")";
        }

        public override bool Equals(object obj)
        {
            GateAddress other = obj as GateAddress;
            if (obj == null)
                return false;
            return Gate.Equals(other.Gate) && Port == other.Port;
        }

        public override int GetHashCode()
        {
            return 53 * Gate.GetHashCode() + 31 * Port;
        }
    }

    public class InputGateAddress : GateAddress
    {
        public InputGateAddress(Gate gate, int port)
            : base(gate, port)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is InputGateAddress && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 19 * base.GetHashCode() + 13;
        }
    }

    public class OutputGateAddress : GateAddress
    {
        public OutputGateAddress(Gate gate, int port)
            : base(gate, port)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is OutputGateAddress && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 11 * base.GetHashCode() + 29;
        }
    }

    public class GateConnection
    {
        public readonly OutputGateAddress FromAddr;
        public readonly InputGateAddress ToAddr;

        public GateConnection(OutputGateAddress from, InputGateAddress to)
        {
            FromAddr = from;
            ToAddr = to;
        }

        public override bool Equals(object obj)
        {
            GateConnection other = obj as GateConnection;
            if (obj == null)
                return false;
            return FromAddr.Equals(other.FromAddr) && ToAddr.Equals(other.ToAddr);
        }

        public override int GetHashCode()
        {
            return 5 * FromAddr.GetHashCode() + 37 * ToAddr.GetHashCode();
        }
    }

    public static class GateAddressHelper
    {
        static internal string Render(this GateAddress addr)
        {
            return (addr == null) ? "null" : addr.ToString();
        }
    }

    public enum ComputationGateType
    {
        COMPARE_AND_SWAP,
    }

    public static class ComputationGateTypeHelper
    {
        static internal int GetInputCount(this ComputationGateType type)
        {
            switch(type)
            {
                case ComputationGateType.COMPARE_AND_SWAP: return 2;
                default:
                    Debug.Assert(false, "Should not reach here");
                    return 0;
            }
        }

        static internal int GetOutputCount(this ComputationGateType type)
        {
            switch (type)
            {
                case ComputationGateType.COMPARE_AND_SWAP: return 2;
                default:
                    Debug.Assert(false, "Should not reach here");
                    return 0;
            }
        }

        static internal string GetName(this ComputationGateType type)
        {
            switch (type)
            {
                case ComputationGateType.COMPARE_AND_SWAP: return "C&S";
                default:
                    Debug.Assert(false, "Should not reach here");
                    return null;
            }
        }
    }
    
    public class ComputationGate : Gate
    {
        public int EvaluationQuorum;

        public readonly ComputationGateType Type;

        public ComputationGate(ComputationGateType type)
            : base(type.GetInputCount(), type.GetOutputCount())
        {
            Type = type;
        }

        public ComputationGate(ComputationGateType type, int inputCount, int outputCount)
            : base(inputCount, outputCount)
        {
            Type = type;
        }

        public override Gate Copy()
        {
            ComputationGate clone = new ComputationGate(Type);
            clone.EvaluationQuorum = EvaluationQuorum;
            return clone;
        }

        public override string ToString()
        {
            return base.ToString() + " Gate Type: " + Type.GetName();
        }
    }

    public class PermutationGate : Gate
    {
        private int[] Permutation;
        private Func<int, int> PermutationFunc;
        public int Count { get; private set; }
        public PermutationGate(int[] permutation)
            : base(permutation.Length, permutation.Length)
        {
            Permutation = permutation;
            Count = permutation.Length;
        }

        public PermutationGate(int count, Func<int, int> permutation)
            : base(count, count)
        {
            Count = count;
            PermutationFunc = permutation;
        }

        public int Permute(int input)
        {
            if (Permutation != null)
                return Permutation[input];
            return PermutationFunc(input);
        }

        public int Unpermute(int output)
        {
            for (int i = 0; i < Count; i++)
            {
                if ((Permutation != null && Permutation[i] == output) || (PermutationFunc(i) == output))
                    return i;
            }
            return -1;
        }

        public override Gate Copy()
        {
            if (Permutation != null)
            {
                return new PermutationGate((int[])Permutation.Clone());
            }
            else
                return new PermutationGate(Count, (Func<int, int>)PermutationFunc.Clone());
        }

        public override string ToString()
        {
            return base.ToString() + " Gate Type: Permutation";
        }
    }

    public static class PermutationGateFactory
    {
        public static PermutationGate CreateSwapGate()
        {
            Func<int, int> swapFunc = (a => (a == 0) ? 1 : 0);
            return new PermutationGate(2, swapFunc);
        }

        public static PermutationGate CreateNopGate()
        {
            return new PermutationGate(1, a => a);
        }

        public static PermutationGate CreateSplitGate(int count, int[] group, bool moveGroupToTop)
        {
            int[] sortedGroup = group.Clone() as int[];
            Array.Sort(sortedGroup);

            int[] perm = new int[count];

            int groupIndex = 0;
            int notGroupIndex = moveGroupToTop ? 0 : group.Length;
            for (int i = 0; i < count; i++)
            {
                if (groupIndex < group.Length && i == sortedGroup[groupIndex])
                    perm[groupIndex++] = i;
                else
                    perm[notGroupIndex++] = i;
            }

            return new PermutationGate(perm);
        }

        public static PermutationGate CreateUnshuffleGate(int count, int numGroups)
        {
            Debug.Assert(count % numGroups == 0);
            int countPerGroup = count / numGroups;
            // I will be in group "i mod numGroups".  To get to the start of that
            // multiply by countPerGroup.  My offset within the group is i/numGroups
            // since numGroups elemnts are grabbed for each offset
            Func<int, int> unshuffleFunc = (input => (input % numGroups) * countPerGroup + input / numGroups);
            return new PermutationGate(count, unshuffleFunc);

        }

        public static PermutationGate CreateMultiGroupInserterGate(int count, int groupSize, int numGroups)
        {
            // expect the first groupSize * groupCount elements to be the groups
            // and the next groupCount elements to be what we are supposed to add, one to each group
            // at the front of each group
            Func<int, int> inserterFunc = (input =>
            {
                if (input < groupSize * numGroups)
                    return input + input / groupSize + 1;
                else
                    return (input - groupSize * numGroups) * (groupSize + 1);
            });


            return new PermutationGate(count, inserterFunc);
        }

        public static PermutationGate CreateShuffleGate(int count, int numGroups)
        {
            Debug.Assert(count % numGroups == 0);

            int countPerGroup = count / numGroups;

            // take my position within the group and multiply to get which block i am in
            // then add my group number to get offset within block
            Func<int, int> shuffleFunc = (input => (input % countPerGroup) * numGroups + (input / countPerGroup));

            return new PermutationGate(count, shuffleFunc);
        }

        public static PermutationGate CreateInvertGate(int count)
        {
            return new PermutationGate(count, (input => (count - 1 - input)));
        }
    }
}
