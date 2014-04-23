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
	public abstract class Protocol
	{
		/// <summary>
		/// Unique idetifier of the protocol type.
		/// </summary>
		public abstract ProtocolIds Id { get; }

		/// <summary>
		/// The party associated with this protocol.
		/// </summary>
		protected readonly Party Party;

		/// <summary>
		/// Number of parties in the network.
		/// </summary>
		protected readonly int NumParties;
		protected readonly ReadOnlyCollection<int> PartyIds;

		public Protocol(Party e, ReadOnlyCollection<int> partyIds)
		{
			this.Party = e;
			PartyIds = partyIds;
			NumParties = partyIds.Count;
		}

		/// <summary>
		/// Runs the protocol.
		/// </summary>
		public abstract void Run();
	}
}