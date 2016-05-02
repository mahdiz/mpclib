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
        public const ulong NOP_PROTOCOL_PREFIX = ((ulong)0x01) << 48;
        public static ulong GenericIdentifier(int id)
        {
            return GENERIC_IDENTIFIER_PREFIX | (ulong)id;
        }

        public static ulong QuorumProtocolIdentifier(short quorumNumber, int intraQuorumProtocolNumber)
        {
            return QUORUM_PROTOCOL_PREFIX | ((ulong)(ushort)quorumNumber) << 16 | (uint)intraQuorumProtocolNumber;
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
            return NOP_PROTOCOL_PREFIX | baseProtocolIdentifier;
        }
    }
}
