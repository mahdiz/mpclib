using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using MpcLib.Common;

namespace MpcLib.DistributedSystem
{
	///// <param name="key">A key used by the wrapper (i.e. outer) protocol to uniquely identify 
	///// the inner protocol (i.e. sender) instance in order to redirect the relpy message to the 
	///// appropriate inner instance.</param>
	//public delegate void SendHandler(int fromId, int toId, Msg msg);

	//public delegate void BroadcastHandler(int fromId, IEnumerable<int> toIds, Msg msg);

	/// <summary>
	/// Represents an abstract network protocol.
	/// </summary>
	public abstract class Protocol
	{
		/// <summary>
		/// Unique idetifier of the protocol type.
		/// </summary>
		public abstract ProtocolIds Id { get; }

		/// <summary>
		/// The entity associated with this protocol.
		/// </summary>
		protected readonly Entity Entity;

		/// <summary>
		/// Number of entities (parties) in the network.
		/// </summary>
		protected readonly int NumParties;
		protected readonly StateKey StateKey;
		protected readonly ReadOnlyCollection<int> EntityIds;

		private event SendHandler sendMsg;
		private event SendRecvHandler sendRecvMsg;
		private event BroadcastHandler broadcastMsg;
		private readonly Dictionary<int, IRelayEntry<Msg>> relayDic = new Dictionary<int, IRelayEntry<Msg>>();
		private Dictionary<ProtocolIds, Protocol> subProtocols = new Dictionary<ProtocolIds, Protocol>();

		public Protocol(Entity e, ReadOnlyCollection<int> entityIds, StateKey stateKey)
			: this(e, entityIds, e.Send, e.SendReceive, e.Broadcast, stateKey)
		{
		}

		public Protocol(Entity e, ReadOnlyCollection<int> entityIds,
			SendHandler send, StateKey stateKey)
			: this(e, entityIds, send, null, null, stateKey)
		{
		}

		public Protocol(Entity e, ReadOnlyCollection<int> entityIds,
			SendRecvHandler sendRecv, StateKey stateKey)
			: this(e, entityIds, null, sendRecv, null, stateKey)
		{
		}

		public Protocol(Entity e, ReadOnlyCollection<int> entityIds,
			SendHandler send, SendRecvHandler sendRecv, BroadcastHandler bcast, StateKey stateKey)
		{
			Entity = e;
			EntityIds = entityIds;
			NumParties = entityIds.Count;
			StateKey = stateKey;
			sendMsg += send;
			broadcastMsg += bcast;
			sendRecvMsg += sendRecv;
#if !SIMULATION
			RandGen = new CryptoRandom();
#endif
		}

		public abstract void Run();

		protected virtual void Send(int fromId, int toId, Msg msg)
		{
			Debug.Assert(sendMsg != null, "No send method have been set for the protocol!");
			msg.ProtocolId = Id;
			sendMsg(fromId, toId, msg);
		}

		/// <summary>
		/// Broadcasts a message to the specified set of entities.
		/// Currently does not support the malicious model (active adversary), 
		/// which requires Byzantine agreement to ensure consistency.
		/// </summary>
		protected void Broadcast(IEnumerable<int> toIds, Msg msg)
		{
			Debug.Assert(broadcastMsg != null, "No broadcast method has been set for the protocol!");
			msg.ProtocolId = Id;
			broadcastMsg(Entity.Id, toIds, msg);
		}

		/// <summary>
		/// Broadcasts a message.
		/// Currently does not support the malicious model, 
		/// which requires Byzantine agreement to ensure consistency.
		/// </summary>
		protected void Broadcast(Msg msg)
		{
			Broadcast(EntityIds, msg);
		}

		/// <summary>
		/// Sends the i-th message to the i-th entity.
		/// </summary>
		protected void Send(IEnumerable<int> toIds, IEnumerable<Msg> msgs)
		{
			Debug.Assert(sendMsg != null);
			Debug.Assert(toIds.Count() == msgs.Count());
			var e = msgs.GetEnumerator();

			foreach (var toId in toIds)
			{
				var b = e.MoveNext();
				Debug.Assert(b);
				e.Current.ProtocolId = Id;
				sendMsg(Entity.Id, toId, e.Current);
			}
		}

		/// <summary>
		/// Sends the i-th message to the entity with the i-th smallest id.
		/// </summary>
		protected void Send(IEnumerable<Msg> msgs)
		{
			Debug.Assert(sendMsg != null);
			Debug.Assert(EntityIds.Count() == msgs.Count());
			var e = msgs.GetEnumerator();

			foreach (var toId in EntityIds.OrderBy(i => i))
			{
				var b = e.MoveNext();
				Debug.Assert(b);
				e.Current.ProtocolId = Id;
				sendMsg(Entity.Id, toId, e.Current);
			}
		}

		/// <summary>
		/// Collects a specific number of messages of a given type and 
		/// then relays a list of them to a given callback method that
		/// belongs to a concrete protocol class.
		/// </summary>
		internal void Receive(Msg msg)
		{
			Debug.Assert(msg.SenderId >= 0, "Invalid message!");

			IRelayEntry<Msg> re = null;
			if (msg.ProtocolId == Id)
			{
				// the message belongs to me
				re = relayDic[msg.StageKey];
			}
			else
			{
				Debug.Assert(subProtocols.ContainsKey(msg.ProtocolId),
					"No receiver protocol registered! Have you registered all sub-protocols? If not, do this using RegisterSubProtocol()?");

				// so, the message belongs to one of my sub-protocols
				re = subProtocols[msg.ProtocolId].relayDic[msg.StageKey];
			}

			// if the sender is not in the list of expected senders, just ignore the message.
			if (re.SenderIds.Contains(msg.SenderId))
			{
				if (re.Count == re.SenderIds.Count - 1)
				{
					re.Add(msg);
					re.Relay();		// relay the messages
				}
				else
					re.Add(msg);
			}
		}

		/// <summary>
		/// Defines a callback method to be invoked once for every party 
		/// a message with the given protocol stage is received.
		/// </summary>
		/// <typeparam name="T">Type of messages to capture.</typeparam>
		/// <param name="stageKey">Protocol stage key.</param>
		/// <param name="onRecv">The delegate method to be invoked once all messages are received.</param>
		protected void OnReceive<T>(int stageKey, Action<List<T>> onRecv) where T : Msg
		{
			OnReceive(stageKey, EntityIds, onRecv);
		}

		/// <summary>
		/// Defines a callback method to be invoked once a specific number of messages
		/// of a given protocol stage are received from a list expected senders.
		/// </summary>
		/// <typeparam name="T">Type of messages to capture.</typeparam>
		/// <param name="stageKey">Protocol stage key.</param>
		/// <param name="senderIds">List of expected senders.</param>
		/// <param name="onRecv">The delegate method to be invoked once all messages are received.</param>
		protected void OnReceive<T>(int stageKey, IList<int> senderIds, Action<List<T>> onRecv) where T : Msg
		{
			// WARNING: THIS IS NOT SECURE! ONE MALICIOUS PARTY CAN STAY SILENT TO FREEZE THE ENTIRE PROTOCOL.
			// SOLUTION: INSTEAD OF THIS REGISTER FOR ALL MSGS RECEIVED IN THE NEXT ROUND.
			// DEFINE A ROUND TO BE A CERTAIN NUMBER OF UNIT OF TIME. THEN, INVOKE RECEIVE()
			// FORWARDING ALL MESSAGES RECEIVED IN THIS ROUND NOT FOR EVERY MESSAGE.
			// ALTERNATIVELY, WE CAN SKIP REGISTERING AND INSTEAD THE LOWER LEVEL ALWAYS
			// CALLS RECEIVE() IF THE PROTOCOL HAS ANY MESSAGE IN ITS MAILBOX FOR THAT ROUND.

			var entry = new RelayEntry<T>() { SenderIds = senderIds, OnReceive = onRecv };
			relayDic.Add(stageKey, entry);		// supported via generic covariance
		}

		/// <summary>
		/// Registers a sub-protocol to forward corresponding messages to it.
		/// </summary>
		protected void RegisterSubProtocol(Protocol sub)
		{
			subProtocols.Add(sub.Id, sub);
		}

		/// <summary>
		/// Synchronous send and receive. 
		/// Sends each given message to a given party and returns the reply messages.
		/// If a party does not respond after a timeout, then it is ignored.
		/// This method must be used in a muti-threaded/multi-process setting otherwise
		/// this method may put the single thread/process into an endless sleep.
		/// </summary>
		protected IList<Msg> SendReceive(IList<int> toIds, IList<Msg> msgs)
		{
			Debug.Assert(toIds.Count == msgs.Count);

			var recvMsgs = new List<Msg>();
			for (int i = 0; i < toIds.Count; i++)
				recvMsgs.Add(sendRecvMsg(Entity.Id, toIds[i], msgs[i]));

			return recvMsgs;
		}

		// IN TauMPC architecture:
		// A ConnectionController class handles sends/receives.
		// ConnectionController.Send() sends a message and returns the respond messages.
		// it writes to a network stream and then immediately reads from the network stream.
		// the only thread waits for each expected message and adds it to a list.
		// then the Send function returns with the list of received messages.
	}
}