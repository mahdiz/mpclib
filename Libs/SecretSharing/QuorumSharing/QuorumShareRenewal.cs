using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using MpcLib.Commitments.PolyCommitment;
using MpcLib.Common;
using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem;
using MpcLib.SecretSharing.eVSS;

namespace MpcLib.SecretSharing.QuorumShareRenewal //what?
{
    public class QuorumShareRenewal : Protocol
    {
        private PolyCommit PolyCommit;


        public QuorumShareRenewal(Party me, IList<int> participants, BigZp share, BigInteger prime, PolyCommit polyCommit)
            : base(me, participants)
        {
            PolyCommit = polyCommit;
        }

        public override bool CanHandleMessageType(MsgType type)
        {
            throw new NotImplementedException();
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            throw new NotImplementedException();
        }

        public override bool IsCompleted()
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }
    }

    class ShareRenewalRound : Protocol
    {
        private PolyCommit PolyCommit;
        private IList<int> QuorumFrom;
        private IList<int> QuorumTo;
        private BigZp[] StartShares;
        private int StartSharesPerParty;
        private int FinalSharesPerParty;
        private int OldPolyDeg;
        private int NewPolyDeg;
        private int FinalShareCount;
        private int StartShareCount;
        private BigInteger Prime;
        private BigZp[] VandermondeInv;

        private Dictionary<int, int> numCommitsRecv = new Dictionary<int, int>();
        private Dictionary<int, CommitMsg> commitsRecv = new Dictionary<int, CommitMsg>();
        private Dictionary<int, List<ShareWitnessMsg<BigZp>>[]> sharesRecv = new Dictionary<int, List<ShareWitnessMsg<BigZp>>[]>();


        private BigZp[] FinalShares;

        public ShareRenewalRound(Party me, IList<int> participants, IList<int> quorumFrom, IList<int> quorumTo, BigZp[] startShares, BigInteger prime, int startSharesPerParty, int finalSharesPerParty, PolyCommit polyCommit)
            : base(me, participants)
        {
            PolyCommit = polyCommit;
            QuorumFrom = quorumFrom;
            QuorumTo = quorumTo;
            FinalSharesPerParty = finalSharesPerParty;
            StartSharesPerParty = startSharesPerParty;
            StartShares = startShares;
            Prime = prime;

            foreach (var from in QuorumFrom)
            {
                sharesRecv[from] = new List<ShareWitnessMsg<BigZp>>[startSharesPerParty];
                for (int i = 0; i < startSharesPerParty; i++)
                {
                    numCommitsRecv[from] = 0;
                    sharesRecv[from][i] = new List<ShareWitnessMsg<BigZp>>();
                }
            }

            StartShareCount = QuorumFrom.Count * StartSharesPerParty;
            FinalShareCount = QuorumTo.Count * FinalSharesPerParty;
            NewPolyDeg = FinalShareCount / 3;

            VandermondeInv = BigZpMatrix.GetVandermondeMatrix(StartShareCount, StartShareCount, Prime).Inverse.GetMatrixColumn(0);
        }

        public override bool CanHandleMessageType(MsgType type)
        {
            throw new NotImplementedException();
        }

        public override void HandleMessage(int fromId, Msg msg)
        {
            switch (msg.Type)
            {
                case MsgType.Commit:
                    commitsRecv[fromId] = msg as CommitMsg;
                    numCommitsRecv[fromId]++;
                    Debug.Assert(numCommitsRecv[fromId] <= StartSharesPerParty);
                    break;

                case MsgType.Share:
                    if (commitsRecv.ContainsKey(fromId))
                    {
                        int whichOrigShare = numCommitsRecv[fromId] - 1;
                        int whichFinalFromOrigShare = sharesRecv[fromId][whichOrigShare].Count;
                        int evalPoint = QuorumTo.IndexOf(Me.Id) * FinalSharesPerParty + whichOrigShare + 1;
                        var swMessage = msg as ShareWitnessMsg<BigZp>;
                        sharesRecv[fromId][numCommitsRecv[fromId] - 1].Add(swMessage);
                        if (!PolyCommit.VerifyEval(commitsRecv[fromId].Commitment, new BigZp(Prime, evalPoint), swMessage.Share, swMessage.Witness))
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else Console.WriteLine("No commitment from " + fromId);

                    // when to send myself the next round message?
                    break;
                case MsgType.NextRound:
                    Recombine();
                    break;
            }

    
        }

        public override bool IsCompleted()
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        private void Reshare(BigZp secret)
        {
            Debug.Assert(PolyCommit != null);
            Debug.Assert(Prime == secret.Prime);

            IList<BigZp> coeffs = null;

            var shares = BigShamirSharing.Share(secret, FinalShareCount, NewPolyDeg - 1, out coeffs); // why -1?
            MG[] witnesses = null;
            MG commitment = BigShamirSharing.GenerateCommitment(FinalShareCount, coeffs.ToArray(), Prime, ref witnesses, PolyCommit);

            Me.Multicast(QuorumTo, new CommitMsg(commitment));

            // create the share messages
            Debug.Assert(PolyCommit.VerifyEval(commitment, new BigZp(Prime, 2), shares[1], witnesses[1]));
            Debug.Assert(BigShamirSharing.Recombine(shares, NewPolyDeg - 1, Prime) == secret);

            for (int i = 0; i < FinalShareCount; i++)
                Me.Send(i / FinalSharesPerParty, new ShareWitnessMsg<BigZp>(shares[i], witnesses[i]));

        }

        private void Recombine()
        {
            BigZp[,] orderedShares = new BigZp[FinalSharesPerParty,StartShareCount];

            int senderPos = 0;
            foreach (var sender in QuorumFrom)
            {
                var sharesFromSender = sharesRecv[sender];
                for (int i = 0; i < StartSharesPerParty; i++)
                {
                    var resharesForShare = sharesFromSender[i];
                    for (int j = 0; j < resharesForShare.Count; j++)
                    {
                        var msg = resharesForShare[j];
                        orderedShares[j, senderPos * StartSharesPerParty + i] = msg.Share;
                    }
                }
            }

            FinalShares = new BigZp[FinalSharesPerParty];

            for (int i = 0; i < FinalSharesPerParty; i++)
            {
                FinalShares[i] = new BigZp(Prime);
                for (int j = 0; j < StartShareCount; j++)
                {
                    FinalShares[i] += orderedShares[i,j] * VandermondeInv[j];
                }
            }

        }
    }

   

}


/*
namespace MpcLib.SecretSharing.QuorumShareRenewal //what?
{
    public class QuorumShareRenewal : SyncProtocol
    {
        public override ProtocolIds Id { get { return ProtocolIds.QuorumShareRenewal; } }


        private IList<int> quorumFrom, quorumTo;
        private int Prime;
	    public Zp Share { get; private set; }

	    public QuorumShareRenewal(SyncParty p, IList<int> partyIds, IList<int> quorumFrom, IList<int> quorumTo, int prime, Zp share)
	        : base(p, partyIds)
	    {
	        this.quorumFrom = quorumFrom;
	        this.quorumTo = quorumTo;
            this.Prime = prime;
            this.Share = share;
	    }

        public QuorumShareRenewal(SyncParty p, IList<int> partyIds, IList<int> quorumFrom, IList<int> quorumTo, int prime)
            : this(p, partyIds, quorumFrom, quorumTo, prime, null)
        {
        }


        public override void Run()
        {
            if (quorumFrom.Contains(Party.Id))
            {
                doSendQuorum(Share);
            }
            else
            {
                Share = doReceiveQuorum();
            }
        }

        public void doSendQuorum(Zp secret) {
	        int numIntermediateRounds;
	    
	        if (quorumFrom.Count * 3 >= quorumTo.Count) {
		        numIntermediateRounds = 0;
	        } else {
		        int maxSize = 3*quorumFrom.Count;
		        numIntermediateRounds = 0;
		        while (maxSize < quorumTo.Count) {
		            maxSize *= 3;
		            numIntermediateRounds++;
		        }
	        }

            var input = new Zp[1];
            input[0] = secret;

	        for (int i = 0; i < numIntermediateRounds; i++) {
		        input = performIntermediateRound(input);
	        }

	        performFinalRound(input);

	    }

	    public Zp doReceiveQuorum() {

            List<ShareMatrixMsg> reshares = new List<ShareMatrixMsg>();

            foreach (var from in quorumFrom)
            {
                reshares.Add(Receive<ShareMatrixMsg>());
            }

            reshares.Sort(new SenderComparer());


            // we should receive shares from every party in the quorum from, which are the first quorumFrom.Count people in the list

            // now we have to combine the reshares we just got.  We
            // want to sum 
            
            int newShareCount = reshares.Count;

	        var vandermondeInv = ZpMatrix.GetVandermondeMatrix(newShareCount, newShareCount, Prime).Inverse.GetMatrixColumn(0);

	        var newShare = new Zp(Prime);
	    
	        for (int i = 0; i < reshares.Count; i++) {
		        ZpMatrix recv = reshares[i].Matrix;
		        for (int j = 0; j < recv.RowCount; j++) {
                //    Console.WriteLine(recv.RowCount);
		            newShare += vandermondeInv[i*recv.RowCount + j] * recv[j,0];
		        }
	        }

	        return newShare;
	    }

	    private ZpMatrix generateReshares(Zp[] oldShares, int newShareCount) {
            IList<Zp> coeffs;
	        var reshares = new ZpMatrix(oldShares.Length, newShareCount, Prime);

	        for (int i = 0; i < oldShares.Length; i++) {
		        IList<Zp> reshare = ShamirSharing.Share(oldShares[i], newShareCount, newShareCount/3, out coeffs);
                
                for (int j = 0; j < newShareCount; j++) {
		            reshares[i, j] = reshare[j];
		        }
                
	        }

            return reshares;

	    }

	    public Zp[] performIntermediateRound(Zp[] oldShares)
        {
            Console.WriteLine("intermediate");
	        // first thing we want to do is create a new polynomial
	        // that's 3x the degree of our existing polynomial for
	        // each of the shares we have

	        var reshares = generateReshares(oldShares, 3*quorumFrom.Count*oldShares.Length);
	    
	        // now we have shares for the new polynomials, we have to
	        // distrubte them

	        ZpMatrix distributionMatrix;
	        int sharesPerParty = 3*oldShares.Length;
	        IList<ShareMatrixMsg> sendMessages = new List<ShareMatrixMsg>();
	        for (int i = 0; i < quorumFrom.Count; i++) {
		    distributionMatrix =
		        reshares.GetSubMatrix(0, oldShares.Length - 1,
					      i * sharesPerParty, (i+1) * sharesPerParty - 1);
		    sendMessages.Add(new ShareMatrixMsg(distributionMatrix));
	        }

	        IList<ShareMatrixMsg> recvShares = SendReceive(quorumFrom, sendMessages);

	        int newShareCount = 3*oldShares.Length*quorumFrom.Count;

	        // now we have to combine the reshares we just got.  We
	        // want to sum the first columns of all of the matricies,
	        // the second columns of all of the matricies, etc.

	        var vandermondeInv = ZpMatrix.GetVandermondeMatrix(newShareCount, newShareCount, Prime).Inverse.GetMatrixColumn(0);

	        var newShares = new Zp[3*oldShares.Length];

	        for (int i = 0; i < recvShares.Count; i++) {
		        ZpMatrix recv = recvShares[i].Matrix;

		        for (int j = 0; j < recv.ColCount; j++) {
		            for (int k = 0; k < recv.RowCount; k++) {
			        newShares[j] += vandermondeInv[i*recv.RowCount + k] * recv[j, k];
		            }
		        }
	        }

	        return newShares;
	    }

	    public void performFinalRound(Zp[] oldShares)
        {
            var reshares = generateReshares(oldShares, quorumTo.Count);
            
	        ZpMatrix distributionMatrix;
	        IList<ShareMatrixMsg> sendMessages = new List<ShareMatrixMsg>();

            var quorumToSorted = new List<int>(quorumTo);
            quorumToSorted.Sort();

            for (int i = 0; i < quorumTo.Count; i++) {
                distributionMatrix = 
                    reshares.GetSubMatrix(0, oldShares.Length - 1, i, i);
                
                Send(quorumToSorted[i], new ShareMatrixMsg(distributionMatrix));
	        }
            
	    }

	    private int getPolynomialDegree(int quorumSize) {
	        return quorumSize / 3;
	    }
	
    }

    public class ShareMatrixMsg : Msg
    {
	    public readonly ZpMatrix Matrix;

	    public ShareMatrixMsg(ZpMatrix matrix)
	    {
	        Matrix = matrix;
	    }

    public override int StageKey
    {
        get
        {
            return 0;
        }
    }

    public override string ToString()
	    {
	        return base.ToString() + ", Shares=" + Matrix.ToString();
	    }
    }
}

*/