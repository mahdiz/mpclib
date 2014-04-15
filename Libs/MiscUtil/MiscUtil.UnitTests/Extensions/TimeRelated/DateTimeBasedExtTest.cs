using System;
using MiscUtil.Extensions.TimeRelated;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Extensions.TimeRelated
{
    [TestFixture]
    public class DateTimeBasedExtTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidDay()
        {
            DateTime d = 29.February(2007);
        }

        [Test]
        public void LeapYear()
        {
            DateTime day = 29.February(2008);
            Assert.AreEqual(2008, day.Year);
            Assert.AreEqual(2, day.Month);
            Assert.AreEqual(29, day.Day);
        }

        [Test]
        public void January()
        {
            DateTime day = 15.January(2008);
            Assert.AreEqual(2008, day.Year);
            Assert.AreEqual(1, day.Month);
            Assert.AreEqual(15, day.Day);
        }

        [Test]
        public void February()
        {
            DateTime day = 15.February(2008);
            Assert.AreEqual(2008, day.Year);
            Assert.AreEqual(2, day.Month);
            Assert.AreEqual(15, day.Day);
        }

        [Test]
        public void March()
        {
            DateTime day = 15.March(2008);
            Assert.AreEqual(2008, day.Year);
            Assert.AreEqual(3, day.Month);
            Assert.AreEqual(15, day.Day);
        }

        [Test]
        public void April()
        {
            DateTime day = 15.April(2008);
            Assert.AreEqual(2008, day.Year);
            Assert.AreEqual(4, day.Month);
            Assert.AreEqual(15, day.Day);
        }

        [Test]
        public void May()
        {
            DateTime day = 15.May(2008);
            Assert.AreEqual(2008, day.Year);
            Assert.AreEqual(5, day.Month);
            Assert.AreEqual(15, day.Day);
        }

        [Test]
        public void June()
        {
            DateTime day = 15.June(2008);
            Assert.AreEqual(2008, day.Year);
            Assert.AreEqual(6, day.Month);
            Assert.AreEqual(15, day.Day);
        }

        [Test]
        public void July()
        {
            DateTime day = 15.July(2008);
            Assert.AreEqual(2008, day.Year);
            Assert.AreEqual(7, day.Month);
            Assert.AreEqual(15, day.Day);
        }

        [Test]
        public void August()
        {
            DateTime day = 15.August(2008);
            Assert.AreEqual(2008, day.Year);
            Assert.AreEqual(8, day.Month);
            Assert.AreEqual(15, day.Day);
        }

        [Test]
        public void September()
        {
            DateTime day = 15.September(2008);
            Assert.AreEqual(2008, day.Year);
            Assert.AreEqual(9, day.Month);
            Assert.AreEqual(15, day.Day);
        }

        [Test]
        public void October()
        {
            DateTime day = 15.October(2008);
            Assert.AreEqual(2008, day.Year);
            Assert.AreEqual(10, day.Month);
            Assert.AreEqual(15, day.Day);
        }

        [Test]
        public void November()
        {
            DateTime day = 15.November(2008);
            Assert.AreEqual(2008, day.Year);
            Assert.AreEqual(11, day.Month);
            Assert.AreEqual(15, day.Day);
        }

        [Test]
        public void December()
        {
            DateTime day = 15.December(2008);
            Assert.AreEqual(2008, day.Year);
            Assert.AreEqual(12, day.Month);
            Assert.AreEqual(15, day.Day);
        }
    }
}
