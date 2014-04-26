using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using MpcLib.Common;

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
	/// <typeparam name="T">Type of network parties.</typeparam>
	public abstract class Protocol<T> where T : Party
	{
		/// <summary>
		/// Unique idetifier of the protocol type.
		/// </summary>
		public abstract ProtocolIds Id { get; }

		/// <summary>
		/// The party associated with this protocol.
		/// </summary>
		public readonly T Party;

		/// <summary>
		/// Number of parties in the network.
		/// </summary>
		protected readonly int NumParties;
		protected readonly IList<int> PartyIds;

		public Protocol(T p, IList<int> partyIds)
		{
			this.Party = p;
			PartyIds = partyIds;
			NumParties = partyIds.Count;

			//// make sure I am among the parties.
			//Debug.Assert(partyIds.Contains(p.Id), "The party must be in the list of parties.");
		}

		/// <summary>
		/// Runs the protocol.
		/// </summary>
		public abstract void Run();
	}
}