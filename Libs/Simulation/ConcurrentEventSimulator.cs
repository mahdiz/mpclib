using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace MpcLib.Simulation.Des
{
	/// <summary>
	/// Implements a concurrent (parllel) Discrete-Event Simulation (DES) system.
	/// </summary>
	public class ConcurrentEventSimulator : EventSimulator<ConcurrentEventQueue>
	{
		private Object myLock = new Object();

		public override void Run()
		{
			long t = 1;			
			while (!halted && queue.Count > 0)
			{
				var eGroups = queue.Dequeue(t).GroupBy(e => e.TargetId);
				int activeCount = eGroups.Count();
				clock = t;

				// run events in eList in parallel
				foreach (var eGroup in eGroups)
				{
					ThreadPool.QueueUserWorkItem(new WaitCallback(
						delegate(Object s)
						{
							foreach (var e in eGroup)
								e.Handle();

							lock (myLock)
								activeCount--;
						}));
				}

				// wait until all events in eList are finished
				while (activeCount > 0)
					Thread.Sleep(10);

				t++;
			}
		}
	}
}