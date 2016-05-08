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
    public interface IGateEvaluationProtocolFactory<T> where T : class
    {
        QuorumProtocol<T[]> GetEvaluationProtocolFor(ComputationGateType type, Party me, Quorum quorum, T[] inputs);
        MultiQuorumProtocol<T> GetResharingProtocol(Party me, Quorum fromQuorum, Quorum toQuorum, T value, int gateNumber, int portNumber);
    }

    /*
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
    */

    public class SecureMultiQuorumCircuitEvaluation<T> : MultiQuorumProtocol<IDictionary<OutputGateAddress, T>> where T : class
    {
        private List<Gate> GateList;
        private IDictionary<Gate, Quorum> GateQuorumMapping;

        private Quorum MyQuorum;

        private IDictionary<InputGateAddress, T> CircuitInputs;
        private Circuit Circuit;
        private BigInteger Prime;
        private IGateEvaluationProtocolFactory<T> ProtocolFactory;

        public SecureMultiQuorumCircuitEvaluation(Party me, Quorum myQuorum, Quorum[] quorums, ulong protocolId, Circuit circuit,
            IDictionary<InputGateAddress, T> circuitInputs, IGateEvaluationProtocolFactory<T> protocolFactory,
            IDictionary<Gate, Quorum> gateQuorumMapping, BigInteger prime)
            : base(me, quorums, protocolId)
        {
            // assign gates to quorums
            GateList = circuit.TopologicalOrder;
            GateQuorumMapping = gateQuorumMapping;

            Circuit = circuit;
            CircuitInputs = circuitInputs;
            Prime = prime;
            Result = new Dictionary<OutputGateAddress, T>();
            MyQuorum = myQuorum;
            ProtocolFactory = protocolFactory;
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg.Type == MsgType.SubProtocolCompleted);
            var completedMsg = (SubProtocolCompletedMsg)msg;
            // all my gates finished! collect all of the results
            var outputList = completedMsg.Result;
            foreach (var output in completedMsg.Result)
            {
                var gateOutputs = (IDictionary<OutputGateAddress, T>)output.Value;

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

                var evalQuorum = GateQuorumMapping[GateList[i]];
                if (evalQuorum == MyQuorum)
                {
                    ulong evalProtocolId = ProtocolIdGenerator.GateEvalIdentifier(i);
                    evalProtocols.Add(new MultiQuorumGateEvaluation<T>(Me, (ComputationGate)GateList[i], GateQuorumMapping, Circuit, ProtocolFactory, CircuitInputs, Prime, evalProtocolId));
                }
            }

            if (evalProtocols.Count > 0)
                ExecuteSubProtocols(evalProtocols);
            else
                IsCompleted = true;
        }
    }

    public class MultiQuorumGateEvaluation<T> : MultiQuorumProtocol<IDictionary<OutputGateAddress, T>> where T : class
    {
        private BigInteger Prime;

        private ComputationGate EvalGate;
        private IDictionary<Gate, Quorum> GateQuorumMapping;
        private Circuit Circuit;
        private IGateEvaluationProtocolFactory<T> ProtocolFactory;


        private IDictionary<InputGateAddress, T> CircuitInputs;
        private T[] InputShares;
        private int SharesReceived;
        private Dictionary<int, ulong> inputIdProtocolIdMap;

        private T[] OutputShares;

        private Dictionary<ulong, InputGateAddress> OutputLoopbacksNeeded;

        private Quorum EvalQuorum;

        private int Stage;

        public MultiQuorumGateEvaluation(Party me, ComputationGate evalGate, IDictionary<Gate, Quorum> gateQuorumMapping, Circuit circuit,
            IGateEvaluationProtocolFactory<T> protocolFactory, IDictionary<InputGateAddress, T> circuitInputs, BigInteger prime, ulong protocolId)
            : base(me, GetParticipatingQuorumList(evalGate, gateQuorumMapping, circuit), protocolId)
        {
            EvalGate = evalGate;
            GateQuorumMapping = gateQuorumMapping;
            Circuit = circuit;
            Prime = prime;
            EvalQuorum = gateQuorumMapping[EvalGate];
            CircuitInputs = circuitInputs;
            ProtocolFactory = protocolFactory;
            Result = new Dictionary<OutputGateAddress, T>();
        }

        private static Quorum[] GetParticipatingQuorumList(Gate evalGate, IDictionary<Gate, Quorum> gateQuorumMapping, Circuit circuit)
        {
            List<Quorum> quorums = new List<Quorum>();

            // first add where we're receiving from
            for (int i = 0; i < evalGate.InputCount; i++)
            {
                if (circuit.InputConnectionCounterparties.ContainsKey(evalGate.GetLocalInputAddress(i)))
                {
                    var counterpartGate = circuit.InputConnectionCounterparties[evalGate.GetLocalInputAddress(i)].Gate;
                    quorums.Add(gateQuorumMapping[counterpartGate]);
                }
            }

            quorums.Add(gateQuorumMapping[evalGate]);

            // then add where we're sending to
            for (int i = 0; i < evalGate.OutputCount; i++)
            {
                if (circuit.OutputConnectionCounterparties.ContainsKey(evalGate.GetLocalOutputAddress(i)))
                {
                    var counterpartGate = circuit.OutputConnectionCounterparties[evalGate.GetLocalOutputAddress(i)].Gate;
                    quorums.Add(gateQuorumMapping[counterpartGate]);
                }
            }

            return quorums.ToArray();
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
                var completedMsg = (SubProtocolCompletedMsg)msg;

                CollectGateEvaluation(completedMsg);
                SendShareStage();
            }
            else if (Stage == 2)
            {
                // we loopback any shares that go to me in a different quorum.
                Debug.Assert(msg is SubProtocolCompletedMsg);
                SubProtocolCompletedMsg completedMsg = (SubProtocolCompletedMsg)msg;
                foreach (var loopback in OutputLoopbacksNeeded)
                {
                    ulong counterpartEvalId = ProtocolIdGenerator.GateEvalIdentifier(loopback.Value.Gate.TopologicalRank);
                    T loopbackVal = (T)completedMsg.Result[loopback.Key];

                    NetSimulator.Loopback(Me.Id, counterpartEvalId, new LoopbackMsg<T>(loopbackVal, loopback.Value.Port));
                }
                IsCompleted = true;
            }
        }


        private void ReceiveShareStage(Msg msg)
        {
            if (msg.Type == MsgType.SubProtocolCompleted)
            {
                var completedMsg = (SubProtocolCompletedMsg)msg;
                foreach (int key in inputIdProtocolIdMap.Keys)
                {
                    InputShares[key] = (T)completedMsg.Result[inputIdProtocolIdMap[key]];
                    SharesReceived++;
                }
            }
            else
            {
                Debug.Assert(msg.Type == MsgType.CircuitLoopback);
                var loopbackMsg = (LoopbackMsg<T>)msg;
                InputShares[loopbackMsg.WhichInputForGate] = loopbackMsg.Value;
                SharesReceived++;
            }
        }

        private void StartGateEvaluation()
        {
            Stage = 1;
            Debug.Assert(InputShares.Length == EvalGate.InputCount);
            ExecuteSubProtocol(ProtocolFactory.GetEvaluationProtocolFor(EvalGate.Type, Me, EvalQuorum, InputShares));
        }

        private void CollectGateEvaluation(SubProtocolCompletedMsg msg)
        {
            OutputShares = (T[])msg.SingleResult;
            Debug.Assert(OutputShares.Length == EvalGate.OutputCount);
        }

        private void SendShareStage()
        {
            List<Protocol> reshareProtocols = new List<Protocol>();
            OutputLoopbacksNeeded = new Dictionary<ulong, InputGateAddress>();

            for (int i = 0; i < EvalGate.OutputCount; i++)
            {

                // figure out where this share is going to
                InputGateAddress counterpartGateAddr;
                if (Circuit.OutputConnectionCounterparties.TryGetValue(EvalGate.GetLocalOutputAddress(i), out counterpartGateAddr))
                {
                    var counterpartGate = counterpartGateAddr.Gate;
                    var counterpartQuorum = GateQuorumMapping[counterpartGate];

                    bool needReshare;
                    bool needLoopback;

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

                    if (needLoopback && !needReshare)
                    {
                        // loop a message back to the other protocol
                        ulong counterpartEvalId = ProtocolIdGenerator.GateEvalIdentifier(counterpartGate.TopologicalRank);
                        NetSimulator.Loopback(Me.Id, counterpartEvalId, new LoopbackMsg<T>(OutputShares[i], counterpartGateAddr.Port));
                    }

                    if (needReshare)
                    {
                        Protocol reshareProtocol = ProtocolFactory.GetResharingProtocol(Me, EvalQuorum, counterpartQuorum, OutputShares[i],
                            counterpartGate.TopologicalRank, counterpartGateAddr.Port);
                        reshareProtocols.Add(reshareProtocol);
                        if (needLoopback)
                        {
                            OutputLoopbacksNeeded[reshareProtocol.ProtocolId] = counterpartGateAddr;
                        }
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
            InputShares = new T[EvalGate.InputCount];

            List<Protocol> receiveSubProtocols = new List<Protocol>();
            inputIdProtocolIdMap = new Dictionary<int, ulong>();

            // start subprotocols to receive the input values
            for (int i = 0; i < EvalGate.InputCount; i++)
            {
                OutputGateAddress counterpartGateAddr;
                if (Circuit.InputConnectionCounterparties.TryGetValue(EvalGate.GetLocalInputAddress(i), out counterpartGateAddr))
                {
                    var counterpartGate = counterpartGateAddr.Gate;
                    var counterpartQuorum = GateQuorumMapping[counterpartGate];
                    if (!counterpartQuorum.HasMember(Me.Id))
                    {
                        // I expect to receive this value from a quorum resharing
                        Protocol reshareReceive = ProtocolFactory.GetResharingProtocol(Me, counterpartQuorum, EvalQuorum, null, EvalGate.TopologicalRank, i);
                        receiveSubProtocols.Add(reshareReceive);
                        inputIdProtocolIdMap[i] = reshareReceive.ProtocolId;
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

    class LoopbackMsg<T> : Msg
    {
        public readonly T Value;
        public readonly int WhichInputForGate;

        public LoopbackMsg(T value, int whichInputForGate)
            : base(MsgType.CircuitLoopback)
        {
            Value = value;
            WhichInputForGate = whichInputForGate;
        }

    }
}
