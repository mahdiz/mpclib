using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
	public static class Timex
	{
		public delegate T Action<T>();
		public delegate void Action();

		/// <summary>
		/// Invokes a method and returns its running time in milliseconds.
		/// </summary>
		/// <param name="action">Input method delegate. 
		/// Wrap in an anonymous delegate if the method has parameters, e.g.,
		/// () => Method(a,b,c) </param>
		/// <param name="output">Return value of 'action'.</param>
		/// <returns>Running time of 'action' in milliseconds.</returns>
		public static double Run<T>(Action<T> action, ref T output)
		{
			var start = DateTime.Now;
			output = action();
			return (DateTime.Now - start).TotalMilliseconds;
		}

		/// <summary>
		/// Invokes a method and returns its running time in milliseconds.
		/// </summary>
		/// <param name="action">Input method delegate.</param>
		/// <returns>Running time of 'action' in milliseconds.</returns>
		public static double Run(Action action)
		{
			var start = DateTime.Now;
			action();
			return (DateTime.Now - start).TotalMilliseconds;
		}
	}
}
