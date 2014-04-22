using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using MpcLib.Common.StochasticUtils;
using MpcLib.DistributedSystem;
using MpcLib.Simulation;
using MpcLib.Simulation.Des;

namespace MpcLib.Simulation
{
	/// <summary>
	/// Represents an abstract network simulation controller.
	/// </summary>
	/// <typeparam name="T">Type of network entities.</typeparam>
	public abstract class SimController<T> where T : Entity, new()
	{
		public int EntityCount { get { return entities.Count; } }

		public ReadOnlyCollection<int> EntityIds { get { return entities.Select(e => e.Id).ToList().AsReadOnly(); } }

		public ReadOnlyCollection<T> Entities { get { return entities.AsReadOnly(); } }

		protected List<T> entities;

		/// <summary>
		/// Total number of messages sent by all entities in the network.
		/// </summary>
		public BigInteger SentMessageCount { get; protected set; }

		/// <summary>
		/// Total number of bytes sent by all entities in the network.
		/// </summary>
		public BigInteger SentByteCount { get; protected set; }

		protected int idGen = 0;

		/// <summary>
		/// Returns the entity specified by 'entityId'. This is an O(n) operation.
		/// Throws InvalidOperationException if the entity does not exist.
		/// </summary>
		public T this[int entityId]
		{
			get
			{
				return entities.First(e => e.Id == entityId);
			}
		}

		public SimController(int seed)
		{
			StaticRandom.Init(seed);
			entities = new List<T>();
		}

		public abstract void Run();

		public T AddNewEntity()
		{
			var entity = CreateEntity();
			entities.Add(entity);
			return entity;
		}

		public ReadOnlyCollection<T> AddNewEntities(int num)
		{
			for (int i = 0; i < num; i++)
				AddNewEntity();

			return entities.AsReadOnly();
		}

		protected abstract T CreateEntity();

		protected T FindEntity(int id)
		{
			return entities.First(e => e.Id == id);
		}
	}
}