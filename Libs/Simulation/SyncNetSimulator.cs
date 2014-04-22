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
			if (!sockets.ContainsKey(toId))
				sockets[toId] = new ConcurrentQueue<T>();

			sockets[toId].Enqueue(msg);			// write to the virtual socket
		}

		public static T Receive(int myId)
		{
			T msg = default(T);
			if (!sockets.ContainsKey(myId) || !sockets[myId].TryDequeue(out msg))		// read from the socket
			{
				while (msg == null)		// WARNING: Not for the malicious case!
				{
					Thread.Sleep(TimeOut);					// wait
					if (sockets.ContainsKey(myId))
						sockets[myId].TryDequeue(out msg);	// retry
				}
			}
			return msg;
		}
	}
}