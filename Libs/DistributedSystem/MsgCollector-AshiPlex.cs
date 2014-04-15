using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace Unm.DistributedSystem
{
	public abstract class MsgCollectionKey
	{
		public abstract override bool Equals(object obj);
		public abstract override int GetHashCode();
	}

	public class MsgCollection<T> : IEnumerable<T> 
		where T : Message
	{
		private Dictionary<int, T> dic = new Dictionary<int, T>();

		public T this[int senderId]
		{
			get
			{
				return dic[senderId];
			}
			internal set
			{
				dic[senderId] = value;
			}
		}

		public int Count
		{
			get
			{
				return dic.Count;
			}
		}

		public IEnumerable<int> EntityIds
		{
			get
			{
				return dic.Keys;
			}
		}

		public bool ContainsSender(int senderId)
		{
			return dic.ContainsKey(senderId);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return dic.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return dic.Values.GetEnumerator();
		}
	}

	/// <summary>
	/// Implements a collector of protocol received messages. This maps a protocol state to a set of messages received from other nodes.
	/// The set of messages itself maps the sender ID to the message received from that sender.
	/// </summary>
	public class MsgCollector<T> 
		where T : Message
	{
		private Dictionary<MsgCollectionKey, MsgCollection<T>> msgs = 
			new Dictionary<MsgCollectionKey, MsgCollection<T>>();

		public MsgCollection<T> Collect(MsgCollectionKey key, T msg, int stopCount)
		{
			Debug.Assert(msg.SenderId < 0, "Invalid message!");

			if (!msgs.ContainsKey(key))
				msgs[key] = new MsgCollection<T>();

			Debug.Assert(msgs[key].ContainsSender(msg.SenderId), "A message has already been received from this entity!");
			var keyMsgs = msgs[key];
			keyMsgs[msg.SenderId] = msg;

			if (keyMsgs.Count < stopCount)
				return null;
			else
			{
				msgs.Remove(key);
				return keyMsgs;
			}
		}
	}
}
