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
	/// Represents an abstract asynchronous network protocol.
	/// </summary>
	public abstract class AsyncProtocol : Protocol<AsyncParty>
	{
		protected readonly StateKey StateKey;
		private event SendHandler sendMsg;
		private event BroadcastHandler broadcastMsg;

		private readonly Dictionary<int, IRelayEntry<Msg>> relayDic = 
			new Dictionary<int, IRelayEntry<Msg>>();

		private Dictionary<ProtocolIds, AsyncProtocol> subProtocols = 
			new Dictionary<ProtocolIds, AsyncProtocol>();

		public AsyncProtocol(AsyncParty p, IList<int> partyIds, StateKey stateKey)
			: this(p, partyIds, p.Send, p.Broadcast, stateKey)
		{
		}

		public AsyncProtocol(AsyncParty p, IList<int> partyIds,
			SendHandler send, StateKey stateKey)
			: this(p, partyIds, send, null, stateKey)
		{
		}

		public AsyncProtocol(AsyncParty p, IList<int> partyIds,
			SendHandler send, BroadcastHandler bcast, StateKey stateKey)
			: base(p, partyIds)
		{
			StateKey = stateKey;
			sendMsg += send;
			broadcastMsg += bcast;
		}

		protected virtual void Send(int fromId, int toId, Msg msg)
		{
			Debug.Assert(sendMsg != null, "No send method have been set for the protocol!");
			msg.ProtocolId = Id;
			sendMsg(fromId, toId, msg);
		}

		/// <summary>
		/// Broadcasts a message to the specified set of parties.
		/// Currently does not support the malicious model (active adversary), 
		/// which requires Byzantine agreement to ensure consistency.
		/// </summary>
		protected void Broadcast(IEnumerable<int> toIds, Msg msg)
		{
			Debug.Assert(broadcastMsg != null, "No broadcast method has been set for the protocol!");
			msg.ProtocolId = Id;
			broadcastMsg(Party.Id, toIds, msg);
		}

		/// <summary>
		/// Broadcasts a message.
		/// Currently does not support the malicious model, 
		/// which requires Byzantine agreement to ensure consistency.
		/// </summary>
		protected void Broadcast(Msg msg)
		{
			Broadcast(PartyIds, msg);
		}

		/// <summary>
		/// Sends the i-th message to the i-th party.
		/// </summary>
		protected void Send(IEnumerable<int> toIds, IEnumerable<Msg> msgs)
		{
			Debug.Assert(sendMsg != null);
			Debug.Assert(toIds.Count() == msgs.Count());
			var p = msgs.GetEnumerator();

			foreach (var toId in toIds)
			{
				var b = p.MoveNext();
				Debug.Assert(b);
				p.Current.ProtocolId = Id;
				sendMsg(Party.Id, toId, p.Current);
			}
		}

		/// <summary>
		/// Sends the i-th message to the party with the i-th smallest id.
		/// </summary>
		protected void Send(IEnumerable<Msg> msgs)
		{
			Debug.Assert(sendMsg != null);
			Debug.Assert(PartyIds.Count() == msgs.Count());
			var e = msgs.GetEnumerator();

			foreach (var toId in PartyIds.OrderBy(i => i))
			{
				var b = e.MoveNext();
				Debug.Assert(b);
				e.Current.ProtocolId = Id;
				sendMsg(Party.Id, toId, e.Current);
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
			OnReceive(stageKey, PartyIds, onRecv);
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
		protected void RegisterSubProtocol(AsyncProtocol sub)
		{
			subProtocols.Add(sub.Id, sub);
		}
	}
}