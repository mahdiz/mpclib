using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using MpcLib.Common;

namespace MpcLib.DistributedSystem
{
	/// <summary>
	/// Represents an abstract synchronous network protocol.
	/// </summary>
	public abstract class SyncProtocol : Protocol<SyncParty>
	{
		public SyncProtocol(SyncParty p, IList<int> partyIds)
			: base(p, partyIds)
		{
		}

		public override void Run()
		{
			// overriding Run() is not mandatory for sync protocols.
		}

		protected void Send<T>(int toId, T msg) where T : Msg
		{
			msg.ProtocolId = Id;
			Party.Send(Party.Id, toId, msg);
		}

		protected void Send<T>(IList<int> toIds, IList<T> msgs) where T : Msg
		{
			Debug.Assert(toIds.Count == msgs.Count);

			for (int i = 0; i < toIds.Count; i++)
			{
				msgs[i].ProtocolId = Id;
				Party.Send(Party.Id, toIds[i], msgs[i]);
			}
		}

		public T Receive<T>() where T : Msg
		{
			return Party.Receive() as T;
		}

		/// <summary>
		/// Synchronous send and receive. 
		/// Sends each given message to a given party and returns the corresponding messages.
		/// If a party does not respond after a timeout, then it is ignored.
		/// This method must be used in a muti-threaded/multi-process setting otherwise
		/// this method may put the single thread/process into an endless sleep.
		/// </summary>
		protected IList<T> SendReceive<T>(IList<int> toIds, IList<T> msgs) where T : Msg
		{
			Debug.Assert(toIds.Count == msgs.Count);

			var recvMsgs = new List<T>();
			for (int i = 0; i < toIds.Count; i++)
			{
				msgs[i].ProtocolId = Id;
				recvMsgs.Add(Party.SendReceive(Party.Id, toIds[i], msgs[i]) as T);
			}

			return recvMsgs;
		}

		/// <summary>
		/// Synchronous send and receive. 
		/// Sends a message to all parties via one-to-one communication 
		/// and returns the corresponding messages.
		/// If a party does not respond after a timeout, then it is ignored.
		/// </summary>
		protected IList<T> SendReceive<T>(T msg) where T : Msg
		{
			msg.ProtocolId = Id;

			foreach (var toId in PartyIds)
				Party.Send(Party.Id, toId, msg);

			var recvMsgs = new List<T>();
			foreach (var toId in PartyIds)
				recvMsgs.Add(Party.Receive() as T);

			return recvMsgs;
		}

		/// <summary>
		/// Synchronous send and receive. 
		/// Sends a message to a given party and returns the corresponding message.
		/// If a party does not respond after a timeout, then it is ignored.
		/// This method must be used in a muti-threaded/multi-process setting otherwise
		/// this method may put the single thread/process into an endless sleep.
		/// </summary>
		protected T SendReceive<T>(int toId, T msg) where T : Msg
		{
			msg.ProtocolId = Id;
			return Party.SendReceive(Party.Id, toId, msg) as T;
		}

		protected void Broadcast<T>(IEnumerable<int> toIds, T msg) where T : Msg
		{
			msg.ProtocolId = Id;
			Party.Broadcast(Party.Id, toIds, msg);
		}

		/// <summary>
		/// Synchronous broadcast. 
		/// Broadcasts a message to the specified set of parties and returns the corresponding messages.
		/// Currently does not support the malicious model (active adversary), 
		/// which requires Byzantine agreement to ensure consistency.
		/// </summary>
		protected List<T> BroadcastReceive<T>(IEnumerable<int> toIds, T msg) where T : Msg
		{
			msg.ProtocolId = Id;
			return Party.BroadcastRecv(Party.Id, toIds, msg).Cast<T>().ToList();
		}

		/// <summary>
		/// Synchronous broadcast. 
		/// Broadcasts a message to all parties and returns the corresponding messages.
		/// Currently does not support the malicious model (active adversary), 
		/// which requires Byzantine agreement to ensure consistency.
		/// </summary>
		protected List<T> BroadcastReceive<T>(T msg) where T : Msg
		{
			msg.ProtocolId = Id;
			return Party.BroadcastRecv(Party.Id, PartyIds, msg).Cast<T>().ToList();
		}

		// IN TauMPC architecture:
		// A ConnectionController class handles sends/receives.
		// ConnectionController.Send() sends a message and returns the respond messages.
		// it writes to a network stream and then immediately reads from the network stream.
		// the only thread waits for each expected message and adds it to a list.
		// then the Send function returns with the list of received messages.
	}
}