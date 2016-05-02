using MpcLib.Commitments.PolyCommitment;
using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem;
using MpcLib.SecretSharing.eVSS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.SecretSharing
{
    public class SharingProtocol : Protocol<Share<BigZp>>
    {
        BigInteger Prime;
        BigZp Secret;
        int SrcParty;
        Quorum DstQuorum;

        PolyCommit PolyCommit;
        MG Commitment;
        Share<BigZp> RecvShare;
        int PolyDegree;

        public SharingProtocol(Party me, int srcParty, Quorum dstQuorum, BigZp secret, BigInteger prime, ulong protocolId)
            : base(me, QuorumPlusParty(dstQuorum, srcParty), protocolId)
        {
            Prime = prime;
            Secret = secret;

            SrcParty = srcParty;
            DstQuorum = dstQuorum;

            if (dstQuorum is ByzantineQuorum)
                PolyCommit = ((ByzantineQuorum)dstQuorum).PolyCommit;
            PolyDegree = (int)Math.Ceiling(DstQuorum.Size / 3.0) - 1;
        }

        private static SortedSet<int> QuorumPlusParty(Quorum q, int p)
        {
            if (q.HasMember(p))
                return q.Members;

            var newSet = new SortedSet<int>(q.Members);
            newSet.Add(p);
            return newSet;
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            switch (msg.Type)
            {
                case MsgType.Commit:
                    // collect commitments from parties
                    Commitment = ((CommitMsg)msg).Commitment;
                    break;

                case MsgType.Share:
                    // collect shares from parties

                    var swMsg = msg as ShareWitnessMsg<BigZp>;
                    if (PolyCommit != null && Commitment != null && 
                        !PolyCommit.VerifyEval(Commitment, new BigZp(Prime, DstQuorum.GetPositionOf(Me.Id) + 1), swMsg.Share, swMsg.Witness))
                    {
                        // broadcast an accusation against the i-th party.
                        throw new NotImplementedException();
                    }
                    else if (PolyCommit != null && Commitment == null)
                    {
                        Console.Write("No commitment received from" + fromId);
                    }
                    
                    IsCompleted = true;
                    Result = new Share<BigZp>(swMsg.Share, false);
                    break;
            }
        }

        public override void Start()
        {
            if (Me.Id == SrcParty)
            {
                Debug.Assert(Secret != null);
                Share(Secret);
            }

            if (!DstQuorum.HasMember(Me.Id))
            {
                IsCompleted = true;
                Result = null;
            }
        }

        public void Share(BigZp secret)
        {
            Debug.Assert(Prime == secret.Prime);
            IList<BigZp> coeffs = null;

            int polyDegree = (int)Math.Ceiling(DstQuorum.Size / 3.0) - 1;

            // generate a random polynomial
            var shares = BigShamirSharing.Share(secret, DstQuorum.Size, PolyDegree, out coeffs);

            MG[] witnesses = null;
            MG commitment = null;
            if (PolyCommit != null)
                commitment = BigShamirSharing.GenerateCommitment(DstQuorum.Size, coeffs.ToArray(), Prime, ref witnesses, PolyCommit);
            else
                witnesses = new MG[DstQuorum.Size];

            Multicast(new CommitMsg(commitment), DstQuorum.Members);

            // create share messages
            var shareMsgs = new ShareWitnessMsg<BigZp>[DstQuorum.Size];
            for (int i = 0; i < DstQuorum.Size; i++)
                shareMsgs[i] = new ShareWitnessMsg<BigZp>(shares[i], witnesses[i]);

            if (PolyCommit != null)
                Debug.Assert(PolyCommit.VerifyEval(commitment, new BigZp(Prime, 2), shareMsgs[1].Share, shareMsgs[1].Witness));
            Debug.Assert(BigShamirSharing.Recombine(shares, PolyDegree, Prime) == secret);

            // send the i-th share message to the i-th party
            Send(shareMsgs, DstQuorum.Members);
        }
    }
}
