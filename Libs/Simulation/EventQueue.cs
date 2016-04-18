using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MpcLib.Common.BasicDataStructures;

namespace MpcLib.Simulation.Des
{
	public interface IEventQueue
	{
		int Count { get; }
		void Enqueue(BaseEvent e);
		BaseEvent Dequeue();
		IList<BaseEvent> Dequeue(long t);
	}

	/// <summary>
	/// Implements an event queue for discrete-event simulation.
	/// </summary>
	public class EventQueue : IEventQueue
	{
		private StableBinaryHeap<BaseEvent> pQueue = new StableBinaryHeap<BaseEvent>();

		public int Count
		{
			get
			{
				return pQueue.Count;
			}
		}

		public EventQueue()
		{
		}

		public virtual void Enqueue(BaseEvent e)
		{
			pQueue.Add(e);
		}

		public virtual BaseEvent Dequeue()
		{
			return pQueue.Remove();
		}

		public virtual IList<BaseEvent> Dequeue(long t)
		{
			var eList = new List<BaseEvent>();
			while (pQueue.Count > 0 && pQueue.Peek().Time <= t)
				eList.Add(pQueue.Remove());

			return eList;
		}
	}
}