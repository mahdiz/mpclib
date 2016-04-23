using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpcLib.DistributedSystem;
using MpcLib.Common.FiniteField;
using System.Numerics;
using System.Diagnostics;
using MpcLib.Commitments.PolyCommitment;
using MpcLib.SecretSharing.eVSS;

namespace MpcLib.SecretSharing
{
    public class RandomGenProtocol : QuorumProtocol<BigZp>
    {
        private BigZp MyRandom;
        private BigInteger Prime;
        private PolyCommit PolyCommit;
        private int PolyDegree;

        private BigZp CombinedShare;

        private int numSharesRecv;
        private bool scheduledReconst;
        private Dictionary<int, CommitMsg> commitsRecv = new Dictionary<int, CommitMsg>();
        private Dictionary<int, ShareWitnessMsg<BigZp>> sharesRecv = new Dictionary<int, ShareWitnessMsg<BigZp>>();
        private Dictionary<int, BigZp> reconstRecv = new Dictionary<int, BigZp>();

        private static readonly List<MsgType> MESSAGE_TYPES = new List<MsgType>() { MsgType.Share, MsgType.Commit, MsgType.Reconst, MsgType.NextRound };

        public RandomGenProtocol(Party me, Quorum quorum, BigZp myRandom, BigInteger prime)
            : base(me, quorum)
        {
            MyRandom = myRandom;
            Prime = prime;
            PolyDegree = (int)Math.Ceiling(Quorum.Size / 3.0) - 1;

            if (Quorum is ByzantineQuorum)
                PolyCommit = (Quorum as ByzantineQuorum).PolyCommit;

            CombinedShare = new BigZp(Prime);
        }

        public override void Start()
        {
            Share(MyRandom);
        }

        protected override void HandleMessage(int fromId, Msg msg)
        {
            switch (msg.Type)
            {
                case MsgType.Commit:
                    // collect commitments from parties
                    commitsRecv[fromId] = msg as CommitMsg;
                    break;

                case MsgType.Share:
                    // collect shares from parties
                    sharesRecv[fromId] = msg as ShareWitnessMsg<BigZp>;

                    if (commitsRecv.ContainsKey(fromId))
                    {
                        // verify the share
                        if (PolyCommit != null && !PolyCommit.VerifyEval(commitsRecv[fromId].Commitment, new BigZp(Prime, Me.Id + 1),
                            sharesRecv[fromId].Share, sharesRecv[fromId].Witness))
                        {
                            // broadcast an accusation against the i-th party.
                            throw new NotImplementedException();
                        }

                        // add the share to the shares received from all parties
                        if (Result == null)
                            Result = new BigZp(Prime, 0);

                        CombinedShare += sharesRecv[fromId].Share;

                        if (++numSharesRecv >= Math.Ceiling(2.0 * Quorum.Size / 3.0) && !scheduledReconst)
                        {
                            // send a loopback message to notify the end of this round. This is done to
                            // ensure we receive the inputs of "all" honest parties in this round and 
                            // bad parties cannot prevent us from receiving honest input by sending bad inputs sooner.
                            Me.Send(Me.Id, new Msg(MsgType.NextRound));
                            scheduledReconst = true;
                            Console.WriteLine("Party " + Me.Id + " scheduled reconst round.");
                        }
                    }
                    else Console.WriteLine("No commitment received for party " + fromId);
                    break;

                case MsgType.NextRound:
                    if (fromId != Me.Id)
                        Console.WriteLine("Invalid next round message received. Party " + fromId + " seems to be cheating!");
                    IsCompleted = true;
                    Result = CombinedShare;
                    break;
            }
        }
        
        public void Share(BigZp secret)
        {
            Debug.Assert(Prime == secret.Prime);
            IList<BigZp> coeffs = null;

            // generate a random polynomial
            var shares = BigShamirSharing.Share(secret, Quorum.Size, PolyDegree, out coeffs);

            MG[] witnesses = null;
            MG commitment = null;
            if (PolyCommit != null)
                commitment = BigShamirSharing.GenerateCommitment(Quorum.Size, coeffs.ToArray(), Prime, ref witnesses, PolyCommit);
            else
                witnesses = new MG[Quorum.Size];
/*

            // generate evaluation points x = {1,...,n}
            var iz = new BigZp[Quorum.Size];
            for (int i = 0; i < Quorum.Size; i++)
                iz[i] = new BigZp(Prime, new BigInteger(i + 1));

            // calculate the commitment and witnesses
            byte[] proof = null;
            MG[] witnesses = null;

            MG mg = null;
            mg = PolyCommit.Commit(coeffs.ToArray(), iz, ref witnesses, ref proof, false);

            // broadcast the commitment
            var commitMsg = new CommitMsg(mg);
            */
            QuorumBroadcast(new CommitMsg(commitment));

            // create share messages
            var shareMsgs = new ShareWitnessMsg<BigZp>[Quorum.Size];
            for (int i = 0; i < Quorum.Size; i++)
                shareMsgs[i] = new ShareWitnessMsg<BigZp>(shares[i], witnesses[i]);

            if (PolyCommit != null)
                Debug.Assert(PolyCommit.VerifyEval(commitment, new BigZp(Prime, 2), shareMsgs[1].Share, shareMsgs[1].Witness));
            Debug.Assert(BigShamirSharing.Recombine(shares, PolyDegree, Prime) == secret);

            // send the i-th share message to the i-th party
            QuorumSend(shareMsgs);
        }
    }
}
