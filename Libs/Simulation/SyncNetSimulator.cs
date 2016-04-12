using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

namespace MpcLib.Simulation
{
	/// <summary>
	/// Declares an abstract synchronous network system.
	/// </summary>
	public class SyncNetSimulator<T>
	{
		const int TimeOut = 20;		// in milliseconds
		static ConcurrentDictionary<int, ConcurrentQueue<T>> sockets = new ConcurrentDictionary<int, ConcurrentQueue<T>>();

		public static void Send(int toId, T msg)
		{
			Debug.Assert(msg != null);

			if (!sockets.ContainsKey(toId))
				sockets.TryAdd(toId, new ConcurrentQueue<T>());

			sockets[toId].Enqueue(msg);			// write to the virtual socket
		}

		public static T Receive(int myId)
		{
			T msg = default(T);

			// WARNING: Not for the malicious case! This loop will never end if any party remains silent!
			while (!sockets.ContainsKey(myId) || !sockets[myId].TryDequeue(out msg))
				Thread.Sleep(TimeOut);		// wait

			Debug.Assert(msg != null);
			return msg;
		}
	}
}