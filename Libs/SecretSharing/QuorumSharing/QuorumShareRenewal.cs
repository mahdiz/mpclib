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

