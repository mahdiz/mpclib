using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MpcLib.Common.StochasticUtils;
using MpcLib.DistributedSystem;
using MpcLib.Simulation;
using MpcLib.Simulation.Des;

namespace MpcLib.Simulation
{
	public delegate void SentHandler(int fromId, int toId, int msgSize);

	/// <summary>
	/// Represents a synchronous network simulation controller.
	/// </summary>
	/// <typeparam name="T">Type of network parties.</typeparam>
	public class SyncSimController<T> : SimController<T> where T : SyncParty, new()
	{
		private Object myLock = new Object();
		public event SentHandler MessageSent;

		public SyncSimController(int seed)
			: base(seed)
		{
		}

		public override void Run()
		{
			if (parties.Count == 0)
				throw new Exception("At least one party must be added before running the simulation.");

			int numActive = parties.Count;
			foreach (var party in parties)
			{
				ThreadPool.QueueUserWorkItem(new WaitCallback(
					delegate(Object s)
					{
						party.Run();

						lock (myLock)
							numActive--;
					}));
			}

			// wait until all parties finish running
			while (numActive > 0)
				Thread.Sleep(10);

			//var tasks = new Task[parties.Count];
			//for (int i = 0; i < parties.Count; i++)
			//{
			//	var party = parties[i];
			//	tasks[i] = Task.Run(() => party.Run());
			//}
			//Task.WaitAll(tasks);
		}

		protected override T CreateParty()
		{
			var p = new T();

			p.SendMsg += send;
			p.ReceiveMsg += receive;
			p.SendRecvMsg += sendReceive;
			p.BroadcastMsg += broadcast;
			p.BroadcastRecvMsg += broadcastReceive;

			p.Id = idGen++;
			return p;
		}

		private void send(int fromId, int toId, Msg msg)
		{
			lock (myLock)
			{
				// For more information, search "atomicity of variable references" in MSDN
				SentMessageCount++;
				SentByteCount += msg.Size;
			}
			msg.SenderId = fromId;
			SyncNetSimulator<Msg>.Send(toId, msg);

			if (MessageSent != null)
				MessageSent(fromId, toId, msg.Size);
		}

		private Msg receive(int myId)
		{
			return SyncNetSimulator<Msg>.Receive(myId);
		}

		private Msg sendReceive(int fromId, int toId, Msg msg)
		{
			send(fromId, toId, msg);
			return SyncNetSimulator<Msg>.Receive(fromId);
		}

		private void broadcast(int fromId, IEnumerable<int> toIds, Msg msg)
		{
			msg.SenderId = fromId;
			foreach (var toId in toIds)
			{
                SyncNetSimulator<Msg>.Send(toId, msg);

				if (MessageSent != null)
					MessageSent(fromId, toId, msg.Size);
			}

			lock (myLock)
			{
				// Add actual number of message sent in the reliable broadcast protocol based on CKS '2000
				// The party first sends msg to every other party and then parties run the Byzantine agreement 
				// protocol of CKS '2000 to ensure consistency.
				// CKS sends a total of 4*n^2 messages so the total number of messages sent is n + 4n^2.
				// Note: we do not add the term n here as we have already sent the message to each party (see above).
				// See OKS'10 for CKS'00 simulation results
				var nSquared = (int)Math.Pow(toIds.Count(), 2);
				SentMessageCount += 4 * nSquared;

				// For 80 bit security (i.e., RSA 1024), CKS sends 4*n^2 messages, where each message is of at most
				// two RSA signatures (i.e., 2048 bit messages). So, the total number of bits sent is 8192*n^2 (or 1024*n^2 bytes).
				SentByteCount += 1024 * nSquared;
			}
		}

		private List<Msg> broadcastReceive(int fromId, IEnumerable<int> toIds, Msg msg)
		{
			broadcast(fromId, toIds, msg);

			var retMsgs = new List<Msg>();
            for (int i = 0; i < toIds.Count(); i++)
            {
                retMsgs.Add(SyncNetSimulator<Msg>.Receive(fromId));
            }
			return retMsgs;
		}
	}
}