using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MpcLib.DistributedSystem;
using MpcLib.SecretSharing;
using MpcLib.Common.FiniteField;

namespace MpcLib.Circuits
{
    // for now assume only 1 quorum
    public class SecureGroupCircuitEvaluation : QuorumProtocol<IDictionary<OutputGateAddress, Share<BigZp>>>
    {
        private List<Gate> GateList;
        private int currentGate = 0;

        private IDictionary<InputGateAddress, Share<BigZp>> CircuitInputs;
        private IDictionary<OutputGateAddress, Share<BigZp>> GateResults;
        private Circuit Circuit;

        public SecureGroupCircuitEvaluation(Party me, Quorum parties, Circuit circuit, IDictionary<InputGateAddress, Share<BigZp>> circuitInputs)
            : base(me, parties)
        {
            // assume that quorums have already been assigned to the gates in the circuit

            GateList = circuit.TopologicalOrder;
            CircuitInputs = circuitInputs;
            Circuit = circuit;

            GateResults = new Dictionary<OutputGateAddress, Share<BigZp>>();
            Result = new Dictionary<OutputGateAddress, Share<BigZp>>();
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            var completedMsg = (SubProtocolCompletedMsg)msg;

            var result = (Tuple<Share<BigZp>, Share<BigZp>>)completedMsg.SingleResult;
            var gate = GateList[currentGate];

            GateResults[gate.GetLocalOutputAddress(0)] = result.Item1;
            GateResults[gate.GetLocalOutputAddress(1)] = result.Item2;

            currentGate++;

            EvaluateGates();
        }

        public override void Start()
        {
            // since there's only 1 quorum, we want to just start evaluating gates
            EvaluateGates();
        }

        private void EvaluateGates()
        {
            while (currentGate < GateList.Count)
            {
                Gate current = GateList[currentGate];

                if (current is ComputationGate)
                {
                    EvaluateComputationGate((ComputationGate)current);
                    break;
                }
                else
                {
                    if (current is PermutationGate)
                    {
                        // can do all this locally
                        EvaluatePermutationGate((PermutationGate)current);
                    }
                }
                currentGate++;
            }

            if (currentGate == GateList.Count)
            {
                // done evaluating the circuit! construct the result
                foreach (var outAddr in Circuit.OutputAddrs)
                {
                    Debug.Assert(GateResults[outAddr] != null);
                    Result[outAddr] = GateResults[outAddr];
                }
                IsCompleted = true;

            }
        }

        private void EvaluateComputationGate(ComputationGate gate)
        {
            if (gate is CompareAndSwapGate)
            {
                var input0 = GetGateInput(gate.GetLocalInputAddress(0));
                var input1 = GetGateInput(gate.GetLocalInputAddress(1));
                
                ExecuteSubProtocol(new CompareAndSwapProtocol(Me, Quorum, input0, input1));
            }
        }

        private void EvaluatePermutationGate(PermutationGate gate)
        {
            for (int i = 0; i < gate.Count; i++)
            {
                var inVal = GetGateInput(gate.GetLocalInputAddress(i));
                var outAddr = gate.GetLocalOutputAddress(gate.Permute(i));

                GateResults[outAddr] = inVal;
            }
        }

        private Share<BigZp> GetGateInput(InputGateAddress addr)
        {
            Share<BigZp> ret;

            if (Circuit.InputAddrs.Contains(addr))
                ret = CircuitInputs[addr];
            else
                ret = GateResults[Circuit.InputConnectionCounterparties[addr]];

            Debug.Assert(ret != null);
            return ret;

        }
    }
}
