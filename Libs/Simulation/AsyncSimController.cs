//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Diagnostics;
//using System.Linq;
//using System.Numerics;
//using MpcLib.Common.StochasticUtils;
//using MpcLib.DistributedSystem;
//using MpcLib.Simulation;
//using MpcLib.Simulation.Des;

//namespace MpcLib.Simulation
//{
//	/// <summary>
//	/// Represents an asynchronous network simulation controller.
//	/// </summary>
//	/// <typeparam name="T">Type of network parties.</typeparam>
//	public class AsyncSimController<T> : Controller<T> where T : AsyncParty, new()
//	{
//		private EventSimulator simulator;
//		private Object myLock = new Object();
//		public event EventHandler MessageSent;

//		public double SimulationTime
//		{
//			get
//			{
//				return simulator.Clock;
//			}
//		}

//		public AsyncSimController(EventSimulator scheduler, int seed)
//			: base(seed)
//		{
//			this.simulator = scheduler;
//		}

//		public override void Run()
//		{
//			if (parties.Count == 0)
//				throw new Exception("At least one party must be added before running the network.");

//			foreach (var e in parties)
//				simulator.Schedule(e.Id, new Handler(e.Start), 1);

//			simulator.Run();
//		}

//		protected override T CreateParty()
//		{
//			var e = new T();
//			e.SendMsg += Send;
//			e.BroadcastMsg += Broadcast;

//			e.Id = idGen++;
//			return e;
//		}

//		#region Send Methods

//		private void Send(int fromId, int toId, Msg msg, int delay)
//		{
//			var party = FindParty(toId);
//			if (party == null)
//				throw new Exception("No party found with partyId = " + toId + ".");

//			msg.SenderId = fromId;
//			Send(fromId, party, msg, delay);
//		}

//		private void Send(int fromId, AsyncParty toParty, Msg msg)
//		{
//			// TODO: Fixed delay for the synchronous model only. Must estimate message delay for the async model.
//			Send(fromId, toParty, msg, 1);
//		}

//		private void Send(int fromId, int toId, Msg msg)
//		{
//			// TODO: Fixed delay for the synchronous model only. Must estimate message delay for the async model.
//			Send(fromId, toId, msg, 1);
//		}

//		private void Send(int fromId, AsyncParty toParty, Msg msg, int delay)
//		{
//			lock (myLock)
//			{
//				// For more information, search "atomicity of variable references" in MSDN
//				SentMessageCount++;
//				SentByteCount += msg.Size;
//			}
//			simulator.Schedule(toParty.Id, toParty.Receive, delay, msg);

//			Debug.WriteLine("t=" + simulator.Clock + ": Send from " +
//				fromId + " to " + toParty.Id + "  Msg=(" + msg.ToString() + ")");

//			if (MessageSent != null)
//				MessageSent(this, new EventArgs());
//		}

//		/// <summary>
//		/// Broadcasts a message to the specified set of parties.
//		/// WARNING: Currently does not support the malicious model (active adversary), 
//		/// which requires a method Byzantine agreement to ensure message consistency.
//		/// </summary>
//		private void Broadcast(int fromId, IEnumerable<int> toIds, Msg msg)
//		{
//			foreach (var toId in toIds)
//				Send(fromId, toId, msg);		// WARNING: NOT secure in malicious model!

//			// Add actual number of message sent in the reliable broadcast protocol based on CKS '2000
//			// The party first sends msg to every other party and then parties run the Byzantine agreement 
//			// protocol of CKS '2000 to ensure consistency.
//			// CKS sends a total of 4*n^2 messages so the total number of messages sent is n + 4n^2.
//			// Note: we do not add the term n here as we have already sent the message to each party (see above).
//			// See OKS'10 for CKS'00 simulation results
//			var nSquared = (int)Math.Pow(toIds.Count(), 2);
//			SentMessageCount += 4 * nSquared;

//			// CKS sends a total of 65536*n^2 bits so the total number of bits sent is n + 65536*n^2.
//			SentByteCount += 65536 * nSquared;
//		}

//		#endregion Send Methods
//	}
//}