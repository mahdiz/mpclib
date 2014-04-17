using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using MpcLib.Common.StochasticUtils;
using MpcLib.SecretSharing;

namespace MpcLib.ByzantineAgreement
{
	using MpcLib.Common;
	using MpcLib.Common.FiniteField;
	using MpcLib.DistributedSystem;

	/// <summary>
	/// Implements a secure broadcast protocol based on Michael Rabin's Byzantine agreement.
	/// Using this protocol, all players can ensure that the sender has sent exactly
	/// the same message to every player in the network.
	/// </summary>
	/// <typeparam name="TData">Type of data to broadcast.</typeparam>
	public class SecureBroadcaster<TData> : Protocol
	{
		private int k;
		private int N;
		private int count;
		private BroadcastMessage<TData> temp;
		private readonly TData data;
		private readonly bool isDealer;
		private readonly int prime;
		public override ProtocolIds Id { get { return ProtocolIds.Rabin; } }

		private List<ShareMessage> dealerShares;
		private Dictionary<BroadcastMessage<TData>, int> collectedMessages = 
			new Dictionary<BroadcastMessage<TData>, int>();

		public SecureBroadcaster(Entity e, TData data, ReadOnlyCollection<int> playerIds, 
			bool isDealer, int prime, StateKey stateKey, int seed)
			: base(e, playerIds, stateKey)
		{
			this.data = data;
			this.prime = prime;
			this.isDealer = isDealer;
		}

		public override void Run()
		{
			if (isDealer)
			{
				// send my data (message) to all players
				Broadcast(new BroadcastMessage<TData>(BroadcastStage.DealerDataSend, data, 0));

				// secret share a random bit sequence with all players
				// each bit will be used in a round of lottery later.

				for (int round = 0; round < N; round++)
				{
					var polynomialDeg = NumParties % 4 == 0 ? (NumParties / 4 - 1) : (NumParties / 4);

					// TODO: uncomment for non-simulation use. (use a directive to disable in simulation use)
					// var shares = Shamir.Share(new Zp(prime, RandGen.Next(0, 2)), NumEntities, polynomialDeg);

					var randomSecret = new Zp(prime, StaticRandom.Next(0, 2));
					var shares = ShamirSharing.Share(randomSecret, NumParties, polynomialDeg);

					for (int j = 0; j < NumParties; j++)
						Send(Entity.Id, EntityIds[j], new ShareMessage(BroadcastStage.DealerLotteryShare, shares[j], round));		// TODO: Message delay needed. These messages must be received after DealerDataSend.
				}
			}
		}

		//public override void Receive(Message msg)
		//{
		//	var bcMsg = msg as BroadcastMessage<TData>;
		//	switch (bcMsg.Stage)
		//	{
		//		case BroadcastStage.DealerLotteryShare:
		//			var shares = DealerShareCollector.Collect(bcMsg as ShareMessage, N);

		//			if (shares != null)
		//			{
		//				dealerShares = shares;

		//				// dealer setup phase is done.
		//				// 1st step of Polling: send (message(i), k) to all.
		//				Send(EntityId, EntityIds, new BroadcastMessage<TData>(BroadcastStage.DataSend, data, k = 1));
		//			}
		//			break;

		//		case BroadcastStage.DataSend:

		//			// 2nd step of Polling
		//			if (collectedMessages.ContainsKey(bcMsg))
		//				collectedMessages[bcMsg] = 1;
		//			else
		//				collectedMessages[bcMsg]++;

		//			if (collectedMessages.Sum(m => m.Value) == 2 * NumEntities / 3 - 1)
		//			{
		//				var maxMsg = collectedMessages.ElementAt(0);
		//				foreach (var m in collectedMessages)
		//					if (m.Value > maxMsg.Value)
		//						maxMsg = m;
		//				temp = maxMsg.Key;
		//				count = maxMsg.Value;
		//			}

		//			// 1st step of Lottery: send the k-th share received from the dealer to all.
		//			Debug.Assert(dealerShares != null, "Dealer setup phase is not done.");
		//			Send(EntityId, EntityIds, dealerShares[k]);

		//			break;

		//		default:
		//			throw new Exception("Invalid broadcast message.");
		//	}
		//}

		private void Polling()
		{
		}

		//public static Sendable Read(int publisher, int prime)
		//{
		//    return recieveSecrets(publisher, prime);
		//}

		//public static void Publish(Sendable toSend, bool[] sendToPlayers)
		//{
		//    sendSercrets(toSend, sendToPlayers);
		//}

		//public static IList<Sendable> PublishAndRead(Sendable toSend, int prime, bool[] sendToPlayers)
		//{
		//    return shareSecrets(toSend, prime, sendToPlayers);
		//}
	}
}