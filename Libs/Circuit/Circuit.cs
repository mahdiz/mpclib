using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MpcLib.Common.BasicDataStructures.Graph;
using System.Diagnostics;

namespace MpcLib.Circuit
{
    public class Circuit : ICloneable
    {
        protected ISet<Gate> Gates;

        public ISet<Gate> InputGates { get; private set; }
        public ISet<Gate> OutputGates { get; private set; }

        public Circuit()
        {
            Gates = new HashSet<Gate>();
            InputGates = new HashSet<Gate>();
            OutputGates = new HashSet<Gate>();
        }

        // create a copy of the gate and add it
        public void AddGate(Gate gate)
        {
            Debug.Assert(gate.InputCount == gate.InputConnections.Length);
            Debug.Assert(gate.OutputCount == gate.OutputConnections.Length);

            // insert before checks to allow loopback
            Gates.Add(gate);

            bool isInput = false, isOutput = false;
            for (int i = 0; i < gate.InputConnections.Length; i++)
            {
                var inputFrom = gate.InputConnections[i];
                if (inputFrom == null)
                {
                    isInput = true;
                    continue;
                }
                
                Debug.Assert(Gates.Contains(inputFrom.Gate));
                AddConnection(inputFrom, new GateAddress(gate, i));
            }

            for (int i = 0; i < gate.OutputConnections.Length; i++)
            {
                var outputTo = gate.OutputConnections[i];
                if (outputTo == null)
                {
                    isOutput = true;
                    continue;
                }

                Debug.Assert(Gates.Contains(outputTo.Gate));
                AddConnection(new GateAddress(gate, i), outputTo);
            }

            if (isInput)
            {
            }
            if (isOutput)
            {
                OutputGates.Add(gate);
            }
        }

        public void AddConnection(GateAddress inputFrom, GateAddress outputTo)
        {
            Debug.Assert(Gates.Contains(inputFrom.Gate));
            Debug.Assert(Gates.Contains(outputTo.Gate));

            // we need to remove the old connections
            GateAddress oldOutputTo = inputFrom.Gate.GetOutputConnection(inputFrom.Port);
            GateAddress oldInputFrom = outputTo.Gate.GetInputConnection(outputTo.Port);

            if (!outputTo.Equals(oldOutputTo))
            {
                // remove the old connection and make a new one
                if (oldOutputTo != null)
                {
                    oldOutputTo.Gate.SetInputConnection(oldOutputTo.Port, null);
                    InputGates.Add(oldOutputTo.Gate);
                }

                inputFrom.Gate.SetOutputConnection(inputFrom.Port, outputTo);
                if (!inputFrom.Gate.HasFreeOutputs)
                {
                    OutputGates.Remove(inputFrom.Gate);
                }
            }

            if (!inputFrom.Equals(oldInputFrom))
            {
                if (oldInputFrom != null)
                {
                    oldInputFrom.Gate.SetOutputConnection(oldInputFrom.Port, null);
                    OutputGates.Add(oldInputFrom.Gate);
                }

                outputTo.Gate.SetInputConnection(outputTo.Port, inputFrom);
                if (!outputTo.Gate.HasFreeInputs)
                {
                    InputGates.Remove(outputTo.Gate);
                }
            }
        }

        public void JoinWith(Circuit c, IList<Tuple<GateAddress, GateAddress>> joins)
        {
            // add all of the gates in the other circuit.  we need to make sure that there are there is no overlap
            Debug.Assert(!Gates.Intersect(c.Gates).Any());

            Gates.UnionWith(c.Gates);
            InputGates.UnionWith(c.InputGates);
            OutputGates.UnionWith(c.OutputGates);

            foreach (var join in joins)
            {
                var inputFrom = join.Item1;
                var outputTo = join.Item2;
                AddConnection(inputFrom, outputTo);
            }
        }

        public object Clone()
        {
            Dictionary<Gate, Gate> d;
            return Clone(out d);
        }

        public object Clone(out Dictionary<Gate, Gate> mapping)
        {
            var clone = new Circuit();

            mapping = new Dictionary<Gate, Gate>();

            // add all of the gates and connections that I have
            foreach (var gate in Gates)
            {
                var gateClone = gate.Clone() as Gate;
                clone.Gates.Add(gateClone);
                if (InputGates.Contains(gate))
                    clone.InputGates.Add(gateClone);
                if (OutputGates.Contains(gate))
                    clone.OutputGates.Add(gateClone);

                mapping.Add(gate, gateClone);
            }

            foreach (var gate in Gates)
            {
                mapping[gate].UpdateNeighborsWhenCloned(gate, mapping);
            }
            
            return clone;
        }
    }

    public class PermutationNetwork : ICloneable
    {
        public int WireCount { get; private set; }

        public Circuit Circuit { get; private set; }

        private GateAddress[] LastGateForWire;
        private GateAddress[] FirstGateForWire;

        public PermutationNetwork(int wireCount)
        {
            WireCount = wireCount;
            LastGateForWire = new GateAddress[wireCount];
            FirstGateForWire = new GateAddress[wireCount];
            Circuit = new Circuit();
        }

        public void AppendGate(Gate gate, int[] wires)
        {
            Debug.Assert(gate.InputCount == gate.OutputCount);
            Debug.Assert(gate.InputCount == wires.Length);

            for (int i = 0; i < wires.Length; i++)
            {
                int wire = wires[i];
                gate.InputConnections[i] = LastGateForWire[wire];
                LastGateForWire[wire] = new GateAddress(gate, i);
                if (FirstGateForWire[wire] == null)
                {
                    FirstGateForWire[wire] = LastGateForWire[wire];
                }
            }

            Circuit.AddGate(gate);
        }

        public void AppendGate(Gate gate, int startWire)
        {
            Debug.Assert(WireCount <= startWire + gate.InputCount);
            // wasteful but good enough for now

            int[] wires = new int[gate.InputCount];
            for (int i = 0; i < gate.InputCount; i++)
            {
                wires[i] = startWire + i;
            }

            AppendGate(gate, wires);
        }

        public void AppendNetwork(PermutationNetwork pn, int[] wires)
        {
            // append the provided network to this one. join wires[i] to i
            Debug.Assert(wires.Length == pn.WireCount);
            List<Tuple<GateAddress, GateAddress>> joins = new List<Tuple<GateAddress, GateAddress>>();

            for (int i = 0; i < wires.Length; i++)
            {
                if (LastGateForWire[wires[i]] != null && pn.FirstGateForWire[i] != null)
                {
                    joins.Add(new Tuple<GateAddress, GateAddress>(LastGateForWire[wires[i]], pn.FirstGateForWire[i]));
                }

                if (pn.LastGateForWire[i] != null)
                {
                    LastGateForWire[wires[i]] = pn.LastGateForWire[i];
                }

                if (FirstGateForWire[wires[i]] == null)
                {
                    FirstGateForWire[wires[i]] = pn.FirstGateForWire[i];
                }
            }

            Circuit.JoinWith(pn.Circuit, joins);
        }

        public void AppendNetwork(PermutationNetwork pn, int startWire)
        {
            Debug.Assert(WireCount <= startWire + pn.WireCount);

            // wasteful but good enough for now
            int[] mapping = new int[pn.WireCount];

            for (int i = 0; i < pn.WireCount; i++)
            {
                mapping[i] = startWire + i;
            }

            AppendNetwork(pn, mapping);
        }

        public object Clone()
        {
            var clone = new PermutationNetwork(WireCount);
            
            Dictionary<Gate, Gate> mapping;

            clone.Circuit = Circuit.Clone(out mapping) as Circuit;

            for (int i = 0; i < WireCount; i++)
            {
                if (FirstGateForWire[i] != null)
                    clone.FirstGateForWire[i] = new GateAddress(mapping[FirstGateForWire[i].Gate], FirstGateForWire[i].Port);
                if (LastGateForWire[i] != null)
                    clone.LastGateForWire[i] = new GateAddress(mapping[LastGateForWire[i].Gate], LastGateForWire[i].Port);
            }

            return clone;
        }
    }
}
