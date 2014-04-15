using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using MpcLib.Common.StochasticUtils;
using MpcLib.Simulation.Des;

namespace MpcLib.DistributedSystem
{
	/// <summary>
	/// Represents a distributed network system.
	/// </summary>
	/// <typeparam name="TEntity">Type of network entities.</typeparam>
	public class DistributedSystem<TEntity> where TEntity : Entity, new()
	{
		public int EntityCount { get { return entities.Count; } }

		public ReadOnlyCollection<int> EntityIds { get { return entities.Select(e => e.Id).ToList().AsReadOnly(); } }

		public ReadOnlyCollection<TEntity> Entities { get { return entities.AsReadOnly(); } }

		public event EventHandler MessageSent;

		protected List<TEntity> entities;

		/// <summary>
		/// Total number of messages sent by all entities in the network.
		/// </summary>
		public BigInteger SentMessageCount { get; private set; }

		/// <summary>
		/// Total number of bytes sent by all entities in the network.
		/// </summary>
		public BigInteger SentByteCount { get; private set; }

		private int idGen = 0;
		private Simulator simulator;
		private Object myLock = new Object();

		public double SimulationTime
		{
			get
			{
				return simulator.Clock;
			}
		}

		/// <summary>
		/// Returns the entity specified by 'entityId'. This is an O(n) operation.
		/// Throws InvalidOperationException if the entity does not exist.
		/// </summary>
		public TEntity this[int entityId]
		{
			get
			{
				return entities.First(e => e.Id == entityId);
			}
		}

		public DistributedSystem(Simulator scheduler, int seed)
		{
			StaticRandom.Init(seed);
			this.simulator = scheduler;
			entities = new List<TEntity>();
		}

		public virtual void Run()
		{
			if (entities.Count == 0)
				throw new Exception("At least one entity must be added before running the network.");

			foreach (var e in entities)
				simulator.Schedule(e.Id, new Handler(e.Run), 1);

			simulator.Run();
		}

		public TEntity AddNewEntity()
		{
			var entity = CreateEntity();
			entities.Add(entity);
			return entity;
		}

		public ReadOnlyCollection<TEntity> AddNewEntities(int num)
		{
			for (int i = 0; i < num; i++)
				AddNewEntity();

			return entities.AsReadOnly();
		}

		protected TEntity CreateEntity()
		{
			var e = new TEntity();
			e.SendMsg += Send;
			e.BroadcastMsg += Broadcast;
			e.Id = idGen++;
			return e;
		}

		protected TEntity FindEntity(int id)
		{
			return entities.First(e => e.Id == id);
		}

		#region Send Methods

		private void Send(int fromId, Entity toEntity, Msg msg, int delay)
		{
			lock (myLock)
			{
				// For more information, search "atomicity of variable references" in MSDN
				SentMessageCount++;
				SentByteCount += msg.Size;
			}
			simulator.Schedule(toEntity.Id, toEntity.Receive, delay, msg);

			Debug.WriteLine("t=" + simulator.Clock + ": Send from " + 
				fromId + " to " + toEntity.Id + "  Msg=(" + msg.ToString() + ")");

			if (MessageSent != null)
				MessageSent(this, new EventArgs());
		}

		private void Send(int fromId, int toId, Msg msg, int delay)
		{
			var entity = FindEntity(toId);
			if (entity == null)
				throw new Exception("No entity found with entityId = " + toId + ".");

			msg.SenderId = fromId;
			Send(fromId, entity, msg, delay);
		}

		private void Send(int fromId, Entity toEntity, Msg msg)
		{
			// TODO: Fixed delay for the synchronous model only. Must estimate message delay for the async model.
			Send(fromId, toEntity, msg, 1);
		}

		private void Send(int fromId, int toId, Msg msg)
		{
			// TODO: Fixed delay for the synchronous model only. Must estimate message delay for the async model.
			Send(fromId, toId, msg, 1);
		}

		/// <summary>
		/// Broadcasts a message to the specified set of entities.
		/// WARNING: Currently does not support the malicious model (active adversary), 
		/// which requires a method Byzantine agreement to ensure message consistency.
		/// </summary>
		private void Broadcast(int fromId, IEnumerable<int> toIds, Msg msg)
		{
			foreach (var toId in toIds)
				Send(fromId, toId, msg);		// WARNING: NOT secure in malicious model!

			// Add actual number of message sent in the reliable broadcast protocol based on CKS '2000
			// The party first sends msg to every other party and then parties run the Byzantine agreement 
			// protocol of CKS '2000 to ensure consistency.
			// CKS sends a total of 4*n^2 messages so the total number of messages sent is n + 4n^2.
			// Note: we do not add the term n here as we have already sent the message to each party (see above).
			// See OKS'10 for CKS'00 simulation results
			var nSquared = (int)Math.Pow(toIds.Count(), 2);
			SentMessageCount += 4 * nSquared;

			// CKS sends a total of 65536*n^2 bits so the total number of bits sent is n + 65536*n^2.
			SentByteCount += 65536 * nSquared;
		}

		#endregion Send Methods
	}
}