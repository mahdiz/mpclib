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
	public abstract class SyncProtocol : Protocol
	{
		private event SendRecvHandler sendRecvMsg;
		private event BroadcastRecvHandler broadcastRecvMsg;

		public SyncProtocol(Party e, ReadOnlyCollection<int> partyIds)
			: base(e, partyIds)
		{
		}

		public SyncProtocol(SyncParty e, ReadOnlyCollection<int> partyIds)
			: this(e, partyIds, e.SendReceive, e.BroadcastRecv)
		{
		}

		public SyncProtocol(Party e, ReadOnlyCollection<int> partyIds,
			SendRecvHandler sendRecv, BroadcastRecvHandler bcastRecv)
			: base(e, partyIds)
		{
			sendRecvMsg += sendRecv;
			broadcastRecvMsg += bcastRecv;
		}

		public override void Run()
		{
			// overriding Run() is not mandatory for sync protocols.
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
				recvMsgs.Add(sendRecvMsg(Party.Id, toIds[i], msgs[i]) as T);

			return recvMsgs;
		}

		/// <summary>
		/// Synchronous send and receive. 
		/// Sends each given message to a given party and returns the corresponding messages.
		/// If a party does not respond after a timeout, then it is ignored.
		/// This method must be used in a muti-threaded/multi-process setting otherwise
		/// this method may put the single thread/process into an endless sleep.
		/// </summary>
		protected T SendReceive<T>(int toId, T msg) where T : Msg
		{
			return sendRecvMsg(Party.Id, toId, msg) as T;
		}

		/// <summary>
		/// Synchronous broadcast. 
		/// Broadcasts a message to the specified set of parties and returns the corresponding messages.
		/// Currently does not support the malicious model (active adversary), 
		/// which requires Byzantine agreement to ensure consistency.
		/// </summary>
		protected List<T> BroadcastReceive<T>(IEnumerable<int> toIds, T msg) where T : Msg
		{
			Debug.Assert(broadcastRecvMsg != null, "No broadcast method has been set for the protocol!");
			msg.ProtocolId = Id;
			return broadcastRecvMsg(Party.Id, toIds, msg).Cast<T>().ToList();
		}

		/// <summary>
		/// Synchronous broadcast. 
		/// Broadcasts a message to all parties and returns the corresponding messages.
		/// Currently does not support the malicious model (active adversary), 
		/// which requires Byzantine agreement to ensure consistency.
		/// </summary>
		protected List<T> BroadcastReceive<T>(T msg) where T : Msg
		{
			Debug.Assert(broadcastRecvMsg != null, "No broadcast method has been set for the protocol!");
			msg.ProtocolId = Id;
			return broadcastRecvMsg(Party.Id, PartyIds, msg).Cast<T>().ToList();
		}

		// IN TauMPC architecture:
		// A ConnectionController class handles sends/receives.
		// ConnectionController.Send() sends a message and returns the respond messages.
		// it writes to a network stream and then immediately reads from the network stream.
		// the only thread waits for each expected message and adds it to a list.
		// then the Send function returns with the list of received messages.
	}
}