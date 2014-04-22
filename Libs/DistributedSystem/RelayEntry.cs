using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.DistributedSystem
{
	// Covariant interface
	internal interface IRelayEntry<out T> where T : Msg
	{
		IList<int> SenderIds { get; }
		int Count { get; }
		void Relay();
		void Add(Msg msg);
	}

	internal class RelayEntry<T> : IRelayEntry<T> where T : Msg
	{
		public IList<int> SenderIds { get; set; }
		public Action<List<T>> OnReceive;
		private Dictionary<int, T> msgs = new Dictionary<int, T>();

		public void Add(Msg msg)
		{
			Debug.Assert(msg is T, "Message type and stage key do not match!");

			// if a message already received from this party, ignore it.
			if (!msgs.ContainsKey(msg.SenderId))
				msgs.Add(msg.SenderId, msg as T);
		}

		public int Count
		{
			get { return msgs.Count; }
		}

		public void Relay()
		{
			OnReceive(msgs.Values.ToList());
		}
	}
}
