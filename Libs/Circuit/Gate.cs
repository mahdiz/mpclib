using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.Circuit
{
    public abstract class Gate : ICloneable, IEquatable<Gate>
    {
        public int InputCount { get; protected set; }
        public int OutputCount { get; protected set; }

        public GateAddress[] InputConnections;
        public GateAddress[] OutputConnections;

        public int FreeInputs;
        public int FreeOutputs;

        public bool HasFreeInputs
        {
            get
            {
                return FreeInputs > 0;
            }
        }

        public bool HasFreeOutputs
        {
            get
            {
                return FreeOutputs > 0;
            }
        }

        public Gate(int inputCount, int outputCount)
        {
            InputCount = inputCount;
            OutputCount = outputCount;
            InputConnections = new GateAddress[inputCount];
            OutputConnections = new GateAddress[outputCount];
        }

        public GateAddress GetInputConnection(int port)
        {
            return InputConnections[port];
        }

        public void SetInputConnection(int port, GateAddress inputFrom)
        {
            bool wasNull = (InputConnections[port] == null);
            InputConnections[port] = inputFrom;
            if (wasNull && inputFrom != null)
                FreeInputs--;
            else if (!wasNull && inputFrom == null)
                FreeInputs++;
        }

        public GateAddress GetOutputConnection(int port)
        {
            return OutputConnections[port];
        }

        public void SetOutputConnection(int port, GateAddress outputTo)
        {
            bool wasNull = (OutputConnections[port] == null);
            OutputConnections[port] = outputTo;
            if (wasNull && outputTo != null)
                FreeOutputs--;
            else if (!wasNull && outputTo == null)
                FreeOutputs++;
        }

        public void UpdateNeighborsWhenCloned(Gate old, Dictionary<Gate, Gate> cloneMapping)
        {
            for (int i = 0; i < InputCount; i++)
            {
                if (old.InputConnections[i] != null)
                    InputConnections[i] = new GateAddress(cloneMapping[old.InputConnections[i].Gate], old.InputConnections[i].Port);
            }

            for (int i = 0; i < OutputCount; i++)
            {
                if (old.OutputConnections[i] != null)
                    OutputConnections[i] = new GateAddress(cloneMapping[old.OutputConnections[i].Gate], old.OutputConnections[i].Port);
            }
        }

        public abstract object Clone();
        public abstract bool Equals(Gate other);
    }


    public class GateAddress
    {
        public readonly Gate Gate;
        public readonly int Port;

        public GateAddress(Gate gate, int port)
        {
            Gate = gate;
            Port = port;
        }

        public override bool Equals(object o)
        {
            if (!(o is GateAddress))
                return false;

            GateAddress other = o as GateAddress;

            return Gate.Equals(other.Gate) && Port == other.Port;
        }

        public override int GetHashCode()
        {
            return 31 * Gate.GetHashCode() + 39 * Port;
        }
    }

    public class CompareAndSwapGate : Gate
    {
        public CompareAndSwapGate()
            : base(2, 2)
        {
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(Gate other)
        {
            throw new NotImplementedException();
        }
    }

    public class PermutationGate : Gate
    {
        private int[] Permutation;
        public int Count { get; private set; }
        public PermutationGate(int[] permutation)
            : base(permutation.Length, permutation.Length)
        {
            Permutation = permutation;
            Count = permutation.Length;
        }

        public PermutationGate(int count)
            : base(count, count)
        {
            Count = count;
        }

        public virtual int Permute(int input)
        {
            return Permutation[input];
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(Gate other)
        {
            throw new NotImplementedException();
        }
    }

    public class SwapGate : PermutationGate
    {
        public SwapGate()
            : base(2)
        { 
        }

        public override int Permute(int input)
        {
            return (input == 0) ? 1 : 0;
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(Gate other)
        {
            throw new NotImplementedException();
        }
    }

    public class NopGate : PermutationGate
    {
        public NopGate()
            : base(1)
        {
        }

        public override int Permute(int input)
        {
            return 0;
        }
    }

    public class SplitGate : PermutationGate
    {
        public SplitGate(int count, int[] group, bool moveGroupToTop)
            : base(GeneratePermutationFromSplit(count, group, moveGroupToTop))
        {
        }
        
        // maintain the ordering within each group
        private static int[] GeneratePermutationFromSplit(int count, int[] group, bool moveGroupToTop)
        {
            int[] sortedGroup = group.Clone() as int[];
            Array.Sort(sortedGroup);

            int[] perm = new int[count];

            int groupIndex = 0;
            int notGroupIndex = moveGroupToTop ? 0 : group.Length;
            for (int i = 0; i < count; i++)
            {
                if (i == sortedGroup[groupIndex])
                    groupIndex++;
                else
                    perm[notGroupIndex++] = i;
            }

            Array.Copy(sortedGroup, 0, perm, moveGroupToTop ? 0 : notGroupIndex, sortedGroup.Length);

            return perm;
        }
    }

    public class UnshuffleGate : PermutationGate
    {
        private int CountPerGroup, NumGroups;

        public UnshuffleGate(int count, int numGroups)
            : base(count)
        {
            Debug.Assert(count % numGroups == 0);
            NumGroups = numGroups;
            CountPerGroup = count / numGroups;
        }

        public override int Permute(int input)
        {
            // I will be in group "i mod numGroups".  To get to the start of that
            // multiply by countPerGroup.  My offset within the group is i/numGroups
            // since numGroups elemnts are grabbed for each offset
            return (input % NumGroups) * CountPerGroup + input / NumGroups;
        }
        
    }
    
    public class MultiGroupInserterGate : PermutationGate
    {
        private int GroupSize, NumGroups;

        public MultiGroupInserterGate(int count, int groupSize, int numGroups)
            : base(count)
        {
            Debug.Assert(count == groupSize * (numGroups + 1));

            GroupSize = groupSize;
            NumGroups = numGroups;
        }


        // expect the first groupSize * groupCount elements to be the groups
        // and the next groupCount elements to be what we are supposed to add, one to each group
        // at the front of each group
        public override int Permute(int input)
        {
            if (input < GroupSize*NumGroups)
            {
                return input + input / GroupSize + 1;
            }
            else
            {
                return (input - GroupSize * NumGroups) * (GroupSize + 1);
            }
        }
    }

    public class ShuffleGate : PermutationGate
    {
        private int CountPerGroup, NumGroups;

        public ShuffleGate(int count, int numGroups)
            : base(count)
        {
            Debug.Assert(count % numGroups == 0);

            CountPerGroup = count / numGroups;
        }

        public override int Permute(int input)
        {
            // take my position within the group and multiply to get which block i am in
            // then add my group number to get offset within block
            return (input % CountPerGroup) * NumGroups + (input / CountPerGroup);
        }

    }

    public class InvertGate : PermutationGate
    {
        public InvertGate(int count)
            : base(count)
        {
        }

        public override int Permute(int input)
        {
            return Count - 1 - input;
        }
    }
}
