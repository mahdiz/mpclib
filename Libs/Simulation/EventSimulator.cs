namespace MpcLib.Simulation.Des
{
	public delegate void Handler();
	public delegate void Handler<T>(T e);
    public delegate void Handler<T,S>(T a, S b);

    /// <summary>
    /// Implements a Discrete-Event Simulation (DES) system.
    /// </summary>
    /// <typeparam name="Q">Simulator queue type.</typeparam>
    public class EventSimulator<Q> where Q : IEventQueue, new()
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

        protected Q queue = new Q();

        /// <summary>
        /// Schedules an event without any argument.
        /// </summary>
        /// <param name="targetId">The unique ID of the entity that will handle this event.</param>
        /// <param name="handler">The handler method.</param>
        /// <param name="delay">Delay from current clock.</param>
        public void Schedule(int targetId, Handler handler, int delay)
		{
			queue.Enqueue(new Event(targetId, handler, clock + delay));
		}

        /// <summary>
        /// Schedules an event with a generic type object as the event argument.
        /// </summary>
        /// <param name="targetId">The unique ID of the entity that will handle this event.</param>
        /// <param name="handler">The handler method.</param>
        /// <param name="delay">Delay from current clock.</param>
        /// <param name="arg">The event argument.</param>
        public void Schedule<T>(int targetId, Handler<T> handler, int delay, T arg)
		{
			queue.Enqueue(new Event<T>(targetId, handler, clock + delay, arg));
		}

        /// <summary>
        /// Schedules an event with a generic type object as the event argument.
        /// </summary>
        /// <param name="targetId">The unique ID of the entity that will handle this event.</param>
        /// <param name="handler">The handler method.</param>
        /// <param name="delay">Delay from current clock.</param>
        /// <param name="arg">The event argument.</param>
        public void Schedule<T,S>(int targetId, Handler<T,S> handler, int delay, T arg1, S arg2)
        {
            queue.Enqueue(new Event<T,S>(targetId, handler, clock + delay, arg1, arg2));
        }

        /// <summary>
        /// Schedules a generic event with a handler that would be invoked dynamically for a specific object using reflection.
        /// Use with care due to performance issues. Also, ensure that the handler is declared or inherited by the class of obj,
        /// otherwise a TargetException will be raised when the event is dispatching.
        /// </summary>
        /// <param name="targetId">The unique ID of the entity that will handle this event.</param>
        /// <param name="handler">The event handler method.</param>
        /// <param name="delay">The delay from current simulation time.</param>
        /// <param name="arg">The argument passed to the handler when invoked.</param>		
        public void Schedule<T>(int targetId, Handler<T> handler, object obj, int delay, T arg)
		{
			queue.Enqueue(new ReflectionEvent<T>(targetId, handler, obj, clock + delay, arg));
		}

        /// <summary>
        /// Runs the simulation.
        /// </summary>
        public virtual void Run()
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