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
	/// <typeparam name="T">Type of network parties.</typeparam>
	public abstract class SimController<T> where T : Party, new()
	{
		public int PartyCount { get { return parties.Count; } }

		public IList<int> PartyIds { get { return parties.Select(e => e.Id).ToList(); } }

		public IList<T> Parties { get { return parties; } }

		protected List<T> parties;

		/// <summary>
		/// Total number of messages sent by all parties in the network.
		/// </summary>
		public BigInteger SentMessageCount { get; protected set; }

		/// <summary>
		/// Total number of bytes sent by all parties in the network.
		/// </summary>
		public BigInteger SentByteCount { get; protected set; }

		protected int idGen = 0;

		/// <summary>
		/// Returns the party specified by 'partyId'. This is an O(n) operation.
		/// Throws InvalidOperationException if the party does not exist.
		/// </summary>
		public T this[int partyId]
		{
			get
			{
				return parties.First(p => p.Id == partyId);
			}
		}

		public SimController(int seed)
		{
			StaticRandom.Init(seed);
			parties = new List<T>();
		}

		public abstract void Run();

		public T AddNewParty()
		{
			var party = CreateParty();
			parties.Add(party);
			return party;
		}

		public ReadOnlyCollection<T> AddNewParties(int num)
		{
			for (int i = 0; i < num; i++)
				AddNewParty();

			return parties.AsReadOnly();
		}

		protected abstract T CreateParty();

		protected T FindParty(int id)
		{
			return parties.First(p => p.Id == id);
		}
	}
}