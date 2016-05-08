using MpcLib.BasicProtocols;
using MpcLib.Circuits;
using MpcLib.Common.FiniteField;
using MpcLib.Common.StochasticUtils;
using MpcLib.DistributedSystem;
using MpcLib.SecretSharing;
using MpcLib.SecretSharing.QuorumShareRenewal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.MultiPartyShuffling
{
    public class TaggedBigZpShareGateEvaluationFactory : IGateEvaluationProtocolFactory<Tuple<Share<BigZp>, Share<BigZp>>>
    {
        private BigInteger Prime;

        public TaggedBigZpShareGateEvaluationFactory(BigInteger prime)
        {
            Prime = prime;
        }

        public QuorumProtocol<Tuple<Share<BigZp>, Share<BigZp>>[]> GetEvaluationProtocolFor(ComputationGateType type, Party me, Quorum quorum, Tuple<Share<BigZp>, Share<BigZp>>[] inputs)
        {       
            switch (type)
            {
                case ComputationGateType.COMPARE_AND_SWAP:
                    Debug.Assert(inputs.Length == 2);
                    return new TaggedCompareAndSwapProtocol(me, quorum, inputs[0], inputs[1]);
                default:
                    Debug.Assert(false, "should not get here");
                    return null;
            }
        }

        public MultiQuorumProtocol<Tuple<Share<BigZp>, Share<BigZp>>> GetResharingProtocol(Party me, Quorum fromQuorum, Quorum toQuorum, Tuple<Share<BigZp>, Share<BigZp>> value, int gateNumber, int portNumber)
        {
            return new QuorumTaggedShareRenewalProtocol(me, fromQuorum, toQuorum, value, Prime, ProtocolIdGenerator.GateInputSharingIdentifier(gateNumber, portNumber * 3));
        }
    }

    public class MultiPartyShufflingProtocol : Protocol<List<BigZp>>
    {
        public const int TRUSTED_PARTY_COUNT = 5;
        public static int EVALUATION_QUORUM_COUNT = 2;
        public static int EVALUATION_QUORUM_SIZE = 5;
        public const int POLY_COMMIT_SEED = 0;

        private const int RANDOM_DISTRIBUTION_PROTOCOL = 1;
        private const int CIRCUIT_EVAL_SYNC_PROTOCOL = 2;

        private Quorum RandGenQuorum;
        private BigInteger Prime;
        private BigZp Secret;
        private BigZp ProtocolRandom;
        private BigZp SortValue;

        private int NextQuorumNumber = 0;
        private Quorum[] EvalQuorums;
        private Quorum[] MyQuorums;

        private PermutationNetwork SortNetwork;
        private Dictionary<Gate, Quorum> GateQuorumMapping;

        private Dictionary<InputGateAddress, Tuple<ulong, ulong>> InputProtocolMapping;

        Dictionary<InputGateAddress, Tuple<Share<BigZp>, Share<BigZp>>> CircuitInputs;
        private Dictionary<Quorum, ulong> EvalProtocolMapping;
        private Dictionary<Quorum, IDictionary<OutputGateAddress, Tuple<Share<BigZp>, Share<BigZp>>>> CircuitResultsPerQuorum;

        private TaggedBigZpShareGateEvaluationFactory GateProtocolEvaluationFactory;

        private Dictionary<OutputGateAddress, ulong> ReconstructProtocolMapping;

        int Stage = 0;

        public MultiPartyShufflingProtocol(Party me, SortedSet<int> members, ulong protocolId, BigZp secret, BigInteger prime)
            : base(me, members, protocolId)
        {
            Prime = prime;
            Secret = secret;

            SortValue = new BigZp(prime, me.SafeRandGen.Next(prime));

            GateProtocolEvaluationFactory = new TaggedBigZpShareGateEvaluationFactory(Prime);

            SortNetwork = new LPSortingNetwork(members.Count);
            SortNetwork.CollapsePermutationGates();
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg.Type == MsgType.SubProtocolCompleted);

            var completedMsg = (SubProtocolCompletedMsg)msg;

            switch (Stage)
            {
                case 0:
                    // reconstruct the rand we just created
                    ExecuteSubProtocol(new ReconstructionProtocol(Me, RandGenQuorum, (Share<BigZp>)completedMsg.SingleResult));
                    break;
                case 1:
                    ProtocolRandom = (BigZp)completedMsg.SingleResult;
                    ExecuteSubProtocol(new MajorityFilteringProtocol<BigZp>(Me, PartyIds, PartyIds.Skip(TRUSTED_PARTY_COUNT).ToArray(),
                        ProtocolRandom, ProtocolIdGenerator.GenericIdentifier(1)));
                    break;
                case 2:
                    if (!RandGenQuorum.HasMember(Me.Id))
                        ProtocolRandom = (BigZp)completedMsg.SingleResult;

                    GenerateQuorums();
                    GenerateGateQuorumAssignment();

                    SetupInputDistribution();
                    break;
                case 3:
                    UnpackCircuitInputs(completedMsg.Result);
                    SetupQuorumExecutions();
                    break;
                case 4:
                    UnpackCircuitResults((completedMsg.SingleResult as SubProtocolCompletedMsg).Result);
                    SetupReconstruction();
                    break;
                case 5:
                    UnpackReconstruction(completedMsg.Result);
                    SetupResultBroadcast();
                    break;
                case 6:
                    CollectResults(completedMsg.Result);
                    IsCompleted = true;
                    break;
            }
            Stage++;
        }

        public override void Start()
        {
            // we start by doing a random generation in a group if you are one of the first 5 parties
            Stage = 0;
            SetupRandGenStep();
        }

        public void SetupRandGenStep()
        {
            Debug.Assert(PartyIds.Count > TRUSTED_PARTY_COUNT);

            RandGenQuorum = new Quorum(NextQuorumNumber++, PartyIds.Take(5).ToArray());
            if (RandGenQuorum.HasMember(Me.Id))
            {
                BigZp myRandom = new BigZp(Prime, Me.SafeRandGen.Next(Prime));
                ExecuteSubProtocol(new RandomGenProtocol(Me, RandGenQuorum, myRandom, Prime));
            }
            else
            {
                // receive the rand broadcast
                ExecuteSubProtocol(new MajorityFilteringProtocol<BigZp>(Me, PartyIds, RandGenQuorum.Members.ToList(), ProtocolIdGenerator.GenericIdentifier(1)));
                Stage = 2;
            }
        }

        public void GenerateQuorums()
        {
            var quorumMembers = QuorumGenerator.GenerateQuorums(PartyIds, EVALUATION_QUORUM_COUNT, EVALUATION_QUORUM_SIZE, ProtocolRandom.Value);

            EvalQuorums = quorumMembers.Select(memberSet => new Quorum(NextQuorumNumber++, memberSet)).ToArray();
            MyQuorums = EvalQuorums.Where(q => q.HasMember(Me.Id)).ToArray();
        }

        public void GenerateGateQuorumAssignment()
        {
            GateQuorumMapping = new Dictionary<Gate, Quorum>();
            List<Gate> gates = SortNetwork.Circuit.TopologicalOrder;
            for (int i = 0; i < gates.Count; i++)
            {
                GateQuorumMapping[gates[i]] = EvalQuorums[i % EVALUATION_QUORUM_COUNT];
            }
        }

        public void SetupInputDistribution()
        {
            int myPosition = Quorum.GetPositionOf(PartyIds, Me.Id);
            GateAddress myInputGateAddr = SortNetwork.FirstGateForWire[myPosition];
            Quorum myInputQuorum = GateQuorumMapping[myInputGateAddr.Gate];

            Protocol sortValueDistribution = new SharingProtocol(Me, Me.Id, myInputQuorum, SortValue, Prime,
                ProtocolIdGenerator.GateInputSharingIdentifier(myInputGateAddr.Gate.TopologicalRank, 2 * myInputGateAddr.Port));

            Protocol secretDistribution = new SharingProtocol(Me, Me.Id, myInputQuorum, Secret, Prime,
                ProtocolIdGenerator.GateInputSharingIdentifier(myInputGateAddr.Gate.TopologicalRank, 2 * myInputGateAddr.Port + 1));

            List<Protocol> inputProtocols = new List<Protocol>();

            inputProtocols.Add(sortValueDistribution);
            inputProtocols.Add(secretDistribution);

            InputProtocolMapping = new Dictionary<InputGateAddress, Tuple<ulong, ulong>>();

            for (int i = 0; i < PartyIds.Count; i++)
            {
                var gateAddr = SortNetwork.FirstGateForWire[i];
                foreach (Quorum q in MyQuorums)
                {
                    if (GateQuorumMapping[gateAddr.Gate] == q)
                    {
                        if (i == myPosition)
                        {
                            InputProtocolMapping[gateAddr] = new Tuple<ulong, ulong>(sortValueDistribution.ProtocolId, secretDistribution.ProtocolId);
                        }
                        else
                        {
                            ulong sortRecvId = ProtocolIdGenerator.GateInputSharingIdentifier(gateAddr.Gate.TopologicalRank, 2 * gateAddr.Port);
                            ulong shareRecvId = ProtocolIdGenerator.GateInputSharingIdentifier(gateAddr.Gate.TopologicalRank, 2 * gateAddr.Port + 1);
                            InputProtocolMapping[gateAddr] = new Tuple<ulong, ulong>(sortRecvId, shareRecvId);
                            // I need to receive for this gate
                            inputProtocols.Add(new SharingProtocol(Me, PartyIds.ElementAt(i), q, null, Prime, sortRecvId));
                            inputProtocols.Add(new SharingProtocol(Me, PartyIds.ElementAt(i), q, null, Prime, shareRecvId));
                        }
                    }
                }
            }

            ExecuteSubProtocols(inputProtocols);
        }

        private void UnpackCircuitInputs(IDictionary<ulong, object> circuitInputs)
        {
            CircuitInputs = new Dictionary<InputGateAddress, Tuple<Share<BigZp>, Share<BigZp>>>();
            foreach (var gateAddr in InputProtocolMapping.Keys)
            {
                var sortShare = (Share<BigZp>)circuitInputs[InputProtocolMapping[gateAddr].Item1];
                var secretShare = (Share<BigZp>)circuitInputs[InputProtocolMapping[gateAddr].Item2];
                CircuitInputs[gateAddr] = new Tuple<Share<BigZp>, Share<BigZp>>(sortShare, secretShare);
            }

            InputProtocolMapping = null;
        }

        private void SetupQuorumExecutions()
        {
            List<Protocol> executionProtocols = new List<Protocol>();
            EvalProtocolMapping = new Dictionary<Quorum, ulong>();
            foreach (Quorum q in MyQuorums)
            {
                ulong evalProtocolId = q.GetNextProtocolId();
                EvalProtocolMapping[q] = evalProtocolId;
                executionProtocols.Add(new SecureMultiQuorumCircuitEvaluation<Tuple<Share<BigZp>, Share<BigZp>>>(
                    Me, q, EvalQuorums, evalProtocolId, SortNetwork.Circuit, CircuitInputs, 
                    GateProtocolEvaluationFactory, GateQuorumMapping, Prime));
            }
            
            ExecuteSubProtocol(new SynchronizationProtocol(Me, PartyIds, executionProtocols, ProtocolIdGenerator.GenericIdentifier(CIRCUIT_EVAL_SYNC_PROTOCOL)));
        }

        private void UnpackCircuitResults(SortedDictionary<ulong, object> circuitResults)
        {
            Debug.Assert(MyQuorums.Length == circuitResults.Count);

            CircuitResultsPerQuorum = new Dictionary<Quorum, IDictionary<OutputGateAddress, Tuple<Share<BigZp>, Share<BigZp>>>>();

            foreach (var quorum in EvalProtocolMapping.Keys)
            {
                ulong protocolId = EvalProtocolMapping[quorum];
                CircuitResultsPerQuorum[quorum] = (IDictionary<OutputGateAddress, Tuple<Share<BigZp>, Share<BigZp>>>)circuitResults[protocolId];
            }
            EvalProtocolMapping = null;
        }

        private void SetupReconstruction()
        {
            ReconstructProtocolMapping = new Dictionary<OutputGateAddress, ulong>();
            List<Protocol> reconstructionProtocols = new List<Protocol>();
            foreach (var quorumResults in CircuitResultsPerQuorum)
            {
                foreach (var singleResult in quorumResults.Value)
                {
                    var protocol = new ReconstructionProtocol(Me, quorumResults.Key, singleResult.Value.Item2); // don't worry about reconstructing the sort shares
                    ReconstructProtocolMapping[singleResult.Key] = protocol.ProtocolId;
                    reconstructionProtocols.Add(protocol);
                }
            }

            CircuitResultsPerQuorum = null;

            ExecuteSubProtocols(reconstructionProtocols);
        }

        private void UnpackReconstruction(IDictionary<ulong, object> rawResults)
        {
            Result = new List<BigZp>(new BigZp[PartyIds.Count]);

            var reconstructResults = rawResults.ToDictionary(k => k.Key, v => (BigZp)v.Value);

            Dictionary<OutputGateAddress, int> outputGatePositions = new Dictionary<OutputGateAddress, int>();
            for (int i = 0; i < PartyIds.Count; i++)
            {
                OutputGateAddress outAddr = SortNetwork.LastGateForWire[i];
                if (ReconstructProtocolMapping.ContainsKey(outAddr))
                    Result[i] = reconstructResults[ReconstructProtocolMapping[outAddr]];
            }
        }

        private void SetupResultBroadcast()
        {
            // we need to broadcast each result to every other quorum
            List<Protocol> resultProtocols = new List<Protocol>();

            for (int i = 0; i < PartyIds.Count; i++)
            {
                Quorum evalQuorum = GateQuorumMapping[SortNetwork.LastGateForWire[i].Gate];
                if (Result[i] == null)
                {
                    // setup a receive for this result
                    resultProtocols.Add(new MajorityFilteringProtocol<BigZp>(Me, PartyIds, evalQuorum.Members,
                        ProtocolIdGenerator.ResultBroadcastIdentifier(i)));
                }
                else
                {
                    // setup a broadcast for this result
                    ISet<int> others = new SortedSet<int>(PartyIds);
                    foreach (var qid in evalQuorum.Members)
                        others.Remove(qid);

                    resultProtocols.Add(new MajorityFilteringProtocol<BigZp>(Me, PartyIds, others, Result[i],
                        ProtocolIdGenerator.ResultBroadcastIdentifier(i)));
                }
            }

            ExecuteSubProtocols(resultProtocols);
        }

        private void CollectResults(IDictionary<ulong, object> rawResults)
        {
            var allResults = rawResults.ToDictionary(k => k.Key, v => (BigZp)v.Value);

            for (int i = 0; i < PartyIds.Count; i++)
            {
                if (Result[i] == null)
                {
                    Result[i] = allResults[ProtocolIdGenerator.ResultBroadcastIdentifier(i)];
                }
            }
        }
    }
}

