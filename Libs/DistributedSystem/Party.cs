using System.Collections.Generic;
using System.Diagnostics;
using MpcLib.Simulation.Des;

namespace MpcLib.DistributedSystem
{
	public delegate void SendHandler(int fromId, int toId, Msg msg);
	public delegate Msg SendRecvHandler(int fromId, int toId, Msg msg);
	public delegate void BroadcastHandler(int fromId, IEnumerable<int> toIds, Msg msg);
	public delegate List<Msg> BroadcastRecvHandler(int fromId, IEnumerable<int> toIds, Msg msg);

	/// <summary>
	/// Represents an abstract network party.
	/// </summary>
	public abstract class Party
	{
		/// <summary>
		/// The unique identity of the party.
		/// </summary>
		public int Id { get; internal set; }

		/// <summary>
		/// Runs the party protocol.
		/// </summary>
		public abstract void Run();

		public override int GetHashCode()
		{
			return Id;
		}

		public override string ToString()
		{
			return "Id=" + Id;
		}
	}

	/// <summary>
	/// Represents an abstract party in a synchronous network.
	/// </summary>
	public abstract class SyncParty : Party
	{
		internal event SendRecvHandler SendRecvMsg;
		internal event BroadcastRecvHandler BroadcastRecvMsg;

		public Msg SendReceive(int fromId, int toId, Msg msg)
		{
			return SendRecvMsg(fromId, toId, msg);
		}

		public List<Msg> BroadcastRecv(int fromId, IEnumerable<int> toIds, Msg msg)
		{
			return BroadcastRecvMsg(fromId, toIds, msg);
		}
	}

	/// <summary>
	/// Represents an abstract party in an asynchronous network.
	/// </summary>
	public abstract class AsyncParty : Party
	{
		internal event SendHandler SendMsg;
		internal event BroadcastHandler BroadcastMsg;

		internal abstract void Receive(Msg msg);

		public void Send(int fromId, int toId, Msg msg)
		{
			SendMsg(fromId, toId, msg);
		}

		public void Broadcast(int fromId, IEnumerable<int> toIds, Msg msg)
		{
			BroadcastMsg(fromId, toIds, msg);
		}
	}

	/// <summary>
	/// Represents a generic party of a synchronous network.
	/// </summary>
	/// <typeparam name="T">Protocol type.</typeparam>
	public class SyncParty<T> : SyncParty where T : SyncProtocol
	{
		/// <summary>
		/// The protocol this party uses to communicate with other parties.
		/// </summary>
		public T Protocol;

		public SyncParty()
		{
		}

		public SyncParty(int seed)
		{
		}

		public override void Run()
		{
			Protocol.Run();
		}
	}

	/// <summary>
	/// Represents a generic party of an asynchronous network.
	/// </summary>
	/// <typeparam name="T">Protocol type.</typeparam>
	public class AsyncParty<T> : AsyncParty where T : AsyncProtocol
	{
		/// <summary>
		/// The protocol this party uses to communicate with other parties.
		/// </summary>
		public T Protocol;

		public AsyncParty()
		{
		}

		public AsyncParty(int seed)
		{
		}

		internal override void Receive(Msg msg)
		{
			Protocol.Receive(msg);
		}

		public override void Run()
		{
			Protocol.Run();
		}
	}
}