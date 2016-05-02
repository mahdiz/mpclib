using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MpcLib.Common.BasicDataStructures.Graph;
using System.Diagnostics;

namespace MpcLib.Circuits
{
    public class Circuit : ICloneable
    {
        protected ISet<Gate> Gates;

        public ISet<InputGateAddress> InputAddrs { get; private set; }
        public ISet<OutputGateAddress> OutputAddrs { get; private set; }
        

        public IDictionary<OutputGateAddress, InputGateAddress> OutputConnectionCounterparties;
        public IDictionary<InputGateAddress, OutputGateAddress> InputConnectionCounterparties;

        private List<Gate> TopologicalOrderImpl;

        public List<Gate> TopologicalOrder
        {
            get
            {
                if (TopologicalOrderImpl == null)
                    TopologicalOrderImpl = AssignTopologicalRanks();
                return TopologicalOrderImpl;
            }
        }


        public Circuit()
        {
            Gates = new HashSet<Gate>();
            InputAddrs = new HashSet<InputGateAddress>();
            OutputAddrs = new HashSet<OutputGateAddress>();

            OutputConnectionCounterparties = new Dictionary<OutputGateAddress, InputGateAddress>();
            InputConnectionCounterparties = new Dictionary<InputGateAddress, OutputGateAddress>();
        }
        
        public void AddGate(Gate gate, IEnumerable<GateConnection> connections)
        {
            // insert before checks to allow loopback
            Gates.Add(gate);

            for (int i = 0; i < gate.InputCount; i++)
                InputAddrs.Add(new InputGateAddress(gate, i));
            for (int i = 0; i < gate.OutputCount; i++)
                OutputAddrs.Add(new OutputGateAddress(gate, i));

            foreach (var connection in connections)
            {
                AddConnection(connection);
            }

            TopologicalOrderImpl = null;
        }


        public void AddConnection(GateConnection connection)
        {
            // create a connection from output to input

            if (OutputConnectionCounterparties.ContainsKey(connection.FromAddr))
            {
                // the "from" in this connection already has a counterparty.  want to remove it
                var oldTo = OutputConnectionCounterparties[connection.FromAddr];
                InputConnectionCounterparties.Remove(oldTo);
                InputAddrs.Add(oldTo);
            }

            if (InputConnectionCounterparties.ContainsKey(connection.ToAddr))
            {
                // the "to" in this connection already has a counterparty.  want to remove it
                var oldFrom = InputConnectionCounterparties[connection.ToAddr];
                OutputConnectionCounterparties.Remove(oldFrom);
                OutputAddrs.Add(oldFrom);
            }

            OutputConnectionCounterparties[connection.FromAddr] = connection.ToAddr;
            OutputAddrs.Remove(connection.FromAddr);
            InputConnectionCounterparties[connection.ToAddr] = connection.FromAddr;
            InputAddrs.Remove(connection.ToAddr);
            
            TopologicalOrderImpl = null;
        }

        public void JoinWith(Circuit c, IList<GateConnection> joins)
        {
            // add all of the gates in the other circuit.  we need to make sure that there are there is no overlap
            Debug.Assert(!Gates.Intersect(c.Gates).Any());

            Gates.UnionWith(c.Gates);
            InputAddrs.UnionWith(c.InputAddrs);
            OutputAddrs.UnionWith(c.OutputAddrs);
            
            foreach (var inAddr in c.InputConnectionCounterparties.Keys)
            {
                var outAddr = c.InputConnectionCounterparties[inAddr];
                InputConnectionCounterparties[inAddr] = outAddr;
                OutputConnectionCounterparties[outAddr] = inAddr;
            }

            foreach (var join in joins)
            {
                AddConnection(join);
            }

            TopologicalOrderImpl = null;
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
                var gateClone = gate.Copy() as Gate;
                clone.Gates.Add(gateClone);
                mapping.Add(gate, gateClone);
            }

            foreach (var input in InputAddrs)
            {
                clone.InputAddrs.Add(new InputGateAddress(mapping[input.Gate], input.Port));
            }

            foreach (var output in OutputAddrs)
            {
                clone.OutputAddrs.Add(new OutputGateAddress(mapping[output.Gate], output.Port));
            }

            foreach (var oldInputGate in InputConnectionCounterparties.Keys)
            {
                var newInputGate = new InputGateAddress(mapping[oldInputGate.Gate], oldInputGate.Port);
                var oldOutputGate = InputConnectionCounterparties[oldInputGate];
                var newOutputGate = new OutputGateAddress(mapping[oldOutputGate.Gate], oldOutputGate.Port);

                clone.InputConnectionCounterparties[newInputGate] = newOutputGate;
                clone.OutputConnectionCounterparties[newOutputGate] = newInputGate;
            }

            return clone;
        }

        // DOES NOT DEAL WITH CIRCUITS WITH CYCLES!!!
        private List<Gate> AssignTopologicalRanks()
        {
            // grab an item from the input set
            var inEnum = InputAddrs.GetEnumerator();
            inEnum.MoveNext();
            var gateDeque = new LinkedList<Gate>();
            while (inEnum.MoveNext())
                gateDeque.AddFirst(inEnum.Current.Gate);

            var sortList = new List<Gate>();

            int nextRank = 0;

            while (gateDeque.Count > 0)
            {
                var current = gateDeque.First.Value;

                if (current.TopologicalRank != Gate.NO_RANK)
                {
                    // already assigned a rank to this. can move on to next
                    gateDeque.RemoveFirst();
                    continue;
                }

                // check to see if all of the predecessors have a rank assigned. If any don't then add them to the front of the queue
                bool shouldAssignRank = true;

                for (int i = 0; i < current.InputCount; i++)
                {
                    var addr = current.GetLocalInputAddress(i);
                    if (InputAddrs.Contains(addr))
                        continue;

                    var counterparty = InputConnectionCounterparties[addr].Gate;
                    if (counterparty.TopologicalRank == Gate.NO_RANK)
                    {
                        gateDeque.AddFirst(counterparty);
                        shouldAssignRank = false;
                    }

                }

                if (shouldAssignRank)
                {
                    current.TopologicalRank = nextRank;
                    nextRank++;
                    sortList.Add(current);
                    gateDeque.RemoveFirst();

                    // add all successors
                    for (int i = 0; i < current.OutputCount; i++)
                    {
                        var addr = current.GetLocalOutputAddress(i);
                        if (OutputAddrs.Contains(addr))
                            continue;

                        gateDeque.AddLast(OutputConnectionCounterparties[addr].Gate);
                    }
                }
                // otherwise a predecessor still needs a rank
            }

            return sortList;
        }

        public void CollapsePermutationGates()
        {
            foreach (var gate in Gates.ToList())
            {
                if (gate is PermutationGate)
                {
                    PermutationGate pgate = (PermutationGate)gate;

                    for (int i = 0; i < pgate.Count; i++)
                    {
                        OutputGateAddress inputCounterparty = InputConnectionCounterparties[pgate.GetLocalInputAddress(i)];
                        InputGateAddress outputCounterparty = OutputConnectionCounterparties[pgate.GetLocalOutputAddress(pgate.Permute(i))];

                        AddConnection(new GateConnection(inputCounterparty, outputCounterparty));
                    }

                    RemoveGate(pgate);
                }
            }
        }

        public void RemoveGate(Gate gate)
        {
            for (int i = 0; i < gate.InputCount; i++)
            {
                var inAddr = gate.GetLocalInputAddress(i);
                Debug.Assert(!InputConnectionCounterparties.ContainsKey(inAddr));
                InputAddrs.Remove(inAddr);
            }

            for (int i = 0; i < gate.OutputCount; i++)
            {
                var outAddr = gate.GetLocalOutputAddress(i);
                Debug.Assert(!OutputConnectionCounterparties.ContainsKey(outAddr));
                OutputAddrs.Remove(outAddr);
            }

            Gates.Remove(gate);
        }

        public override string ToString()
        {
            List<Gate> gates = TopologicalOrder;
            // that should also assign all gates to a number

            string str = "";

            str += "Input Addrs: " + string.Join(" ", InputAddrs.Select(addr => addr.Render())) + "\n";
            str += "Output Addrs: " + string.Join(" ", OutputAddrs.Select(addr => addr.Render())) + "\n";

            str += string.Join("\n", gates);

            str += "\n";

            str += string.Join("\n", OutputConnectionCounterparties.Select(kv => kv.Key + " -> " + kv.Value));

            str += "\n";

            return str;
        }
    }

    public class PermutationNetwork : ICloneable
    {
        public int WireCount { get; private set; }

        public Circuit Circuit { get; private set; }

        public OutputGateAddress[] LastGateForWire;
        public InputGateAddress[] FirstGateForWire;

        private List<Gate>[] WireGateList;

        public PermutationNetwork(int wireCount)
        {
            WireCount = wireCount;
            LastGateForWire = new OutputGateAddress[wireCount];
            FirstGateForWire = new InputGateAddress[wireCount];
            Circuit = new Circuit();

            WireGateList = new List<Gate>[wireCount];
            for (int i = 0; i < wireCount; i++)
                WireGateList[i] = new List<Gate>();
        }

        public void AppendGate(Gate gate, int[] wires)
        {
            Debug.Assert(gate.InputCount == gate.OutputCount);
            Debug.Assert(gate.InputCount == wires.Length);

            ISet<GateConnection> joins = new HashSet<GateConnection>();

            for (int i = 0; i < wires.Length; i++)
            {
                int wire = wires[i];
                if (LastGateForWire[wire] != null)
                    joins.Add(new GateConnection(LastGateForWire[wire], gate.GetLocalInputAddress(i)));

                LastGateForWire[wire] = gate.GetLocalOutputAddress(i);
                if (FirstGateForWire[wire] == null)
                    FirstGateForWire[wire] = gate.GetLocalInputAddress(i);

                WireGateList[wire].Add(gate);
            }

            Circuit.AddGate(gate, joins);
        }

        public void AppendGate(Gate gate, int startWire)
        {
            Debug.Assert(startWire + gate.InputCount <= WireCount);
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
            List<GateConnection> joins = new List<GateConnection>();

            for (int i = 0; i < wires.Length; i++)
            {
                int wire = wires[i];
                if (LastGateForWire[wire] != null && pn.FirstGateForWire[i] != null)
                {
                    joins.Add(new GateConnection(LastGateForWire[wire], pn.FirstGateForWire[i]));
                }

                if (pn.LastGateForWire[i] != null)
                {
                    LastGateForWire[wire] = pn.LastGateForWire[i];
                }

                if (FirstGateForWire[wire] == null)
                {
                    FirstGateForWire[wire] = pn.FirstGateForWire[i];
                }

                WireGateList[wire].AddRange(pn.WireGateList[i]);
            }

            Circuit.JoinWith(pn.Circuit, joins);
        }

        public void AppendNetwork(PermutationNetwork pn, int startWire)
        {
            Debug.Assert(startWire + pn.WireCount <= WireCount);

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
                    clone.FirstGateForWire[i] = new InputGateAddress(mapping[FirstGateForWire[i].Gate], FirstGateForWire[i].Port);
                if (LastGateForWire[i] != null)
                    clone.LastGateForWire[i] = new OutputGateAddress(mapping[LastGateForWire[i].Gate], LastGateForWire[i].Port);
            }

            return clone;
        }

        public void CollapsePermutationGates()
        {
            // unwind input and output addresses until we find an address on a non-permutation gate
            for (int i = 0; i < WireCount; i++)
            {
                InputGateAddress inAddr = FirstGateForWire[i];
                while (inAddr != null && inAddr.Gate is PermutationGate)
                {
                    var pgate = (PermutationGate)inAddr.Gate;
                    var permuteOut = pgate.GetLocalOutputAddress(pgate.Permute(inAddr.Port));

                    Circuit.OutputConnectionCounterparties.TryGetValue(permuteOut, out inAddr);
                }

                FirstGateForWire[i] = inAddr;
            }

            for (int i = 0; i < WireCount; i++)
            {
                OutputGateAddress outAddr = LastGateForWire[i];
                while (outAddr != null && outAddr.Gate is PermutationGate)
                {
                    var pgate = (PermutationGate)outAddr.Gate;
                    var permuteIn = pgate.GetLocalInputAddress(pgate.Unpermute(outAddr.Port));

                    Circuit.InputConnectionCounterparties.TryGetValue(permuteIn, out outAddr);
                }

                LastGateForWire[i] = outAddr;
            }

            Circuit.CollapsePermutationGates();
        }

        public override string ToString()
        {
            string str = Circuit.ToString();
            str += "First Gate Per Wire: " + string.Join(" ", FirstGateForWire.Select(addr => addr.Render())) + "\n";
            str += "Last Gate Per Wire: " + string.Join(" ", LastGateForWire.Select(addr => addr.Render())) + "\n";

            for (int i = 0; i < WireCount; i++)
            {
                str += "Wire " + i + ": " + string.Join(" ", WireGateList[i].Select(g => g.TopologicalRank)) + "\n";
            }

            return str;
        }
    }
}
