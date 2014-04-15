using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

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
		#region Fields

		/// <summary>
		/// The entity associated with this protocol.
		/// </summary>
		protected readonly Entity Entity;

		/// <summary>
		/// Number of entities (nodes) in the network.
		/// </summary>
		protected readonly int EntityCount;
		protected readonly StateKey StateKey;
		protected readonly ReadOnlyCollection<int> EntityIds;
		private readonly Dictionary<int, IRelayEntry<Msg>> relayDic = new Dictionary<int, IRelayEntry<Msg>>();
		private event SendHandler sendMsg;
		private event BroadcastHandler broadcastMsg;

		#endregion Fields

		#region RelayEntry definitions

		// Covariant interface
		private interface IRelayEntry<out T> where T : Msg
		{
			int GoalCount { get; }
			int Count { get; }
			void Relay();
			void Add(Msg msg);
		}

		private class RelayEntry<T> : IRelayEntry<T> where T : Msg
		{
			public int GoalCount { get; set; }
			public Action<List<T>> OnReceive;
			private List<T> msgs = new List<T>();

			public void Add(Msg msg)
			{
				Debug.Assert(msg is T, "Message type and stage key do not match!");
				msgs.Add(msg as T);
			}

			public int Count
			{
				get { return msgs.Count; }
			}

			public void Relay()
			{
				OnReceive(msgs);
			}
		}

		#endregion OnReceive definitions

		public Protocol(Entity e, ReadOnlyCollection<int> entityIds, StateKey stateKey)
			: this(e, entityIds, e.Send, e.Broadcast, stateKey)
		{
		}

		public Protocol(Entity e, ReadOnlyCollection<int> entityIds,
			SendHandler send, StateKey stateKey)
			: this(e, entityIds, send, null, stateKey)
		{
		}

		public Protocol(Entity e, ReadOnlyCollection<int> entityIds,
			SendHandler send, BroadcastHandler bcast, StateKey stateKey)
		{
			Entity = e;
			EntityIds = entityIds;
			EntityCount = entityIds.Count;
			sendMsg += send;
			broadcastMsg += bcast;
			StateKey = stateKey;
#if !SIMULATION
			RandGen = new CryptoRandom();
#endif
		}

		protected virtual void Send(int fromId, int toId, Msg msg)
		{
			Debug.Assert(sendMsg != null, "No send method have been set for the protocol!");
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
				sendMsg(Entity.Id, toId, e.Current);
			}
		}

		/// <summary>
		/// Sends the i-th message to entity with id i.
		/// </summary>
		protected void Send(IEnumerable<Msg> msgs)
		{
			Debug.Assert(sendMsg != null);
			Debug.Assert(EntityIds.Count() == msgs.Count());
			var e = msgs.GetEnumerator();

			foreach (var toId in EntityIds)
			{
				var b = e.MoveNext();
				Debug.Assert(b);
				sendMsg(Entity.Id, toId, e.Current);
			}
		}

		public abstract void Run();

		/// <summary>
		/// Collects a specific number of messages of a given type and 
		/// then relays a list of them to a given callback method that
		/// belongs to a concrete protocol class.
		/// </summary>
		internal void Receive(Msg msg)
		{
			Debug.Assert(msg.SenderId >= 0, "Invalid message!");		
			var e = relayDic[msg.StageKey];

			if (e.Count == e.GoalCount - 1)
			{
				e.Add(msg);
				e.Relay();		// relay the messages to 
			}
			else e.Add(msg);
		}

		/// <summary>
		/// Defines a callback method via a delegate to be invoked once a
		/// specific number of messages of a given protocol stage are received.
		/// </summary>
		/// <typeparam name="T">Type of messages to capture.</typeparam>
		/// <param name="stageKey">Protocol stage key.</param>
		/// <param name="numMsgToReceive">Number of messages to be received before the delegate is invoked.</param>
		/// <param name="onRecv">The delegate method to be invoked once the given number of messages are received.</param>
		protected void OnReceive<T>(int stageKey, int numMsgToReceive, Action<List<T>> onRecv) where T : Msg
		{
			var entry = new RelayEntry<T>() { GoalCount = numMsgToReceive, OnReceive = onRecv };
			relayDic.Add(stageKey, entry);		// supported via generic covariance
		}
	}
}