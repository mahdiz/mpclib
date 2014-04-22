using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MpcLib.Simulation.Des
{
	public class SequentialEventSimulator : EventSimulator<EventQueue>
	{
		public override void Run()
		{
			Reset();
			while (!halted && queue.Count > 0)
			{
				var e = queue.Dequeue();

				// dispatch the event
				clock = e.Time;
				e.Handle();
			}
		}
	}

}
