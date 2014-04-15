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
		/// <summary>
		/// Check that if you start several threads at the same time,
		/// they all get different sequences.
		/// </summary>
		[Test]
		public void CheckDifferentSources()
        {
			RandomGrabber[] grabbers = new RandomGrabber[100];
			Thread[] threads = new Thread[grabbers.Length];

			for (int i=0; i < grabbers.Length; i++)
			{
				grabbers[i] = new RandomGrabber(30, true);
				threads[i] = new Thread(new ThreadStart(grabbers[i].GrabNumbers));
			}

			for (int i=0; i < grabbers.Length; i++)
			{
				threads[i].Start();
			}
			for (int i=0; i < grabbers.Length; i++)
			{
				threads[i].Join();
			}
			for (int i=0; i < grabbers.Length-1; i++)
			{
				for (int j=i+1; j < grabbers.Length; j++)
				{
					if (grabbers[i].Equals(grabbers[j]))
					{
						Assert.Fail("Duplicate code sequences retrieved");
					}
				}
			}
		}

        [Test]
        public void NextDoubleShouldNotAlwaysReturnInts()
        {
            // We might get a double like 1.0 *once*, but we're unlikely
            // to get 10 of them unless there's a bug (like the one
            // prompting this test)
            for (int i = 0; i < 10; i++)
            {
                double d = StaticRandom.NextDouble();
                if ((int)d != d)
                {
                    return;
                }
            }
            Assert.Fail("NextDouble shouldn't just return ints");
        }
    }
}
