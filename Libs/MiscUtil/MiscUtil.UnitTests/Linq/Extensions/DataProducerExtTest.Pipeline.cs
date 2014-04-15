using System;
using System.Collections.Generic;
using System.Linq;
using MiscUtil.Collections;
using MiscUtil.Collections.Extensions;
using MiscUtil.Linq;
using MiscUtil.Linq.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Linq.Extensions
{
    public partial class DataProducerExtTest
    {
        [Test]
        public void DefaultIfEmptyWithData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IDataProducer<string> result = subject.DefaultIfEmpty();

            ProduceAndCheck(subject, result, new[] { "a", "b", "c" }, new[] { "a", "b", "c" });
        }

        [Test]
        public void DefaultIfEmptyWithoutData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IDataProducer<string> result = subject.DefaultIfEmpty();
            ProduceAndCheck(subject, result, new string[0], new string[] { null });
        }

        [Test]
        public void DefaultIfEmptyWithDataAndDefaultReplacement()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IDataProducer<string> result = subject.DefaultIfEmpty("foo");

            ProduceAndCheck(subject, result, new[] { "a", "b", "c" }, new[] { "a", "b", "c" });
        }

        [Test]
        public void DefaultIfEmptyWithoutDataButWithDefaultReplacement()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IDataProducer<string> result = subject.DefaultIfEmpty("foo");
            ProduceAndCheck(subject, result, new string[0], new string[] { "foo" });
        }

        [Test]
        public void Select()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.Select(x => x * 5);

            ProduceAndCheck(subject, result, new[] { 3, 10, 5 }, new[] { 15, 50, 25 });
        }

        [Test]
        public void SelectWithIndex()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.Select((x, index) => x * 5 + index);

            ProduceAndCheck(subject, result, new[] { 3, 10, 5 }, new[] { 15, 51, 27 });
        }

#if DOTNET35
        [Test]
        public void Reverse()
        {
            DataProducer<string> subject = new DataProducer<string>();

            ProduceAndCheck(subject, subject.Reverse(), new[] { "a", "b", "c", "d" }, new[] { "d", "c", "b", "a" });
        }
#endif

        [Test]
        public void WhereNoResults()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IDataProducer<string> result = subject.Where(x => x.Length >= 3);
            List<string> output = new List<string>();
            bool gotEnd = false;
            result.DataProduced += value => output.Add(value);
            result.EndOfData += () => gotEnd = true;
            subject.ProduceAndEnd("a", "b", "c", "d");
            Assert.IsTrue(gotEnd);
            Assert.AreEqual(0, output.Count);
        }

        [Test]
        public void WhereWithResults()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IDataProducer<string> result = subject.Where(x => x.Length >= 3);
            List<string> output = new List<string>();
            bool gotEnd = false;
            result.DataProduced += value => output.Add(value);
            result.EndOfData += () => gotEnd = true;
            subject.ProduceAndEnd("a", "bcd", "ef", "ghi");
            Assert.IsTrue(gotEnd);
            Assert.AreEqual(2, output.Count);
            Assert.AreEqual("bcd", output[0]);
            Assert.AreEqual("ghi", output[1]);
        }

        [Test]
        public void WhereWithCondition()
        {
            DataProducer<int> subject = new DataProducer<int>();

            ProduceAndCheck(subject, subject.Where((x, index) => x > index),
                new[] { 0, 3, 1, 4, 5, 2 }, new[] { 3, 4, 5 });
        }

        [Test]
        public void TakeWithEnoughData()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.Take(4);

            ProduceAndCheck(subject, result, new[] { 3, 10, 5, 7, 2, 8 }, new[] { 3, 10, 5, 7 });
        }

        [Test]
        public void SkipWithEnoughData()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.Skip(4);

            ProduceAndCheck(subject, result, new[] { 3, 10, 5, 7, 2, 8 }, new[] { 2, 8 });
        }

        [Test]
        public void TakeWithInsufficientData()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.Take(4);

            ProduceAndCheck(subject, result, new[] { 3, 10 }, new[] { 3, 10 });
        }

        [Test]
        public void SkipWithInsufficientData()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.Skip(4);

            ProduceAndCheck(subject, result, new[] { 3, 10 }, new int[0]);
        }

        [Test]
        public void TakeWithZeroCount()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.Take(0);

            ProduceAndCheck(subject, result, new[] { 3, 10 }, new int[0]);
        }

        [Test]
        public void SkipWithZeroCount()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.Skip(0);

            ProduceAndCheck(subject, result, new[] { 3, 10, 5, 7, 2, 8 }, new[] { 3, 10, 5, 7, 2, 8 });
        }

        [Test]
        public void SkipWhileAndConditionMetInSequenceWithIndex()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.SkipWhile((x, index) => x > index);

            ProduceAndCheck(subject, result, new[] { 3, 4, 10, 2, 3 }, new[] { 2, 3 });
        }

        [Test]
        public void SkipWhileAndConditionMetInSequence()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.SkipWhile(x => x < 5);

            ProduceAndCheck(subject, result, new[] { 3, 4, 10, 2, 3 }, new[] { 10, 2, 3 });
        }

        [Test]
        public void TakeWhileAndConditionMetInSequence()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.TakeWhile(x => x < 5);

            ProduceAndCheck(subject, result, new[] { 3, 4, 10, 2, 3 }, new[] { 3, 4 });
        }

        [Test]
        public void TakeWhileAndConditionMetInSequenceWithIndex()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.TakeWhile((x, index) => x > index);

            ProduceAndCheck(subject, result, new[] { 3, 4, 10, 2, 3 }, new[] { 3, 4, 10 });
        }

        [Test]
        public void SkipWhileAndConditionNeverMet()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.SkipWhile(x => x > 10);

            ProduceAndCheck(subject, result, new[] { 3, 4, 10, 2, 3 }, new[] { 3, 4, 10, 2, 3 });
        }

        [Test]
        public void SkipWhileAndConditionAlwaysMet()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.SkipWhile(x => x < 100);

            ProduceAndCheck(subject, result, new[] { 3, 4, 10, 2, 3 }, new int[0]);
        }

        [Test]
        public void TakeWhileAndConditionNeverMet()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.TakeWhile(x => x > 10);

            ProduceAndCheck(subject, result, new[] { 3, 4, 10, 2, 3 }, new int[0]);
        }

        [Test]
        public void TakeWhileAndConditionAlwaysMet()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IDataProducer<int> result = subject.TakeWhile(x => x < 100);

            ProduceAndCheck(subject, result, new[] { 3, 4, 10, 2, 3 }, new[] { 3, 4, 10, 2, 3 });
        }

        [Test]
        public void PumpWithBufferingRequired()
        {
            // This will keep track of how many we've consumed already
            int consumed = 0;

            DataProducer<int> producer = new DataProducer<int>();
            IDataProducer<int> ordered = producer.OrderBy(x => x);
            var selected = ordered.Select(x => new { X = x, Consumed = consumed });

            int[] data = new int[] { 1, 0, 4, 2, 3, 5 };

            var results = producer.PumpProduceAndEnd(data, selected);
            int count = 0;
            foreach (var result in results)
            {
                Assert.AreEqual(count, result.X);
                count++;
                // Nothing will be yielded until we end everything, at which point
                // everything will have been buffered internally before we can increment Consumed
                Assert.AreEqual(0, result.Consumed);
                consumed++;
            }
            Assert.AreEqual(count, 6);
        }

        [Test]
        public void PumpWithoutBufferingRequired()
        {
            // This will keep track of how many we've consumed already
            int consumed = 0;

            DataProducer<int> producer = new DataProducer<int>();
            var selected = producer.Select(x => new { X = x, Consumed = consumed });

            int[] data = new int[] { 0, 1, 2, 3, 4, 5 };

            var results = producer.PumpProduceAndEnd(data, selected);
            int count = 0;
            foreach (var result in results)
            {
                Assert.AreEqual(count, result.X);
                // This time there should be only a single item buffer - we consume 
                // each item before the next is produced
                Assert.AreEqual(count, result.Consumed);
                count++;
                consumed++;
            }
            Assert.AreEqual(count, 6);
        }

        [Test]
        public void PumpWithEarlyStop()
        {
            DataProducer<int> producer = new DataProducer<int>();
            IDataProducer<int> take3 = producer.Take(3);
            // This will show us everything produced
            List<int> allResults = producer.ToList();

            var result = producer.PumpProduceAndEnd(new int[] { 1, 2, 3, 4, 5 }, take3);
            Assert.IsTrue(result.SequenceEqual(new int[] { 1, 2, 3 }));
            Assert.IsTrue(allResults.SequenceEqual(new int[] { 1, 2, 3 }));
        }

#if DOTNET35
        [Test]
        public void DistinctWithoutComparer()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IDataProducer<string> result = subject.Distinct();

            ProduceAndCheck(subject, result,
                            new[] { "zero", "one", "two", "one", "TWO", "three" },
                            new[] { "zero", "one", "two", "TWO", "three" });
        }

        [Test]
        public void DistinctWithComparer()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IDataProducer<string> result = subject.Distinct(StringComparer.OrdinalIgnoreCase);

            ProduceAndCheck(subject, result,
                            new[] { "zero", "one", "two", "one", "TWO", "three" },
                            new[] { "zero", "one", "two", "three" });
        }

        [Test]
        public void OrderByTestDescending()
        {
            var data = new DataProducer<Complex>();
            IFuture<Complex> result = data.OrderByDescending(x => x.Imaginary)
                    .ThenByDescending(x => -x.Real).First();

            data.Produce(new Complex(1, 2));
            data.Produce(new Complex(3, 2));
            data.Produce(new Complex(1, 4));
            data.Produce(new Complex(3, 4));
            data.End();
            Assert.AreEqual(new Complex(1, 4), result.Value);
        }

        [Test]
        public void OrderByTestDescendingComparer()
        {
            var comparer = Comparer<decimal>.Default.Reverse();
            var data = new DataProducer<Complex>();
            IFuture<Complex> result = data.OrderByDescending(x => x.Imaginary, comparer)
                    .ThenByDescending(x => -x.Real, comparer)
                    .ThenBy(x => x.Imaginary, Comparer<decimal>.Default)
                    .Last();

            data.Produce(new Complex(1, 2));
            data.Produce(new Complex(3, 2));
            data.Produce(new Complex(1, 4));
            data.Produce(new Complex(3, 4));
            data.End();
            Assert.AreEqual(new Complex(1, 4), result.Value);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void OrderByLinkedComparerNullPrimary()
        {
            new LinkedComparer<int>(null, Comparer<int>.Default);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void OrderByLinkedComparerNullSecondary()
        {
            new LinkedComparer<int>(Comparer<int>.Default, null);
        }

        [Test]
        public void OrderByProjectionComparerNullComparer()
        { // (uses default when null)
            IComparer<int> comparer = new ProjectionComparer<int, int>(x => x, null).Reverse();
            List<int> data = new List<int>(new[] { 2, 4, 1 });
            data.Sort(comparer);
            Assert.AreEqual(data[0], 4);
            Assert.AreEqual(data[1], 2);
            Assert.AreEqual(data[2], 1);
        }

        [Test]
        public void OrderByTest()
        {
            DataProducer<OrderTest> source = new DataProducer<OrderTest>();
            OrderTest[] data = {
                 new OrderTest {Id = 1, Value1 = 3, Value2 = 3},
                 new OrderTest {Id = 2, Value1 = 3, Value2 = 1},
                 new OrderTest {Id = 3, Value1 = 1, Value3 = 1},
                 new OrderTest {Id = 4, Value1 = 2, Value2 = 2}
            };
            FlagUsedComparer<int> val3ComparerProducer = new FlagUsedComparer<int>(),
                val3ComparerEnumerable = new FlagUsedComparer<int>(),
                val3ComparerQueryable = new FlagUsedComparer<int>();

            var queryProducer = source.OrderBy(x => x.Value3, val3ComparerProducer).
                OrderBy(x => x.Value1).ThenBy(x => x.Value2);

            var expectedEnumerable = data.AsEnumerable().OrderBy(x => x.Value3, val3ComparerEnumerable).
                OrderBy(x => x.Value1).ThenBy(x => x.Value2).ToArray();

            var expectedQueryable = data.AsQueryable().OrderBy(x => x.Value3, val3ComparerQueryable).
                OrderBy(x => x.Value1).ThenBy(x => x.Value2).ToArray();

            Assert.IsTrue(expectedEnumerable.SequenceEqual(expectedQueryable), "Baseline query/enum sequence check");
            Assert.IsTrue(val3ComparerEnumerable.Used == val3ComparerQueryable.Used, "Baseline query/enum comparer check");

            ProduceAndCheck<OrderTest>(source, queryProducer, data, expectedEnumerable);
            Assert.AreEqual(val3ComparerEnumerable.Used, val3ComparerProducer.Used, "Inner comparer");
        }

        class OrderTest : IEquatable<OrderTest>
        {
            bool IEquatable<OrderTest>.Equals(OrderTest val)
            {
                return val.Id == Id;
            }
            public int Id { get; set; }
            public int Value1 { get; set; }
            public int Value2 { get; set; }
            public int Value3 { get; set; }
        }

        class FlagUsedComparer<T> : IComparer<T>
        {
            public bool Used { get; private set; }
            public IComparer<T> Comparer { get; private set; }

            public FlagUsedComparer() : this(null) { }
            public FlagUsedComparer(IComparer<T> comparer)
            {
                Comparer = comparer ?? Comparer<T>.Default;
            }
            int IComparer<T>.Compare(T x, T y)
            {
                Used = true;
                return Comparer.Compare(x, y);
            }
        }
#endif
    }
}
