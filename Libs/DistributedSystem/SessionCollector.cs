using System.Collections.Generic;

namespace MpcLib.DistributedSystem
{
	/// <summary>
	/// Implements a collector of protocol sessions. This maps a protocol session key to a protocol session.
	/// </summary>
	/// <typeparam name="S">Type of session.</typeparam>
	public class SessionCollector<S>
	{
		private Dictionary<StateKey, S> sessions = new Dictionary<StateKey, S>();

		public S this[StateKey key]
		{
			get
			{
				return sessions[key];
			}
		}

		public void Collect(StateKey key, S session)
		{
			sessions[key] = session;
		}

		public bool ContainsKey(StateKey key)
		{
			return sessions.ContainsKey(key);
		}
	}
}