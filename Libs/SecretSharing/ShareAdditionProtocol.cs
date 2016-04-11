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
    public class ShareAdditionProtocol : Protocol
    {
        private BigZp Input;
        private BigInteger Prime;
        private int Seed;
        private PolyCommit PolyCommit;
        private int PolyDegree;


        private BigZp CombinedShare;
        public BigZp Result { get; private set; }

        private int numSharesRecv;
        private bool scheduledReconst;
        private Dictionary<int, CommitMsg> commitsRecv = new Dictionary<int, CommitMsg>();
        private Dictionary<int, ShareWitnessMsg<BigZp>> sharesRecv = new Dictionary<int, ShareWitnessMsg<BigZp>>();
        private Dictionary<int, BigZp> reconstRecv = new Dictionary<int, BigZp>();

        private static readonly List<MsgType> MESSAGE_TYPES = new List<MsgType>() { MsgType.Share, MsgType.Commit, MsgType.Reconst, MsgType.NextRound };

        public ShareAdditionProtocol(Party me, IList<int> participants, BigZp input, BigInteger prime, int seed, PolyCommit polyCommit)
            : base(me, participants)
        {
            Input = input;
            Prime = prime;
            Seed = seed;
            PolyCommit = polyCommit;
            PolyDegree = (int)Math.Ceiling(NumParties / 3.0);
            CombinedShare = new BigZp(Prime);
        }

        public override bool CanHandleMessageType(MsgType type)
        {
            return MESSAGE_TYPES.Contains(type);
        }

        public override void Start()
        {
            Share(Input);
        }

        public override void HandleMessage(int fromId, Msg msg)
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
                        if (!PolyCommit.VerifyEval(commitsRecv[fromId].Commitment, new BigZp(Prime, Me.Id + 1),
                            sharesRecv[fromId].Share, sharesRecv[fromId].Witness))
                        {
                            // broadcast an accusation against the i-th party.
                            throw new NotImplementedException();
                        }

                        // add the share to the shares received from all parties
                        if (Result == null)
                            Result = new BigZp(Prime, 0);

                        CombinedShare += sharesRecv[fromId].Share;

                        if (++numSharesRecv >= Math.Ceiling(2.0 * NumParties / 3.0) && !scheduledReconst)
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

                    // broadcast output share for reconstruction
                    Me.Broadcast(new ShareMsg<BigZp>(CombinedShare, MsgType.Reconst));
                    break;

                case MsgType.Reconst:
                    // collect added shares from parties
                    reconstRecv[fromId] = (msg as ShareMsg<BigZp>).Share;

                    //if (reconstRecv.Count >= Math.Ceiling(2.0 * NumParties / 3.0))
                    if (reconstRecv.Count == NumParties)
                    {
                        // reconstruct the output
                        var orderedShares = reconstRecv.OrderBy(p => p.Key).Select(p => p.Value).ToList();
                        Result = BigShamirSharing.Recombine(orderedShares, PolyDegree - 1, Prime);

                        // Error-correction procedure
                        //var xValues = new List<BigZp>();
                        //for (int i = 1; i <= reconstRecv.Count; i++)
                        //    xValues.Add(new BigZp(Prime, i));

                        //var fixedShares = WelchBerlekampDecoder.Decode(xValues, reconstRecv, PolyDegree, PolyDegree, Prime);

                        //// interpolate again
                        //return BigShamirSharing.Recombine(fixedShares, PolyDegree, Prime);
                    }
                    break;
            }
        }

        public override bool IsCompleted()
        {
            return (Result != null);
        }

        public void Share(BigZp secret)
        {
            Debug.Assert(PolyCommit != null, "PolyCommit is not initialized yet.");
            Debug.Assert(Prime == secret.Prime);
            IList<BigZp> coeffs = null;

            // generate a random polynomial
            var shares = BigShamirSharing.Share(secret, NumParties, PolyDegree - 1, out coeffs);

            // generate evaluation points x = {1,...,n}
            var iz = new BigZp[NumParties];
            for (int i = 0; i < NumParties; i++)
                iz[i] = new BigZp(Prime, new BigInteger(i + 1));

            // calculate the commitment and witnesses
            byte[] proof = null;
            MG[] witnesses = null;

            MG mg = null;
            mg = PolyCommit.Commit(coeffs.ToArray(), iz, ref witnesses, ref proof, false);

            // broadcast the commitment
            var commitMsg = new CommitMsg(mg);
            Me.Broadcast(commitMsg);

            // create share messages
            var shareMsgs = new ShareWitnessMsg<BigZp>[NumParties];
            for (int i = 0; i < NumParties; i++)
                shareMsgs[i] = new ShareWitnessMsg<BigZp>(shares[i], witnesses[i]);

            Debug.Assert(PolyCommit.VerifyEval(commitMsg.Commitment, new BigZp(Prime, 2), shareMsgs[1].Share, shareMsgs[1].Witness));
            Debug.Assert(BigShamirSharing.Recombine(shares, PolyDegree - 1, Prime) == secret);

            // send the i-th share message to the i-th party
            // the delay is to ensure my shares are distributed after my commits
            Me.Send(shareMsgs, 1);
        }
    }
}
