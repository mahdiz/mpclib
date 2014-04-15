using System;
using System.Linq;
using MiscUtil.Collections;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Collections
{
    [TestFixture]
    public class RangeTest
    {
        [Test]
        public void CustomComparer()
        {
            var subject = new Range<string>("a", "e", StringComparer.Ordinal);
            Assert.IsFalse(subject.Contains("B"));
            Assert.IsTrue(subject.Contains("b"));

            subject = new Range<string>("a", "e", StringComparer.OrdinalIgnoreCase);
            Assert.IsTrue(subject.Contains("B"));
            Assert.IsTrue(subject.Contains("A"));
            Assert.IsTrue(subject.Contains("E"));
            Assert.IsFalse(subject.Contains("F"));
        }

        [Test]
        public void CustomComparerExcludingEnd()
        {
            var subject = new Range<string>("a", "e", StringComparer.OrdinalIgnoreCase).ExcludeEnd();
            Assert.IsTrue(subject.Contains("A"));
            Assert.IsFalse(subject.Contains("E"));
        }

        [Test]
        public void CustomComparerExcludingStart()
        {
            var subject = new Range<string>("a", "e", StringComparer.OrdinalIgnoreCase).ExcludeStart();
            Assert.IsTrue(subject.Contains("E"));
            Assert.IsFalse(subject.Contains("A"));
        }

        [Test]
        public void DefaultComparer()
        {
            var subject = new Range<int>(0, 5);
            Assert.IsTrue(subject.IncludesStart);
            Assert.IsTrue(subject.IncludesEnd);
            Assert.IsTrue(subject.Contains(0));
            Assert.IsTrue(subject.Contains(1));
            Assert.IsTrue(subject.Contains(5));
            Assert.IsFalse(subject.Contains(-1));
            Assert.IsFalse(subject.Contains(6));
        }

        [Test]
        public void DefaultComparerExcludingEnd()
        {
            var subject = new Range<int>(0, 5).ExcludeEnd();
            Assert.IsTrue(subject.IncludesStart);
            Assert.IsFalse(subject.IncludesEnd);
            Assert.IsTrue(subject.Contains(0));
            Assert.IsTrue(subject.Contains(1));
            Assert.IsFalse(subject.Contains(5));
        }

        [Test]
        public void DefaultComparerExcludingStart()
        {
            var subject = new Range<int>(0, 5).ExcludeStart();
            Assert.IsFalse(subject.IncludesStart);
            Assert.IsTrue(subject.IncludesEnd);
            Assert.IsFalse(subject.Contains(0));
            Assert.IsTrue(subject.Contains(1));
            Assert.IsTrue(subject.Contains(5));
        }

        [Test]
        public void ExcludeBothEnds()
        {
            var subject = new Range<int>(0, 5).ExcludeStart().ExcludeEnd();
            Assert.IsFalse(subject.IncludesStart);
            Assert.IsFalse(subject.IncludesEnd);
            Assert.IsFalse(subject.Contains(0));
            Assert.IsTrue(subject.Contains(1));
            Assert.IsFalse(subject.Contains(5));
        }


        [Test]
        public void ExcludeThenIncludeStart()
        {
            var subject = new Range<int>(0, 5);
            subject = subject.ExcludeStart();
            Assert.IsFalse(subject.IncludesStart);
            subject = subject.IncludeStart();
            Assert.IsTrue(subject.IncludesStart);
        }

        [Test]
        public void ExcludeThenIncludeEnd()
        {
            var subject = new Range<int>(0, 5);
            subject = subject.ExcludeEnd();
            Assert.IsFalse(subject.IncludesEnd);
            subject = subject.IncludeEnd();
            Assert.IsTrue(subject.IncludesEnd);
        }

        [Test]
        public void IncludeStartOnInclusiveStart()
        {
            var subject = new Range<int>(0, 5);
            Assert.AreSame(subject, subject.IncludeStart());
        }

        [Test]
        public void IncludeEndOnInclusiveEnd()
        {
            var subject = new Range<int>(0, 5);
            Assert.AreSame(subject, subject.IncludeEnd());
        }

        [Test]
        public void ExcludeStartOnExclusiveStart()
        {
            var subject = new Range<int>(0, 5).ExcludeStart();
            Assert.AreSame(subject, subject.ExcludeStart());
        }

        [Test]
        public void ExcludeEndOnExclusiveEnd()
        {
            var subject = new Range<int>(0, 5).ExcludeEnd();
            Assert.AreSame(subject, subject.ExcludeEnd());
        }

        [Test]
        public void HalfOpenSamePointIsEmpty()
        {
            var subject = new Range<int>(3, 3).ExcludeEnd();
            Assert.IsFalse(subject.Contains(3));
            subject = new Range<int>(3, 3).ExcludeStart();
            Assert.IsFalse(subject.Contains(3));
        }
        
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void StartHigherThanEndThrows()
        {
            new Range<int>(5, 0);
        }

        [Test]
        public void Ascending()
        {
            var subject = new Range<int>(0, 5).FromStart(x => x + 2);
            Assert.IsTrue(subject.SequenceEqual(new int[] { 0, 2, 4 }));
        }

        [Test]
        public void Descending()
        {
            var subject = new Range<int>(0, 5).FromEnd(x => x - 2);
            Assert.IsTrue(subject.SequenceEqual(new int[] { 5, 3, 1 }));
        }

#if DOTNET35
        [Test]
        public void UpBy()
        {
            var subject = new Range<int>(0, 5).UpBy(2);
            Assert.IsTrue(subject.SequenceEqual(new int[] { 0, 2, 4 }));
        }

        [Test]
        public void DownBy()
        {
            var subject = new Range<int>(0, 5).DownBy(2);
            Assert.IsTrue(subject.SequenceEqual(new int[] { 5, 3, 1 }));
        }

        [Test]
        public void UpByAlternative()
        {
            var subject = new Range<DateTime>(new DateTime(2000, 1, 1), new DateTime(2000, 1, 5)).UpBy(TimeSpan.FromDays(1));
            Assert.IsTrue(subject.SequenceEqual
                (new[] { new DateTime(2000, 1, 1), 
                          new DateTime(2000, 1, 2), 
                          new DateTime(2000, 1, 3), 
                          new DateTime(2000, 1, 4), 
                          new DateTime(2000, 1, 5)}));
        }

        [Test]
        public void DownByAlternative()
        {
            var subject = new Range<DateTime>(new DateTime(2000, 1, 1), new DateTime(2000, 1, 5)).DownBy(TimeSpan.FromDays(1));
            Assert.IsTrue(subject.SequenceEqual
                (new[] { new DateTime(2000, 1, 5), 
                          new DateTime(2000, 1, 4), 
                          new DateTime(2000, 1, 3), 
                          new DateTime(2000, 1, 2), 
                          new DateTime(2000, 1, 1)}));
        }
#endif

        [Test]
        public void StepAscending()
        {
            var subject = new Range<int>(0, 5).Step(x => x + 1);
            Assert.IsTrue(subject.SequenceEqual(new [] { 0, 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void StepDescending()
        {
            var subject = new Range<int>(0, 5).Step(x => x - 1);
            Assert.IsTrue(subject.SequenceEqual(new [] { 5, 4, 3, 2, 1, 0 }));
        }

#if DOTNET35
        [Test]
        public void StepAmountAscending()
        {
            var subject = new Range<int>(0, 5).Step(1);
            Assert.IsTrue(subject.SequenceEqual(new [] { 0, 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void StepAmountDescending()
        {
            var subject = new Range<int>(0, 5).Step(-1);
            Assert.IsTrue(subject.SequenceEqual(new [] { 5, 4, 3, 2, 1, 0 }));
        }

        [Test]
        public void StepAlternativeAmountAscending()
        {
            var subject = new Range<DateTime>(new DateTime(2000, 1, 1), new DateTime(2000, 1, 5)).Step(TimeSpan.FromDays(1));
            Assert.IsTrue(subject.SequenceEqual
                (new [] { new DateTime(2000, 1, 1), 
                          new DateTime(2000, 1, 2), 
                          new DateTime(2000, 1, 3), 
                          new DateTime(2000, 1, 4), 
                          new DateTime(2000, 1, 5)}));
        }

        [Test]
        public void StepAlternativeAmountDescending()
        {
            var subject = new Range<DateTime>(new DateTime(2000, 1, 1), new DateTime(2000, 1, 5)).Step(TimeSpan.FromDays(-1));
            Assert.IsTrue(subject.SequenceEqual
                (new[] { new DateTime(2000, 1, 5), 
                          new DateTime(2000, 1, 4), 
                          new DateTime(2000, 1, 3), 
                          new DateTime(2000, 1, 2), 
                          new DateTime(2000, 1, 1)}));
        }
#endif
    }
}
