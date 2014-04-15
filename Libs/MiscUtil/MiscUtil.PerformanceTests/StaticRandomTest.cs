using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace MiscUtil.UnitTests
{
	/// <summary>
	/// Tests for MiscUtil.StaticRandom
	/// </summary>
	[TestFixture]
	public class StaticRandomTest
	{
        const int Size = 10000000;
        const int ThreadCount = 20;

		/// <summary>
		/// Test a load of threads grabbing a load of numbers.
		/// This test should never actually fail; it will just display how
		/// long it took to run.
		/// </summary>
		[Test]
		public void MultipleThreads()
		{
            Stopwatch sw = Stopwatch.StartNew();
			Thread[] threads = new Thread[ThreadCount];
			for (int i=0; i < threads.Length; i++)
			{
				threads[i] = new Thread(new ThreadStart(new RandomGrabber(Size, false).GrabNumbers));
				threads[i].Start();
			}
			for (int i=0; i < threads.Length; i++)
			{
				threads[i].Join();
			}
            sw.Stop();
			Console.WriteLine ("{0} threads grabbing {1} numbers from StaticRandom each took {2}ms", 
				               ThreadCount, Size, (int)sw.ElapsedMilliseconds);
			Console.WriteLine();
		}

        [Test]
        public void SingleThread()
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < ThreadCount*Size; i++)
            {
                StaticRandom.Next(100000);
            }
            sw.Stop();
            Console.WriteLine("Single thread grabbing {0} numbers from StaticRandom took {1}ms",
                               ThreadCount*Size, (int)sw.ElapsedMilliseconds);
            Console.WriteLine();
        }

        [Test]
        public void SingleThreadNotUsingStaticRandom()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Random rng = new Random();
            for (int i = 0; i < ThreadCount * Size; i++)
            {
                rng.Next(100000);
            }
            sw.Stop();
            Console.WriteLine("Single thread grabbing {0} numbers from separate Random took {1}ms",
                               ThreadCount * Size, (int)sw.ElapsedMilliseconds);
            Console.WriteLine();
        }
        
        /// <summary>
        /// Same test as above, but with each thread using its own RNG instead
        /// of StaticRandom.
        /// </summary>
        [Test]
        public void MultipleThreadsNotUsingStaticRandom()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Thread[] threads = new Thread[ThreadCount];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    Random rng = new Random();
                    for (int x=0; x < Size; x++)
                    {
                        rng.Next(100000);
                    }
                });
                threads[i].Start();
            }
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }
            sw.Stop();
            Console.WriteLine("{0} threads grabbing {1} numbers from individual Random instances each took {2}ms",
                               ThreadCount, Size, (int)sw.ElapsedMilliseconds);
            Console.WriteLine();
        }
	}
}
