
namespace MpcLib.DistributedSystem //what?
{
    public class QuorumShareRenewal : SyncProtocol
    {

	private IList<int> quorumFrom, quorumTo;
	
	public ShareRenewal(SyncParty p, IList<int> partyIds, IList<int> quorumFrom, IList<int> quorumTo, BigInteger prime)
	    : base(p, partyIds), 
	{
	    this.quorumFrom = quorumFrom;
	    this.quorumTo = quorumTo;

	    
	}

	public void doSendQuorum(Zp secret) {
	    int numIntermediateRounds;
	    
	    if (quorumFrom.Count * 3 >= quorumTo.Count) {
		numIntermediateRounds = 0;
	    } else {
		int maxSize = 3*quorumFrom.Count;
		numIntermediateRounds = 0;
		while (maxSize < quorumToCount) {
		    maxSize *= 3;
		    numIntermediateRounds++;
		}
	    }

	    var input = new List<Zp>(secret);

	    for (int i = 0; i < numIntermediateRounds; i++) {
		input = performIntermediateRound(input);
	    }

	    performFinalRound(input);

	}

	public Zp doReceiveQuorum() {

	    IList<ShareMatrixMsg> reshares = Receive(fromQuorum);

	    // we should receive shares from every party in the quorum from, which are the first quorumFrom.Count people in the list

	    // now we have to combine the reshares we just got.  We
	    // want to sum 

	    var vandermondeInv = ZpMatrix.GetVandermondeMatrix(newShareCount, newShareCount, prime).GetMatrixColumn(0);

	    var newShare = new Zp();
	    
	    for (int i = 0; i < recvShares.Count; i++) {
		ZpMatrix recv = recvShares[i].Matrix;
		for (int j = 0; k < recv.Col; k++) {
		    newShare += vandermondeInv[i*recv.RowCount + j] * recv[j][0];
		}
	    }

	    return newShare;
	}

	private ZpMatrix generateReshares(Zp[] oldShares, int newShareCount) {
	    IList<Zp> coeffs;
	    var reshares = new ZpMatrix(oldShares.Length, oldShares.Length*3, prime);

	    for (int i = 0; i < oldShares.Count; i++) {
		IList<Zp> reshare = ShamirSharing.Share(oldShares[i], 3*oldShares.Length*quorumFrom.Count, oldShares.Length * quorumFrom.Count, coeffs);
		for (int j = 0; j < 3*oldShares.Count; j++) {
		    reshares[i][j] = reshare[j];
		}
	    }
	}

	public Zp[] performIntermediateRound(Zp[] oldShares) {
	    // first thing we want to do is create a new polynomial
	    // that's 3x the degree of our existing polynomial for
	    // each of the shares we have

	    ZpMatrix reshares = generateReshares(oldShares, 3*oldShares.Count);
	    
	    // now we have shares for the new polynomials, we have to
	    // distrubte them

	    ZpMatrix distributionMatrix;
	    int sharesPerParty = 3*oldShares.Length;
	    IList<ShareMatrixMsg> sendMessages = new List<ShareMatrixMsg>();
	    for (int i = 0; i < quorumFrom.Count; i++) {
		distributionMatrix =
		    reshares.GetSubmatrix(0, oldShares.Length - 1,
					  i * sharesPerParty, (i+1) * sharesPerParty - 1);
		sendMessages.Add(new ShareMatrixMsg(distributionMatrix));
	    }

	    IList<ShareMatrixMsg> recvShares = SendReceive(fromQuorum, sendMessages);

	    int newShareCount = 3*oldShares.Length*quorumFrom.Count;

	    // now we have to combine the reshares we just got.  We
	    // want to sum the first columns of all of the matricies,
	    // the second columns of all of the matricies, etc.

	    var vandermondeInv = ZpMatrix.GetVandermondeMatrix(newShareCount, newShareCount, prime).GetMatrixColumn(0);

	    var newShares = new Zp[3*oldShares.Length];

	    for (int i = 0; i < recvShares.Count; i++) {
		ZpMatrix recv = recvShares[i].Matrix;

		for (int j = 0; j < recv.ColCount; j++) {
		    for (int k = 0; k < recv.RowCount; k++) {
			newShares[j] += vandermondeInv[i*recv.RowCount + k] * recv[j][k];
		    }
		}
	    }

	    return newShares;
	}

	public void performFinalRound(Zp oldShares) {
	    IList<Zp> coeffs;
	    var reshares = new ZpMatrix(oldShares.Length, quorumTo.Count, prime);

	    ZpMatrix distributionMatrix;
	    IList<ShareMatrixMsg> sendMessages = new List<ShareMatrixMsg>();

	    for (int i = 0; i < quorumTo.Count; i++) {
		distributionMatrix = 
		    reshares.GetSubmatrix(0, oldShares.Length - 1, i, i);

		sendMessages.Add(new ShareMatrixMsg(distributionMatrix[i]));
	    }

	    IList<ShareMatrixMsg> emptyShares = SendReceive(toQuorum, sendMessages);
	}

	private int getPolynomialDegree(int quorumSize) {
	    return quorumSize / 3;
	}
	
    }

    public class ShareMatrixMsg<T> : Msg where T : ISizable
    {
	public readonly ZpMatrix Matrix;

	public ShareMatrixMsg(ZpMatrix matrix)
	{
	    Matrix = matrix;
	}

	public override string ToString()
	{
	    return base.ToString() + ", Shares=" + Shares.ToString();
	}
    }

}

public ZpMatrix GetSubmatrix(int r0, int r1, int c0, int c1)
{
    var x = new ZpMatrix(r1 - r0 + 1, c1 - c0, Prime);
    var B = x.data;

    for (int i = r0; i <= r1; i++) {
	for (int j = c0; j <= c1; j++) {
	    B[i - r0][j - c0] = data[i][j];
	}
    }
}
