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
		public readonly BigInteger Prime;
		public readonly int PolyDegree;
		protected PolyCommit PolyCommit;
		private static Object myLock = new Object();

		public eVSS(SyncParty p, IList<int> pIds, BigInteger prime, int polyDegree)
			: base(p, pIds)
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

		public List<BigZp> ShareVerify(BigZp secret, bool reciprocal)
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

			// broadcast the commitment
			IList<CommitMsg> recvCommits = null;
			if (reciprocal)
				recvCommits = BroadcastReceive(PartyIds, new CommitMsg(mg)).OrderBy(s => s.SenderId).ToList();
			else
				Broadcast(PartyIds, new CommitMsg(mg));

			// create share messages
			var shareMsgs = new ShareWitnessMsg<BigZp>[NumParties];
			for (int i = 0; i < NumParties; i++)
				shareMsgs[i] = new ShareWitnessMsg<BigZp>(shares[i], witnesses[i]);

			// send the i-th share message to the i-th party
			if (reciprocal)
			{
				var recvShares = SendReceive(PartyIds, shareMsgs).OrderBy(s => s.SenderId).ToList();
				Verify(recvCommits, recvShares);
				return (from s in recvShares select s.Share).ToList();
			}

			Send(PartyIds, shareMsgs);
			return null;
		}

		public void Verify(IList<CommitMsg> commits, IList<ShareWitnessMsg<BigZp>> shares)
		{
			var rankZp = new BigZp(Prime, GetRank(Party.Id, PartyIds));

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

		public void Verify(CommitMsg commit, ShareWitnessMsg<BigZp> share)
		{
			var rankZp = new BigZp(Prime, GetRank(Party.Id, PartyIds));

			lock (myLock)	// because NTL is not thread-safe
			{
				if (!PolyCommit.VerifyEval(commit.Commitment,
					rankZp, share.Share, share.Witness))
				{
					// broadcast an accusation against the i-th party.
					throw new NotImplementedException();
				}
			}
		}

		public BigZp Reconst(BigZp ui)
		{
			// send the share to every party
			var recvShares = SendReceive(new ShareMsg<BigZp>(ui))
				.OrderBy(s => s.SenderId).Select(s => s.Share).ToList();

			return BigShamirSharing.Recombine(recvShares, PolyDegree - 1, Prime);

			// *WARNING*: Not for the malicious case. If any point is not on
			// the interpolated polynomial, then must use Welch-Berlekamp
			// error recovery to fix errors. This is how it should be done:
			//
			//var xValues = new List<BigZp>();
			//for (int i = 1; i <= recvShares.Count; i++)
			//	xValues.Add(new BigZp(Prime, i));
			//
			//var fixedShares = WelchBerlekampDecoder.Decode(xValues, recvShares, PolyDegree, PolyDegree, Prime);
			//
			// interpolate again
			// return BigShamirSharing.Recombine(fixedShares, PolyDegree, Prime);
		}

		public static int GetRank(int myId, IList<int> ids)
		{
			// find my rank in sorted list of ids
			int rank = 1;
			foreach (var id in ids.OrderBy(p => p))
			{
				if (id == myId)
					break;
				rank++;
			}
			return rank;
		}
	}
}
