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

namespace MpcLib.SecretSharing.eVSS
{
	public delegate void ShareFinishHandler(IEnumerable<BigZp> shares);

	public enum Stage
	{
		Commit,
		Share
	}

	public class eVSS : SyncProtocol
	{
		public override ProtocolIds Id { get { return ProtocolIds.eVSS; } }
		protected readonly BigInteger Prime;
		protected readonly int PolyDegree;
		protected PolyCommit PolyCommit;
		private static object myLock = new object();

		public eVSS(Party e, ReadOnlyCollection<int> pIds, BigInteger prime, int polyDegree)
			: base(e, pIds)
		{
			Prime = prime;
			PolyCommit = null;
			PolyDegree = polyDegree;
		}

		/// <summary>
		/// Initializes eVSS parameters using a deterministic RNG. 
		/// *WARNING* NOT SECURE! For simulation-purposes only!
		/// </summary>
		public void Setup(int seed)
		{
			PolyCommit = new PolyCommit();

			lock (myLock)
				PolyCommit.Setup(PolyDegree, seed);
		}

		public List<BigZp> Share(BigZp secret)
		{
			Debug.Assert(PolyCommit != null, "eVSS is not initialized yet.");
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
			lock (myLock)	// because NTL is not thread-safe
			{
				mg = PolyCommit.Commit(
					coeffs.ToArray(), iz, ref witnesses, ref proof, false);
			}

			Debug.Assert(PolyCommit.VerifyEval(mg,
				new BigZp(Prime, Party.Id + 1), shares[Party.Id], witnesses[Party.Id]));

			// broadcast the commitment
			var recvCommits = BroadcastReceive(PartyIds, new CommitMsg(mg)).OrderBy(s => s.SenderId).ToList();

			// send the i-th share and witness to the i-th party
			var shareMsgs = new ShareMsg<BigZp>[NumParties];
			for (int i = 0; i < NumParties; i++)
				shareMsgs[i] = new ShareMsg<BigZp>(shares[i], witnesses[i]);

			var recvShares = SendReceive(PartyIds, shareMsgs).OrderBy(s => s.SenderId).ToList();
			Verify(recvCommits, recvShares);

			return (from s in recvShares select s.Share).ToList();
		}

		protected void Verify(IList<CommitMsg> commits, IList<ShareMsg<BigZp>> shares)
		{
			// find my rank in sorted list of ids
			int rank = 1;
			foreach (var id in PartyIds.OrderBy(e => e))
			{
				if (id == Party.Id)
					break;
				rank++;
			}
			var rankZp = new BigZp(Prime, rank);

			for (int i = 0; i < commits.Count; i++)
			{
				lock (myLock)	// because NTL is not thread-safe
				{
					if (!PolyCommit.VerifyEval(commits[i].Commitment,
						rankZp, shares[i].Share, shares[i].Witness))
					{
						// broadcast an accusation against the i-th party.
						throw new NotImplementedException();
					}
				}
			}
		}

		public void Reconstruct()
		{
			throw new NotImplementedException();
		}
	}
}
