namespace MpcLib.Simulation.Des
{
	public delegate void Handler();
	public delegate void Handler<T>(T e);

	/// <summary>
	/// Declares an abstract Discrete-Event Simulation (DES) system.
	/// </summary>
	public abstract class EventSimulator
	{
		/// <summary>
		/// Simulation clock.
		/// </summary>
		protected long clock;
		protected bool halted;

		public double Clock
		{
			get
			{
				return clock;
			}
		}

		public void Reset()
		{
			halted = false;
			clock = 0;
		}

		/// <summary>
		/// Stops the simulation.
		/// </summary>
		public void Halt()
		{
			halted = true;
		}		

		/// <summary>
		/// Runs the simulation.
		/// </summary>
		public abstract void Run();

		/// <summary>
		/// Schedules an event at a specific time.
		/// </summary>
		/// <param name="targetId">The unique ID of the entity that will handle this event.</param>
		/// <param name="handler">The event handler method.</param>
		/// <param name="delay">The delay from current simulation time.</param>
		public abstract void Schedule(int targetId, Handler handler, int delay);

		/// <summary>
		/// Schedules an event at a specific time.
		/// </summary>
		/// <param name="targetId">The unique ID of the entity that will handle this event.</param>
		/// <param name="handler">The event handler method.</param>
		/// <param name="delay">The delay from current simulation time.</param>
		/// <param name="arg">The argument passed to the handler when invoked.</param>		
		public abstract void Schedule<T>(int targetId, Handler<T> handler, int delay, T arg);

		/// <summary>
		/// Schedules an event at a specific time.
		/// </summary>
		/// <param name="targetId">The unique ID of the entity that will handle this event.</param>
		/// <param name="handler">The event handler method.</param>
		/// <param name="delay">The delay from current simulation time.</param>
		/// <param name="arg">The argument passed to the handler when invoked.</param>		
		public abstract void Schedule<T>(int targetId, Handler<T> handler, object obj, int delay, T arg);
	}

	/// <summary>
	/// Implements a Discrete-Event Simulation (DES) system.
	/// </summary>
	/// <typeparam name="Q">Simulator queue type.</typeparam>
	public abstract class EventSimulator<Q> : EventSimulator where Q : IEventQueue, new()
	{
		protected Q queue = new Q();

		/// <summary>
		/// Schedules an event without any argument.
		/// </summary>
		/// <param name="handler">The handler method.</param>
		/// <param name="delay">Delay from current clock.</param>
		public override void Schedule(int targetId, Handler handler, int delay)
		{
			queue.Enqueue(new Event(targetId, handler, clock + delay));
		}

		/// <summary>
		/// Schedules an event with a generic type object as the event argument.
		/// </summary>
		/// <param name="handler">The handler method.</param>
		/// <param name="delay">Delay from current clock.</param>
		/// <param name="arg">The event argument.</param>
		public override void Schedule<T>(int targetId, Handler<T> handler, int delay, T arg)
		{
			queue.Enqueue(new Event<T>(targetId, handler, clock + delay, arg));
		}

		/// <summary>
		/// Schedules a generic event with a handler that would be invoked dynamically for a specific object using reflection.
		/// Use with care due to performance issues. Also, ensure that the handler is declared or inherited by the class of obj,
		/// otherwise a TargetException will be raised when the event is dispatching.
		/// </summary>
		public override void Schedule<T>(int targetId, Handler<T> handler, object obj, int delay, T arg)
		{
			queue.Enqueue(new ReflectionEvent<T>(targetId, handler, obj, clock + delay, arg));
		}
	}
}