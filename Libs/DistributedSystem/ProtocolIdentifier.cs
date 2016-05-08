using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.DistributedSystem
{
    public static class ProtocolIdGenerator
    {

        public const ulong GENERIC_IDENTIFIER_PREFIX = ((ulong)0x00) << 56;
        public const ulong QUORUM_PROTOCOL_PREFIX = ((ulong)0x01) << 56;
        public const ulong GATE_INPUT_SHARING_PREFIX = ((ulong)0x02) << 56;
        public const ulong GATE_EVAL_PREFIX = ((ulong)0x03) << 56;
        public const ulong RESULT_BROADCAST_PREFIX = ((ulong)0x04) << 56;
        public const ulong TWO_QUORUM_PROTOCOL_PREFIX = ((ulong)0x05) << 56;
        public const ulong NOP_PROTOCOL_MODIFIER = ((ulong)0x01) << 48;
        public static ulong GenericIdentifier(int id)
        {
            return GENERIC_IDENTIFIER_PREFIX | (ulong)id;
        }

        public static ulong QuorumProtocolIdentifier(ushort quorumNumber, uint intraQuorumProtocolNumber)
        {
            return QUORUM_PROTOCOL_PREFIX | ((ulong)quorumNumber) << 32 | intraQuorumProtocolNumber;
        }

        public static uint GetIntraQuorumProtocolNumber(ulong id)
        {
            return (uint)(id & 0xFFFFFFFF);
        }

        public static bool IsQuorumProtocolIdentifier(ulong id)
        {
            return (QUORUM_PROTOCOL_PREFIX & id) != 0;
        }

        public static ulong TwoQuorumProtocolIdentifier(ushort quorumNumberA, ushort quorumNumberB, ushort intraQuorumProtocolNumber)
        {
            ushort maxQuorum = Math.Max(quorumNumberA, quorumNumberB);
            ushort minQuorum = Math.Min(quorumNumberA, quorumNumberB);

            return TWO_QUORUM_PROTOCOL_PREFIX | ((ulong)maxQuorum) << 32 | ((ulong)minQuorum) << 16 | intraQuorumProtocolNumber; 
        }
        
        public static ulong GateInputSharingIdentifier(int gateNumber, int whichInput)
        {
            Debug.Assert(gateNumber <= 1 << 24);
            return GATE_INPUT_SHARING_PREFIX | ((ulong)(uint)gateNumber) << 32 | (uint)whichInput;
        }

        public static ulong GateEvalIdentifier(int gateNumber)
        {
            return GATE_EVAL_PREFIX | ((uint)gateNumber);
        }

        public static ulong ResultBroadcastIdentifier(int outputNumber)
        {
            return RESULT_BROADCAST_PREFIX | ((uint)outputNumber);
        }

        public static ulong NopIdentifier(ulong baseProtocolIdentifier)
        {
            return NOP_PROTOCOL_MODIFIER | baseProtocolIdentifier;
        }
    }
}