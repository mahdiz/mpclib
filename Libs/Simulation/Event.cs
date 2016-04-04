using System;
using System.Diagnostics;
using System.Reflection;
using MpcLib.Common.StochasticUtils;

namespace MpcLib.Simulation.Des
{
	public delegate void OnFinish();

	public abstract class BaseEvent : IComparable<BaseEvent>
	{
		/// <summary>
		/// The simulation time this event will be invoked.
		/// </summary>
		public readonly long Time;
		public readonly int TargetId;
		public abstract event OnFinish OnFinish;

		public BaseEvent(int targetId, long time)
		{
			Time = time;
			TargetId = targetId;
		}

		public abstract void Handle();

		public int CompareTo(BaseEvent other)
		{
			return Time.CompareTo(other.Time);
		}

		public override string ToString()
		{
			return "t=" + Time.ToString("0.#") + ", TargetId=" + TargetId;
		}
	}

	/// <summary>
	/// Represents an argument-free event for scheduling.
	/// </summary>
	internal class Event : BaseEvent
	{
		private readonly Handler handler;
		public override event OnFinish OnFinish;

		public Event(int targetId, Handler handler, long time)
			: base(targetId, time)
		{
			this.handler = handler;
		}

		public override void Handle()
		{
			Debug.Assert(handler != null, "Event does not have any handler.");
			handler();

			if (OnFinish != null)
				OnFinish();
		}
	}

	/// <summary>
	/// Represents an event for scheduling.
	/// </summary>
	/// <typeparam name="T">The event argument type.</typeparam>
	internal class Event<T> : BaseEvent
	{
		private readonly T arg;
		public readonly Handler<T> handler;
		public override event OnFinish OnFinish;

		public Event(int targetId, Handler<T> handler, long time, T arg)
			: base(targetId, time)
		{
			this.arg = arg;
			this.handler = handler;
		}

		public override void Handle()
		{
			Debug.Assert(handler != null, "Event does not have any handler.");
			handler(arg);

			if (OnFinish != null)
				OnFinish();
		}

		public override string ToString()
		{
			return base.ToString() + ", " + arg;
		}
	}

    /// <summary>
    /// Represents an event for scheduling.
    /// </summary>
    /// <typeparam name="T">The event argument type.</typeparam>
    /// <typeparam name="S">The event argument type.</typeparam>
    internal class Event<T,S> : BaseEvent
    {
        private readonly T arg1;
        private readonly S arg2;
        public readonly Handler<T,S> handler;
        public override event OnFinish OnFinish;

        public Event(int targetId, Handler<T,S> handler, long time, T arg1, S arg2)
            : base(targetId, time)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.handler = handler;
        }

        public override void Handle()
        {
            Debug.Assert(handler != null, "Event does not have any handler.");
            handler(arg1, arg2);

            if (OnFinish != null)
                OnFinish();
        }

        public override string ToString()
        {
            return base.ToString() + ", " + arg1 + ", " + arg2;
        }
    }

    /// <summary>
    /// Represents an event that its handler would be determined through reflection.
    /// This type of events should be used with care. Excessive use may bring about
    /// significantly bad performance.
    /// </summary>
    /// <typeparam name="T">The event argument type.</typeparam>
    internal class ReflectionEvent<T> : BaseEvent
	{
		private readonly T arg;
		private readonly object obj;
		private readonly Handler<T> handler;
		public override event OnFinish OnFinish;

		public ReflectionEvent(int targetId, Handler<T> handler, object obj, long time, T arg)
			: base(targetId, time)
		{
			this.arg = arg;
			this.obj = obj;
			this.handler = handler;
		}

		public override void Handle()
		{
			// invokes the strongly-typed handler of the weakly-typed object
			// NOTE: if calling this method raises TargetException, then the handler is not declared or inherited by the class of obj
			handler.Method.Invoke(obj,
				BindingFlags.ExactBinding | BindingFlags.InvokeMethod | BindingFlags.Public,
				null, new object[] { arg }, null);

			if (OnFinish != null)
				OnFinish();
		}
	}
}