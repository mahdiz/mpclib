using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MpcLib.DistributedSystem;
using MpcLib.SecretSharing;
using MpcLib.SecretSharing.QuorumShareRenewal;
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

    public class SecureMultiQuorumCircuitEvaluation : MultiQuorumProtocol<IDictionary<OutputGateAddress, Share<BigZp>>>
    {
        private List<Gate> GateList;
        private Dictionary<Gate, int> GateQuorumMapping = new Dictionary<Gate, int>();

        private Quorum MyQuorum;

        private IDictionary<InputGateAddress, Share<BigZp>> CircuitInputs;
        private Circuit Circuit;
        private BigInteger Prime;

        public SecureMultiQuorumCircuitEvaluation(Party me, Quorum myQuorum, Quorum[] quorums, ulong protocolId, Circuit circuit, IDictionary<InputGateAddress, Share<BigZp>> circuitInputs, BigInteger prime)
            : base(me, quorums, protocolId)
        {
            // assign gates to quorums
            GateList = circuit.TopologicalOrder;
            for (int i = 0; i < GateList.Count; i++)
                GateQuorumMapping[GateList[i]] = i % quorums.Length;

            Circuit = circuit;
            CircuitInputs = circuitInputs;
            Prime = prime;
            Result = new Dictionary<OutputGateAddress, Share<BigZp>>();
            MyQuorum = myQuorum;
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg.Type == MsgType.SubProtocolCompleted);
            var completedMsg = (SubProtocolCompletedMsg)msg;
            // all my gates finished! collect all of the results
            var outputList = completedMsg.Result;
            foreach (var output in completedMsg.Result)
            {
                var gateOutputs = (IDictionary<OutputGateAddress, Share<BigZp>>)output.Value;

                foreach (var gateOutput in gateOutputs)
                {
                    Result[gateOutput.Key] = gateOutput.Value;
                }
            }

            IsCompleted = true;
        }

        public override void Start()
        {
            List<Protocol> evalProtocols = new List<Protocol>();
            // we want to start protocols for all of the gates I'm involved with
            for (int i = 0; i < GateList.Count; i++)
            {
                Debug.Assert(GateList[i] is ComputationGate);

                var evalQuorum = Quorums[GateQuorumMapping[GateList[i]]];
                if (evalQuorum == MyQuorum)
                {
                    ulong evalProtocolId = ProtocolIdGenerator.GateEvalIdentifier(i);
                    evalProtocols.Add(new MultiQuorumGateEvaluation(Me, Quorums, GateList[i], GateQuorumMapping, Circuit, CircuitInputs, Prime, evalProtocolId));
                }
            }

            if (evalProtocols.Count > 0)
                ExecuteSubProtocols(evalProtocols);
            else
                IsCompleted = true;
        }
    }

    public class MultiQuorumGateEvaluation : MultiQuorumProtocol<IDictionary<OutputGateAddress, Share<BigZp>>>
    {
        private BigInteger Prime;

        private Quorum[] AllQuorums;
        private Gate EvalGate;
        private Dictionary<Gate, int> GateQuorumMapping;
        private Circuit Circuit;

        private IDictionary<InputGateAddress, Share<BigZp>> CircuitInputs;
        private Share<BigZp>[] InputShares;
        private int SharesReceived;
        private Dictionary<int, ulong> inputIdProtocolIdMap;

        private Share<BigZp>[] OutputShares;

        private Quorum EvalQuorum;

        private int Stage;

        public MultiQuorumGateEvaluation(Party me, Quorum[] allQuorums, Gate evalGate, Dictionary<Gate, int> gateQuorumMapping, Circuit circuit, IDictionary<InputGateAddress, Share<BigZp>> circuitInputs, BigInteger prime, ulong protocolId)
            : base(me, GetParticipatingQuorumList(evalGate, allQuorums, gateQuorumMapping, circuit), protocolId)
        {
            AllQuorums = allQuorums;
            EvalGate = evalGate;
            GateQuorumMapping = gateQuorumMapping;
            Circuit = circuit;
            Prime = prime;
            EvalQuorum = allQuorums[gateQuorumMapping[EvalGate]];
            CircuitInputs = circuitInputs;
            Result = new Dictionary<OutputGateAddress, Share<BigZp>>();
        }

        private static Quorum[] GetParticipatingQuorumList(Gate evalGate, Quorum[] allQuorums, Dictionary<Gate, int> gateQuorumMapping, Circuit circuit)
        {
            List<int> quorumsIds = new List<int>();

            // first add where we're receiving from
            for (int i = 0; i < evalGate.InputCount; i++)
            {
                if (circuit.InputConnectionCounterparties.ContainsKey(evalGate.GetLocalInputAddress(i)))
                {
                    var counterpartGate = circuit.InputConnectionCounterparties[evalGate.GetLocalInputAddress(i)].Gate;
                    quorumsIds.Add(gateQuorumMapping[counterpartGate]);
                }
            }

            quorumsIds.Add(gateQuorumMapping[evalGate]);

            // then add where we're sending to
            for (int i = 0; i < evalGate.OutputCount; i++)
            {
                if (circuit.OutputConnectionCounterparties.ContainsKey(evalGate.GetLocalOutputAddress(i)))
                {
                    var counterpartGate = circuit.OutputConnectionCounterparties[evalGate.GetLocalOutputAddress(i)].Gate;
                    quorumsIds.Add(gateQuorumMapping[counterpartGate]);
                }
            }

            return quorumsIds.Select(id => allQuorums[id]).ToArray();
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            if (Stage == 0)
            {
                ReceiveShareStage(msg);
                if (SharesReceived == EvalGate.InputCount)
                {
                    StartGateEvaluation();
                }
            }
            else if (Stage == 1)
            {
                Debug.Assert(msg.Type == MsgType.SubProtocolCompleted);
                Debug.Assert(EvalGate is CompareAndSwapGate);
                var completedMsg = (SubProtocolCompletedMsg)msg;

                OutputShares = new Share<BigZp>[EvalGate.OutputCount];
                CollectCompareAndSwapProtocol(completedMsg);
                SendShareStage();
            }
            else if (Stage == 2)
                IsCompleted = true;
        }


        private void ReceiveShareStage(Msg msg)
        {
            if (msg.Type == MsgType.SubProtocolCompleted)
            {
                var completedMsg = (SubProtocolCompletedMsg)msg;
                foreach (int key in inputIdProtocolIdMap.Keys)
                {
                    InputShares[key] = (Share<BigZp>)completedMsg.Result[inputIdProtocolIdMap[key]];
                    SharesReceived++;
                }
            }
            else
            {
                Debug.Assert(msg.Type == MsgType.Share);
                var loopbackMsg = (BigZpShareLoopbackMsg)msg;
                InputShares[loopbackMsg.WhichInputForGate] = loopbackMsg.Share;
                SharesReceived++;
            }
        }

        private void StartGateEvaluation()
        {
            Debug.Assert(EvalGate is CompareAndSwapGate);
            Stage = 1;
            ExecuteSubProtocol(ConstructCompareAndSwapProtocol());
        }

        private Protocol ConstructCompareAndSwapProtocol()
        {
            return new CompareAndSwapProtocol(Me, EvalQuorum, InputShares[0], InputShares[1]);
        }

        private void CollectCompareAndSwapProtocol(SubProtocolCompletedMsg msg)
        {
            Tuple<Share<BigZp>, Share<BigZp>> compOutput = (Tuple<Share<BigZp>, Share<BigZp>>)msg.SingleResult;

            OutputShares[0] = compOutput.Item1;
            OutputShares[1] = compOutput.Item2;
        }

        private void SendShareStage()
        {
            List<Protocol> reshareProtocols = new List<Protocol>();

            for (int i = 0; i < EvalGate.OutputCount; i++)
            {

                // figure out where this share is going to
                InputGateAddress counterpartGateAddr;
                if (Circuit.OutputConnectionCounterparties.TryGetValue(EvalGate.GetLocalOutputAddress(i), out counterpartGateAddr))
                {
                    var counterpartGate = counterpartGateAddr.Gate;
                    var counterpartQuorum = AllQuorums[GateQuorumMapping[counterpartGate]];

                    bool needLoopback;
                    bool needReshare;

                    if (counterpartQuorum == EvalQuorum)
                    {
                        needLoopback = true;
                        needReshare = false;
                    }
                    else if (counterpartQuorum.HasMember(Me.Id))
                    {
                        needLoopback = true;
                        needReshare = true;
                    }
                    else
                    {
                        needLoopback = false;
                        needReshare = true;
                    }

                    if (needLoopback)
                    {
                        // loop a message back to the other protocol
                        ulong counterpartEvalId = ProtocolIdGenerator.GateEvalIdentifier(counterpartGate.TopologicalRank);
                        NetSimulator.Loopback(Me.Id, counterpartEvalId, new BigZpShareLoopbackMsg(OutputShares[i], counterpartGateAddr.Port));
                    }
                    if (needReshare)
                    {
                        reshareProtocols.Add(new QuorumShareRenewalProtocol(Me, EvalQuorum, counterpartQuorum, OutputShares[i], Prime,
                            ProtocolIdGenerator.GateInputSharingIdentifier(counterpartGate.TopologicalRank, counterpartGateAddr.Port)));
                    }
                }
                else
                {
                    // this was a circuit output
                    Result[EvalGate.GetLocalOutputAddress(i)] = OutputShares[i];
                }
            }

            Stage = 2;

            if (reshareProtocols.Count > 0)
            {
                ExecuteSubProtocols(reshareProtocols);
            }
            else
            {
                IsCompleted = true;
            }
            
        }

        public override void Start()
        {
            InputShares = new Share<BigZp>[EvalGate.InputCount];

            List<Protocol> receiveSubProtocols = new List<Protocol>();
            inputIdProtocolIdMap = new Dictionary<int, ulong>();

            // start subprotocols to receive the input values
            for (int i = 0; i < EvalGate.InputCount; i++)
            {
                OutputGateAddress counterpartGateAddr;
                if (Circuit.InputConnectionCounterparties.TryGetValue(EvalGate.GetLocalInputAddress(i), out counterpartGateAddr))
                {
                    var counterpartGate = counterpartGateAddr.Gate;
                    var counterpartQuorum = AllQuorums[GateQuorumMapping[counterpartGate]];
                    if (!counterpartQuorum.HasMember(Me.Id))
                    {
                        ulong recvProtocolId = ProtocolIdGenerator.GateInputSharingIdentifier(EvalGate.TopologicalRank, i);
                        // I expect to receive this value from a quorum resharing
                        receiveSubProtocols.Add(new QuorumShareRenewalProtocol(Me, counterpartQuorum, EvalQuorum, null, Prime, recvProtocolId));
                        inputIdProtocolIdMap[i] = recvProtocolId;
                    }
                }
                else
                {
                    InputShares[i] = CircuitInputs[EvalGate.GetLocalInputAddress(i)];
                    SharesReceived++;
                }
            }

            if (receiveSubProtocols.Count > 0)
            {
                Stage = 0;
                ExecuteSubProtocols(receiveSubProtocols);
            }
            else if (SharesReceived == EvalGate.InputCount)
                StartGateEvaluation();
        }
    }

    public class BigZpShareLoopbackMsg : Msg
    {
        public readonly Share<BigZp> Share;
        public readonly int WhichInputForGate;

        public BigZpShareLoopbackMsg(Share<BigZp> share, int whichInputForGate)
            : base(MsgType.Share)
        {
            Share = share;
            WhichInputForGate = whichInputForGate;
        }
    }

}
