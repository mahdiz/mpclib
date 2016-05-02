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
using MpcLib.Common.StochasticUtils;

namespace MpcLib.SecretSharing
{
    public class RandomGenProtocol : QuorumProtocol<Share<BigZp>>
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
                        if (PolyCommit != null && !PolyCommit.VerifyEval(commitsRecv[fromId].Commitment, new BigZp(Prime, Quorum.GetPositionOf(Me.Id) + 1),
                            sharesRecv[fromId].Share, sharesRecv[fromId].Witness))
                        {
                            // broadcast an accusation against the i-th party.
                            throw new NotImplementedException();
                        }

                        // add the share to the shares received from all parties

                        CombinedShare += sharesRecv[fromId].Share;

                        if (++numSharesRecv >= Math.Ceiling(2.0 * Quorum.Size / 3.0) && !scheduledReconst)
                        {
                            // send a loopback message to notify the end of this round. This is done to
                            // ensure we receive the inputs of "all" honest parties in this round and 
                            // bad parties cannot prevent us from receiving honest input by sending bad inputs sooner.
                            Send(Me.Id, new Msg(MsgType.NextRound));
                            scheduledReconst = true;
                        }
                    }
                    else Console.WriteLine("No commitment received for party " + fromId);
                    break;

                case MsgType.NextRound:
                    if (fromId != Me.Id)
                        Console.WriteLine("Invalid next round message received. Party " + fromId + " seems to be cheating!");
                    IsCompleted = true;
                    Result = new Share<BigZp>(CombinedShare, false);
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

    public class RandomBitGenProtocol : QuorumProtocol<Share<BigZp>>
    {
        private BigInteger Prime;

        private Share<BigZp> RandShare, Rand2Share;
        private BigZp Rand2, RandP;

        private int Stage;

        public RandomBitGenProtocol(Party me, Quorum quorum, BigInteger prime)
            : base(me, quorum)
        {
            Prime = prime;
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg.Type == MsgType.SubProtocolCompleted);

            var completedMsg = msg as SubProtocolCompletedMsg;
            switch (Stage)
            {
                case 0:
                    RandShare = (Share<BigZp>)completedMsg.SingleResult;
                    ExecuteSubProtocol(new ShareMultiplicationProtocol(Me, Quorum, RandShare, RandShare));
                    break;
                case 1:
                    Rand2Share = (Share<BigZp>)completedMsg.SingleResult;
                    ExecuteSubProtocol(new ReconstructionProtocol(Me, Quorum, Rand2Share));
                    break;
                case 2:
                    Rand2 = (BigZp)completedMsg.SingleResult;
                    if (Rand2.Value == 0)
                    {
                        // need to start over
                        Start();
                        return;
                    }   
                    else
                        FinalProcessing();
                    break;
            }

            Stage++;
        }

        public override void Start()
        {
            Stage = 0;
            BigZp myRandom = new BigZp(Prime, Me.SafeRandGen.Next(Prime));
            ExecuteSubProtocol(new RandomGenProtocol(Me, Quorum, myRandom, Prime));
        }

        private void FinalProcessing()
        {
            RandP = Rand2.SquareRoot;
            // get into the correct range
            if (RandP > Prime / 2)
                RandP = RandP.AdditiveInverse;

            Result = new Share<BigZp>(new BigZp(Prime, 2).MultipInverse * (RandP.MultipInverse * RandShare.Value + 1), false);
            IsCompleted = true;
        }
    }

    public class RandomBitwiseGenProtocol : QuorumProtocol<List<Share<BigZp>>>
    {
        private BigInteger Max;
        private BigInteger Prime;
        int BitsNeeded;

        private int Stage;

        public RandomBitwiseGenProtocol(Party me, Quorum quorum, BigInteger prime, BigInteger max)
            : base(me, quorum)
        {
            Max = max;
            Prime = prime;

            BitsNeeded = NumTheoryUtils.GetBitLength2(Max);
            if (Max.IsPowerOfTwo)
            {
                BitsNeeded--;
            }

            Result = new List<Share<BigZp>>();
        }

        public override void Start()
        {
            Stage = 0;

            List<Protocol> bitGens = new List<Protocol>();
            for (int i = 0; i < BitsNeeded; i++)
            {
                bitGens.Add(new RandomBitGenProtocol(Me, Quorum, Prime));
            }

            ExecuteSubProtocols(bitGens);
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            Debug.Assert(msg is SubProtocolCompletedMsg);

            SubProtocolCompletedMsg completedMsg = msg as SubProtocolCompletedMsg;

            switch (Stage)
            {
                case 0:
                    Result = completedMsg.ResultList.Cast<Share<BigZp>>().ToList();
                    if (Max.IsPowerOfTwo)
                    {
                        // we automatically know the number we generated is in the range
                        IsCompleted = true;
                    }
                    else
                    {
                        var maxBits = NumTheoryUtils.GetBitDecomposition(Max, Prime);
                        var maxBitsShares = new List<Share<BigZp>>();
                        foreach (var bit in maxBits)
                            maxBitsShares.Add(new Share<BigZp>(bit, true));
                        ExecuteSubProtocol(new BitwiseLessThanProtocol(Me, Quorum, Result, maxBitsShares));
                        Stage++;
                    }
                    break;
                case 1:
                    ExecuteSubProtocol(new ReconstructionProtocol(Me, Quorum, (Share<BigZp>)completedMsg.SingleResult));
                    Stage++;
                    break;
                case 2:
                    if (((BigZp)completedMsg.SingleResult).Value == 1)
                    {
                        // generation succeeded
                        IsCompleted = true;
                    }
                    else
                    {
                        // try again :(
                        if (Me.Id == 0) Console.WriteLine("RAND GEN FAILED " + ProtocolId);
                        Result.Clear();
                        Start();
                    }
                    break;
            }
        }
    }
}
