using System;
using MiscUtil.Linq;
using MiscUtil.Linq.Extensions;
using NUnit.Framework;


namespace MiscUtil.UnitTests.Linq.Extensions
{
    public partial class DataProducerExtTest
    {
        [Test]
        public void Count()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<int> count = subject.Count();
            subject.ProduceAndEnd("a", "b", "c", "d");
            Assert.AreEqual(4, count.Value);
        }
        [Test]
        public void CountWithNulls()
        {
            int?[] data = { 1, null, 4, null, 3, null, 2 };
            DataProducer<int?> subject = new DataProducer<int?>();
            IFuture<int> count = subject.Count();
            subject.ProduceAndEnd(data);
            Assert.AreEqual(data.Length, count.Value);
            Assert.AreEqual(7, count.Value); // to be sure...
        }

        [Test]
        public void CountWithFilter()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<int> count = subject.Count(x => x.Length >= 2);
            subject.ProduceAndEnd("a", "bbb", "cc", "d");
            Assert.AreEqual(2, count.Value);
        }

        [Test]
        public void LongCount()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<long> count = subject.LongCount();
            subject.ProduceAndEnd("a", "b", "c", "d");
            Assert.AreEqual(4L, count.Value);
        }

        [Test]
        public void LongCountWithFilter()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<long> count = subject.LongCount(x => x.Length >= 2);
            subject.ProduceAndEnd("a", "bbb", "cc", "d");
            Assert.AreEqual(2L, count.Value);
        }

        /// <summary>
        /// This test was taken from the MSDN example of Enumerable.Aggregate
        /// </summary>
        [Test]
        public void AggregateNoSeed()
        {
            string sentence = "the quick brown fox jumps over the lazy dog";

            // Split the string into individual words.
            string[] words = sentence.Split(' ');

            DataProducer<string> subject = new DataProducer<string>();

            // Prepend each word to the beginning of the 
            // new sentence to reverse the word order.
            IFuture<string> reversed = subject.Aggregate((workingSentence, next) => next + " " + workingSentence);

            subject.ProduceAndEnd(words);

            Assert.AreEqual("dog lazy the over jumps fox brown quick the", reversed.Value);
        }

        /// <summary>
        /// This test was taken from the MSDN example of Enumerable.Aggregate
        /// </summary>
        [Test]
        public void AggregateWithSeed()
        {
            int[] ints = { 4, 8, 8, 3, 9, 0, 7, 8, 2 };

            DataProducer<int> subject = new DataProducer<int>();

            // Count the even numbers in the array, using a seed value of 0.
            IFuture<int> result = subject.Aggregate
                (0,
                 (total, next) => next % 2 == 0 ? total + 1 : total);

            subject.ProduceAndEnd(ints);
            Assert.AreEqual(6, result.Value);
        }

        /// <summary>
        /// This test was taken from the MSDN example of Enumerable.Aggregate
        /// </summary>
        [Test]
        public void AggregateWithSeedAndTranslation()
        {
            string[] fruits = { "apple", "mango", "orange", "passionfruit", "grape" };

            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> result = subject.Aggregate
                ("banana",
                 (longest, next) => next.Length > longest.Length ? next : longest,
                // Return the final result as an upper case string.
                 fruit => fruit.ToUpper());

            subject.ProduceAndEnd(fruits);
            Assert.AreEqual("PASSIONFRUIT", result.Value);
        }

        [Test]
        public void FirstWithData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> first = subject.First();
            subject.ProduceAndEnd("a", "b", "c", "d");
            Assert.AreEqual("a", first.Value);
        }

        [Test]
        public void FirstWithConditionAndMatchingData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> first = subject.First(x => x[0] > 'b');
            subject.ProduceAndEnd("a", "b", "c", "d");
            Assert.AreEqual("c", first.Value);
        }

        [Test]
        public void LastWithData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> last = subject.Last();
            subject.ProduceAndEnd("a", "b", "c", "d");
            Assert.AreEqual("d", last.Value);
        }

        [Test]
        public void LastWithConditionAndData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> last = subject.Last(x => x[0] < 'c');
            subject.ProduceAndEnd("a", "b", "c", "d");
            Assert.AreEqual("b", last.Value);
        }

        [Test]
        public void FirstWithoutData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.First();
            try
            {
                subject.End();
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void FirstWithConditionAndNoMatchingData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.First(x => x[0] > 'b');
            subject.Produce("a");
            subject.Produce("b");
            try
            {
                subject.End();
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void LastWithoutData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.Last();
            try
            {
                subject.End();
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void LastWithConditionButNoMatchingData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.Last(x => x.Length > 1);
            subject.Produce("x");
            subject.Produce("y");
            try
            {
                subject.End();
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void FirstOrDefaultWithData()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> first = subject.FirstOrDefault();
            subject.ProduceAndEnd(3, 4, 5, 6, 7);
            Assert.AreEqual(3, first.Value);
        }

        [Test]
        public void FirstOrDefaultWithDataAndCondition()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> first = subject.FirstOrDefault(x => x > 5);
            subject.ProduceAndEnd(3, 4, 5, 6, 7);
            Assert.AreEqual(6, first.Value);
        }

        [Test]
        public void LastOrDefaultWithData()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> last = subject.LastOrDefault();
            subject.ProduceAndEnd(3, 4, 5, 6, 7);
            Assert.AreEqual(7, last.Value);
        }

        [Test]
        public void LastOrDefaultWithDataAndCondition()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> last = subject.LastOrDefault(x => x % 2 == 0);
            subject.ProduceAndEnd(3, 4, 5, 6, 7);
            Assert.AreEqual(6, last.Value);
        }

        [Test]
        public void FirstOrDefaultWithoutData()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> first = subject.FirstOrDefault();
            subject.End();
            Assert.AreEqual(0, first.Value);
        }

        [Test]
        public void FirstOrDefaultWithConditionButNoMatchingData()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> first = subject.FirstOrDefault(x => x > 5);
            subject.ProduceAndEnd(3, 4);
            Assert.AreEqual(0, first.Value);
        }


        [Test]
        public void LastOrDefaultWithoutData()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> last = subject.LastOrDefault();
            subject.End();
            Assert.AreEqual(0, last.Value);
        }


        [Test]
        public void LastOrDefaultWithConditionButNoMatchingData()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> first = subject.LastOrDefault(x => x > 5);
            subject.ProduceAndEnd(3, 4);
            Assert.AreEqual(0, first.Value);
        }

        [Test]
        public void SingleNoMatchingData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.Single(x => x.Length == 5);
            subject.Produce("a");
            subject.Produce("b");
            try
            {
                subject.End();
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void SingleNoData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.Single();
            try
            {
                subject.End();
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void SingleWithSingleElement()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> single = subject.Single();
            subject.ProduceAndEnd(5);
            Assert.AreEqual(5, single.Value);
        }

        [Test]
        public void SingleWithSingleMatchingElement()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> single = subject.Single(x => x % 10 == 0);
            subject.ProduceAndEnd(5, 10, 15);
            Assert.AreEqual(10, single.Value);
        }

        [Test]
        public void SingleWithMultipleElements()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.Single();
            subject.Produce("foo");
            try
            {
                subject.Produce("bar");
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void SingleWithMultipleMatchingElements()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.Single(x => x.Length == 3);
            subject.Produce("a");
            subject.Produce("foo");
            try
            {
                subject.Produce("bar");
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void SingleOrDefaultNoData()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> single = subject.SingleOrDefault();
            subject.End();
            Assert.AreEqual(0, single.Value);
        }

        [Test]
        public void SingleOrDefaultNoMatchingData()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> single = subject.SingleOrDefault(x => x > 5);
            subject.ProduceAndEnd(1, 2, 3);
            Assert.AreEqual(0, single.Value);
        }

        [Test]
        public void SingleOrDefaultWithSingleElement()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> single = subject.SingleOrDefault();
            subject.ProduceAndEnd(5);
            Assert.AreEqual(5, single.Value);
        }

        [Test]
        public void SingleOrDefaultWithSingleMatchingElement()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> single = subject.SingleOrDefault(x => x % 10 == 0);
            subject.ProduceAndEnd(5, 10, 15);
            Assert.AreEqual(10, single.Value);
        }

        [Test]
        public void SingleOrDefaultWithMultipleMatchingElements()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.SingleOrDefault(x => x.Length == 3);
            subject.Produce("a");
            subject.Produce("foo");
            subject.Produce("b");
            try
            {
                subject.Produce("bar");
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void SingleOrDefaultWithMultipleElements()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.SingleOrDefault();
            subject.Produce("foo");
            try
            {
                subject.Produce("bar");
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void ElementAtWithinRange()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> result = subject.ElementAt(2);
            subject.ProduceAndEnd("zero", "one", "two", "three", "four");
            Assert.AreEqual("two", result.Value);
        }

        [Test]
        public void ElementAtOutsideRange()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.ElementAt(5);
            subject.Produce("zero");
            subject.Produce("one");
            subject.Produce("two");
            subject.Produce("three");
            try
            {
                subject.End();
                Assert.Fail("Expected exception");
            }
            catch (ArgumentOutOfRangeException)
            {
                // Expected
            }
        }

        [Test]
        public void ElementAtOrDefaultWithinRange()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> result = subject.ElementAtOrDefault(2);
            subject.ProduceAndEnd("zero", "one", "two", "three", "four");
            Assert.AreEqual("two", result.Value);
        }

        [Test]
        public void ElementAtOrDefaultOutsideRange()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> result = subject.ElementAtOrDefault(5);
            subject.ProduceAndEnd("zero", "one", "two", "three", "four");
            Assert.IsNull(result.Value);
        }

        [Test]
        public void AllNoData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<bool> result = subject.All(x => x.Length < 5);
            subject.End();
            Assert.IsTrue(result.Value);
        }

        [Test]
        public void AllReturningTrue()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<bool> result = subject.All(x => x.Length < 5);
            subject.ProduceAndEnd("zero", "one", "two");
            Assert.IsTrue(result.Value);
        }

        [Test]
        public void AllReturningFalse()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<bool> result = subject.All(x => x.Length < 5);
            subject.ProduceAndEnd("zero", "one", "two", "three", "four");
            Assert.IsFalse(result.Value);
        }

        [Test]
        public void AnyNoPredicateNoData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<bool> result = subject.Any();
            subject.End();
            Assert.IsFalse(result.Value);
        }

        [Test]
        public void AnyNoPredicateWithData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<bool> result = subject.Any();
            subject.ProduceAndEnd("zero", "one");
            Assert.IsTrue(result.Value);
        }

        [Test]
        public void AnyWithPredicateNoData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<bool> result = subject.Any(x => x.Length < 5);
            subject.End();
            Assert.IsFalse(result.Value);
        }

        [Test]
        public void AnyWithPredicateWithMatch()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<bool> result = subject.Any(x => x.Length < 5);
            subject.ProduceAndEnd("zero", "one", "two", "three", "four");
            Assert.IsTrue(result.Value);
        }

        [Test]
        public void AnyWithPredicateNoMatch()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<bool> result = subject.Any(x => x.Length == 6);
            subject.ProduceAndEnd("zero", "one", "two", "three", "four");
            Assert.IsFalse(result.Value);
        }

        [Test]
        public void ContainsNoComparerNoMatch()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<bool> result = subject.Contains("FOUR");
            subject.ProduceAndEnd("zero", "one", "two", "three", "four");
            Assert.IsFalse(result.Value);
        }

        [Test]
        public void ContainsNoComparerWithMatch()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<bool> result = subject.Contains("four");
            subject.ProduceAndEnd("zero", "one", "two", "three", "four");
            Assert.IsTrue(result.Value);
        }

        [Test]
        public void ContainsWithComparerNoMatch()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<bool> result = subject.Contains("FOUR", StringComparer.Ordinal);
            subject.ProduceAndEnd("zero", "one", "two", "three", "four");
            Assert.IsFalse(result.Value);
        }

        [Test]
        public void ContainsWithComparerWithMatch()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<bool> result = subject.Contains("FOUR", StringComparer.OrdinalIgnoreCase);
            subject.ProduceAndEnd("zero", "one", "two", "three", "four");
            Assert.IsTrue(result.Value);
        }
    }
}
