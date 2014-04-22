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
	/// Represents an abstract network entity.
	/// </summary>
	public abstract class Entity
	{
		/// <summary>
		/// The unique identity of the entity.
		/// </summary>
		public int Id { get; internal set; }

		/// <summary>
		/// Runs the entity protocol.
		/// </summary>
		public abstract void Run();

		internal event SendHandler SendMsg;
		internal event SendRecvHandler SendRecvMsg;
		internal event BroadcastHandler BroadcastMsg;
		internal event BroadcastRecvHandler BroadcastRecvMsg;

		internal abstract void Receive(Msg msg);

		public void Send(int fromId, int toId, Msg msg)
		{
			SendMsg(fromId, toId, msg);
		}

		public Msg SendReceive(int fromId, int toId, Msg msg)
		{
			return SendRecvMsg(fromId, toId, msg);
		}

		public void Broadcast(int fromId, IEnumerable<int> toIds, Msg msg)
		{
			BroadcastMsg(fromId, toIds, msg);
		}

		public List<Msg> BroadcastRecv(int fromId, IEnumerable<int> toIds, Msg msg)
		{
			return BroadcastRecvMsg(fromId, toIds, msg);
		}

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
	/// Represents a generic network entity.
	/// </summary>
	/// <typeparam name="T">Protocol type.</typeparam>
	public class Entity<T> : Entity where T : Protocol
	{
		/// <summary>
		/// The protocol this entity uses to communicate with other entities.
		/// A stack of protocols should be modeled with inheritance (is-a relations).
		/// </summary>
		public T Protocol;

		public Entity()
		{
		}

		public Entity(int seed)
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