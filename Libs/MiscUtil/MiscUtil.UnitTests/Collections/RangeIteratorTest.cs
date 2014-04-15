using System;
using System.Linq;
using MiscUtil.Collections;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Collections
{
    [TestFixture]
    public class RangeIteratorTest
    {
        [Test]
        public void InclusiveRange()
        {
            var subject = new RangeIterator<int>(new Range<int>(0, 5), x => x+1);
            Assert.IsTrue(subject.SequenceEqual(new[] { 0, 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void RangeExcludingStart()
        {
            var subject = new RangeIterator<int>(new Range<int>(0, 5).ExcludeStart(), x => x + 1);
            Assert.IsTrue(subject.SequenceEqual(new[] { 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void RangeExcludingEnd()
        {
            var subject = new RangeIterator<int>(new Range<int>(0, 5).ExcludeEnd(), x => x + 1);
            Assert.IsTrue(subject.SequenceEqual(new[] { 0, 1, 2, 3, 4 }));
        }

        [Test]
        public void RangeExcludingBothEnds()
        {
            var subject = new RangeIterator<int>(new Range<int>(0, 5).ExcludeStart().ExcludeEnd(), x => x + 1);
            Assert.IsTrue(subject.SequenceEqual(new[] { 1, 2, 3, 4 }));
        }

#if DOTNET35
        [Test]
        public void HalfOpenRangeSinglePointIsEmpty()
        {
            var subject = new Range<int>(3, 3).ExcludeEnd().UpBy(1);
            Assert.AreEqual(0, subject.Count());
            subject = new Range<int>(3, 3).ExcludeStart().UpBy(1);
            Assert.AreEqual(0, subject.Count());
            subject = new Range<int>(3, 3).ExcludeEnd().DownBy(1);
            Assert.AreEqual(0, subject.Count());
            subject = new Range<int>(3, 3).ExcludeStart().DownBy(1);
            Assert.AreEqual(0, subject.Count());
        }

        [Test]
        public void SinglePointInclusiveRangeYieldsSingleValue()
        {
            var subject = new Range<int>(3, 3).UpBy(1);
            Assert.IsTrue(subject.SequenceEqual(new[] { 3 }));
        }
#endif

        [Test]
        public void DescendingInclusiveRange()
        {
            var subject = new RangeIterator<int>(new Range<int>(0, 5), x => x - 1, false);
            Assert.IsTrue(subject.SequenceEqual(new[] { 5, 4, 3, 2, 1, 0 }));
        }

        [Test]
        public void DescendingRangeExcludingStart()
        {
            var subject = new RangeIterator<int>(new Range<int>(0, 5).ExcludeStart(), x => x - 1, false);
            Assert.IsTrue(subject.SequenceEqual(new[] { 5, 4, 3, 2, 1 }));
        }

        [Test]
        public void DescendingRangeExcludingEnd()
        {
            var subject = new RangeIterator<int>(new Range<int>(0, 5).ExcludeEnd(), x => x - 1, false);
            Assert.IsTrue(subject.SequenceEqual(new[] { 4, 3, 2, 1, 0 }));
        }

        [Test]
        public void DescendingRangeExcludingBothEnds()
        {
            var subject = new RangeIterator<int>(new Range<int>(0, 5).ExcludeStart().ExcludeEnd(), x => x - 1, false);
            Assert.IsTrue(subject.SequenceEqual(new[] { 4, 3, 2, 1 }));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void StepWrongWayThrows()
        {
            new RangeIterator<int>(new Range<int>(0, 5), x => x - 1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void NoOpStepThrows()
        {
            new RangeIterator<int>(new Range<int>(0, 5), x => x);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullStepThrows()
        {
            new RangeIterator<int>(new Range<int>(0, 5), null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void DescendingStepWrongWayThrows()
        {
            new RangeIterator<int>(new Range<int>(0, 5), x => x+1, false);
        }
    }
}
