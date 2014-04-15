using System;
using MiscUtil.Extensions.TimeRelated;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Extensions.TimeRelated
{
    [TestFixture]
    public class TimeSpanBasedExtTest
    {
        [Test]
        public void Ticks()
        {
            TimeSpan t = 30.Ticks();
            Assert.AreEqual(30, t.Ticks);
        }

        [Test]
        public void Milliseconds()
        {
            TimeSpan t = 30.Milliseconds();
            Assert.AreEqual(30 * TimeSpan.TicksPerMillisecond, t.Ticks);
        }

        [Test]
        public void Seconds()
        {
            TimeSpan t = 30.Seconds();
            Assert.AreEqual(30 * TimeSpan.TicksPerSecond, t.Ticks);
        }

        [Test]
        public void Minutes()
        {
            TimeSpan t = 30.Minutes();
            Assert.AreEqual(30 * TimeSpan.TicksPerMinute, t.Ticks);
        }

        [Test]
        public void Hours()
        {
            TimeSpan t = 30.Hours();
            Assert.AreEqual(30 * TimeSpan.TicksPerHour, t.Ticks);
        }

        [Test]
        public void Days()
        {
            TimeSpan t = 30.Days();
            Assert.AreEqual(30 * TimeSpan.TicksPerDay, t.Ticks);
        }
    }
}
