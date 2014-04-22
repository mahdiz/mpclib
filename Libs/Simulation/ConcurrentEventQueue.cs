using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MpcLib.Simulation.Des
{
	/// <summary>
	/// Implements a thread-safe event queue for parallel discrete event simulation.
	/// </summary>
	public class ConcurrentEventQueue : IEventQueue
	{
		private ConcurrentPriorityQueue<long, BaseEvent> pQueue =
			new ConcurrentPriorityQueue<long, BaseEvent>();

		public int Count
		{
			get { return pQueue.Count; }
		}

		public void Enqueue(BaseEvent e)
		{
			pQueue.Enqueue(e.Time, e);
		}

		public BaseEvent Dequeue()
		{
			KeyValuePair<long, BaseEvent> pair;
			while (!pQueue.TryDequeue(out pair))
				;
			return pair.Value;
		}

		public IList<BaseEvent> Dequeue(long t)
		{
			var eList = new List<BaseEvent>();
			KeyValuePair<long, BaseEvent> pair;

			while (pQueue.Count > 0)
			{
				while (!pQueue.TryPeek(out pair))
					;

				if (pair.Value.Time > t)
					break;

				while (!pQueue.TryDequeue(out pair))
					;

				eList.Add(pair.Value);
			}
			return eList;
		}
	}
}
