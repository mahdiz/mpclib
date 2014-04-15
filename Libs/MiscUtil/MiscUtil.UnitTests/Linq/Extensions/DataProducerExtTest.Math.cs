#if DOTNET35
using System;
using System.Collections.Generic;
using System.Linq;
using MiscUtil.Linq;
using MiscUtil.Linq.Extensions;
using NUnit.Framework;


namespace MiscUtil.UnitTests.Linq.Extensions
{
    public partial class DataProducerExtTest
    {
        [Test]
        public void SumWithNoProjection()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> sum = subject.Sum();
            subject.ProduceAndEnd(1, 2, 3, 4);
            Assert.AreEqual(10, sum.Value);
        }

        [Test]
        public void SumWithProjection()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<int> sum = subject.Sum(x => x.Length);
            subject.ProduceAndEnd("first", "second", "third");
            Assert.AreEqual(16, sum.Value);
        }

        [Test]
        public void MaxInt32NoProjection()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> max = subject.Max();
            subject.ProduceAndEnd(10, 20, 15, 25, 10);
            Assert.AreEqual(25, max.Value);
        }

        [Test]
        public void MaxInt32WithProjection()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> max = subject.Max(x => -x);
            subject.ProduceAndEnd(10, 20, 15, 25, 10);
            // Result is the max of the projection, not the projection of the max
            Assert.AreEqual(-10, max.Value);
        }

        [Test]
        public void MaxInt32NoProjectionNoElements()
        {
            DataProducer<int> subject = new DataProducer<int>();
            subject.Max();
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
        public void MaxInt32WithProjectionNoElements()
        {
            DataProducer<int> subject = new DataProducer<int>();
            subject.Max(x => -x);
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
        public void MaxNullableInt32WithProjectionNoElements()
        {
            DataProducer<int?> subject = new DataProducer<int?>();
            IFuture<int?> result = subject.Max(x => -x);
            subject.End();
            Assert.IsNull(result.Value);
        }

        [Test]
        public void MaxStringNoProjection()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> max = subject.Max();
            subject.ProduceAndEnd("apple", "pear", "banana", "passionfruit", "kiwi");

            Assert.AreEqual("pear", max.Value);
        }

        [Test]
        public void MaxStringWithProjection()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<int> max = subject.Max(x => x == null ? 999 : x.Length);
            subject.ProduceAndEnd("apple", "pear", "banana", "passionfruit", "kiwi");

            Assert.AreEqual(12, max.Value);
        }

        [Test]
        public void MaxStringNoProjectionWithAllNulls()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> max = subject.Max();
            subject.ProduceAndEnd(null, null, null);

            Assert.IsNull(max.Value);
        }

        [Test]
        public void MaxStringNoProjectionNoData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> result = subject.Max();
            subject.End();

            Assert.IsNull(result.Value);
        }

        [Test]
        public void MaxStringWithProjectionWithAllNulls()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<int> max = subject.Max(x => x == null ? 999 : x.Length);
            subject.ProduceAndEnd(null, null, null);

            Assert.AreEqual(999, max.Value);
        }

        [Test]
        public void MaxStringWithProjectionToInt32NoData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.Max(x => x == null ? 999 : x.Length);
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
        public void MaxStringWithProjectionToStringNoData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> result = subject.Max(x => x + x);
            subject.End();
            Assert.IsNull(result.Value);
        }

        [Test]
        public void MaxNullableInt32NoProjectionWithData()
        {
            DataProducer<int?> subject = new DataProducer<int?>();
            IFuture<int?> max = subject.Max();

            subject.ProduceAndEnd(10, 20, null, 15, 25, 10);

            Assert.AreEqual(25, max.Value.Value);
        }

        [Test]
        public void MaxNullableInt32NoProjectionAllNulls()
        {
            DataProducer<int?> subject = new DataProducer<int?>();
            IFuture<int?> max = subject.Max();

            subject.ProduceAndEnd(null, null, null);

            Assert.IsNull(max.Value);
        }

        [Test]
        public void MaxNullableInt32NoProjectionStartsNull()
        {
            Aggregate<int?, int?>(x => Enumerable.Max(x),
                x => DataProducerExt.Max(x),
                null, 1, 2);
        }

        [Test]
        public void MaxNullableInt32NoProjectionEmptySequence()
        {
            DataProducer<int?> subject = new DataProducer<int?>();
            IFuture<int?> max = subject.Max();
            subject.End();

            Assert.IsNull(max.Value);
        }

        [Test]
        public void MinInt32NoProjection()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> min = subject.Min();
            subject.ProduceAndEnd(10, 20, 15, 25, 10);
            Assert.AreEqual(10, min.Value);
        }

        [Test]
        public void MinInt32WithProjection()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IFuture<int> min = subject.Min(x => -x);
            subject.ProduceAndEnd(10, 20, 15, 25, 10);
            // Result is the Min of the projection, not the projection of the Min
            Assert.AreEqual(-25, min.Value);
        }

        [Test]
        public void MinInt32NoProjectionNoElements()
        {
            DataProducer<int> subject = new DataProducer<int>();
            subject.Min();
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
        public void MinInt32WithProjectionNoElements()
        {
            DataProducer<int> subject = new DataProducer<int>();
            subject.Min(x => -x);
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
        public void MinNullableInt32WithProjectionNoElements()
        {
            DataProducer<int?> subject = new DataProducer<int?>();
            IFuture<int?> result = subject.Min(x => -x);
            subject.End();
            Assert.IsNull(result.Value);
        }

        [Test]
        public void MinStringNoProjection()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> min = subject.Min();
            subject.ProduceAndEnd("apple", "pear", "banana", "passionfruit", "kiwi");

            Assert.AreEqual("apple", min.Value);
        }

        [Test]
        public void MinStringWithProjection()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<int> min = subject.Min(x => x == null ? 999 : x.Length);
            subject.ProduceAndEnd("apple", "pear", "banana", "passionfruit", "kiwi");

            Assert.AreEqual(4, min.Value);
        }

        [Test]
        public void MinStringNoProjectionWithAllNulls()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> min = subject.Min();
            subject.ProduceAndEnd(null, null, null);

            Assert.IsNull(min.Value);
        }

        [Test]
        public void MinStringNoProjectionNoData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> result = subject.Min();
            subject.End();

            Assert.IsNull(result.Value);
        }

        [Test]
        public void MinStringWithProjectionWithAllNulls()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<int> min = subject.Min(x => x == null ? 999 : x.Length);
            subject.ProduceAndEnd(null, null, null);

            Assert.AreEqual(999, min.Value);
        }

        [Test]
        public void MinStringWithProjectionToInt32NoData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.Min(x => x == null ? 999 : x.Length);
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
        public void MinStringWithProjectionToStringNoData()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<string> result = subject.Min(x => x + x);
            subject.End();
            Assert.IsNull(result.Value);
        }

        [Test]
        public void MinNullableInt32NoProjectionWithData()
        {
            DataProducer<int?> subject = new DataProducer<int?>();
            IFuture<int?> min = subject.Min();

            subject.ProduceAndEnd(10, 20, null, 15, 25, 10);

            Assert.AreEqual(10, min.Value.Value);
        }

        [Test]
        public void MinNullableInt32NoProjectionAllNulls()
        {
            DataProducer<int?> subject = new DataProducer<int?>();
            IFuture<int?> min = subject.Min();

            subject.ProduceAndEnd(null, null, null);

            Assert.IsNull(min.Value);
        }
        [Test]
        public void MinNullableInt32NoProjectionStartsNull()
        {
            // test against typed Enumerable form
            Aggregate<int?, int?>(x => Enumerable.Min(x),
                x => DataProducerExt.Min(x),
                null, 1, 2);
        }

        [Test]
        public void MinNullableInt32NoProjectionEmptySequence()
        {
            DataProducer<int?> subject = new DataProducer<int?>();
            IFuture<int?> min = subject.Min();
            subject.End();

            Assert.IsNull(min.Value);
        }

        [Test]
        public void SumEmpty()
        {
            Aggregate<int?, int?>(Enumerable.Sum, DataProducerExt.Sum);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void SumNullSource()
        {
            IFuture<string> sum = NullDataProducer.Sum();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void SumProjectedNullSource()
        {
            IFuture<int> sum = NullDataProducer.Sum(x => int.Parse(x));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void SumNullProjection()
        {
            IDataProducer<string> prod = new DataProducer<string>();
            Func<string, int> projection = null;
            IFuture<int> sum = prod.Sum(projection);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AverageNullSource()
        {
            IFuture<string> avg = NullDataProducer.Average();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AverageProjectedNullSource()
        {
            IFuture<double> avg = NullDataProducer.Average(x => int.Parse(x));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AverageNullInt32Projection()
        {
            IDataProducer<string> prod = new DataProducer<string>();
            Func<string, int> projection = null;
            IFuture<double> sum = prod.Average(projection);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AverageNullNullableInt32Projection()
        {
            IDataProducer<string> prod = new DataProducer<string>();
            Func<string, int?> projection = null;
            IFuture<double?> sum = prod.Average(projection);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AverageNullInt64Projection()
        {
            IDataProducer<string> prod = new DataProducer<string>();
            Func<string, long> projection = null;
            IFuture<double> sum = prod.Average(projection);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AverageNullNullableInt64Projection()
        {
            IDataProducer<string> prod = new DataProducer<string>();
            Func<string, long?> projection = null;
            IFuture<double?> sum = prod.Average(projection);
        }

        [Test]
        public void AverageInt64()
        {
            var data = new DataProducer<long>();
            IFuture<double> result = data.Average();
            data.ProduceAndEnd(1, 2, 3, 4, 5);
            Assert.AreEqual(3M, result.Value);
        }

        [Test]
        public void AverageComplex()
        {
            var data = new DataProducer<Complex>();
            IFuture<Complex> result = data.Average();
            data.Produce(new Complex(1, 3));
            data.Produce(new Complex(3, 1));
            data.End();
            Assert.AreEqual(new Complex(2, 2), result.Value);
        }

        [Test]
        public void AverageNullableInt64()
        {
            var data = new DataProducer<long?>();
            IFuture<double?> result = data.Average();
            data.ProduceAndEnd(1, 2, null, 3, 4, null, 5);
            Assert.AreEqual(3M, result.Value);
        }
        [Test]
        public void AverageInt64Projection()
        {
            var data = new DataProducer<long>();
            IFuture<double> result = data.Average(x => -2 * x);
            data.ProduceAndEnd(1, 2, 3, 4, 5);
            Assert.AreEqual(-6M, result.Value);
        }
        [Test]
        public void AverageNullableInt64Projection()
        {
            var data = new DataProducer<long?>();
            IFuture<double?> result = data.Average(x => -2 * x);
            data.ProduceAndEnd(1, 2, null, 3, 4, null, 5);
            Assert.AreEqual(-6M, result.Value);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AverageNullDoubleProjection() // tests "the rest"
        {
            IDataProducer<string> prod = new DataProducer<string>();
            Func<string, double> projection = null;
            IFuture<double> sum = prod.Average(projection);
        }


        [Test]
        public void SumNull()
        {
            Aggregate<int?, int?>(Enumerable.Sum, DataProducerExt.Sum, null, null);
        }

        [Test]
        public void SumData()
        {
            Aggregate<int?, int?>(Enumerable.Sum, DataProducerExt.Sum, 1, 2, 3, 4, 5);
        }

        [Test]
        public void SumDataWithNull()
        {
            Aggregate<int?, int?>(Enumerable.Sum, DataProducerExt.Sum, 1, null, 3, null, 5);
        }

        [Test]
        public void SumProjectionEmpty()
        {
            Aggregate<Wrap<int>, int>(
                x => Enumerable.Sum(x, y => y.Value),
                x => DataProducerExt.Sum(x, y => y.Value));

        }
        [Test]
        public void SumProjectionDenseData()
        {
            Aggregate<Wrap<int>, int>(
                x => Enumerable.Sum(x, y => y.Value),
                x => DataProducerExt.Sum(x, y => y.Value),
                new Wrap<int>(5), new Wrap<int>(2), new Wrap<int>(1));

        }
        [Test]
        public void SumProjectionDenseDataWithNulls()
        {
            Aggregate<Wrap<int?>, int?>(
                x => Enumerable.Sum(x, y => y.Value),
                x => DataProducerExt.Sum(x, y => y.Value),
                new Wrap<int?>(5), new Wrap<int?>(null), new Wrap<int?>(1));
        }

        [Test]
        public void SumProjectionSparseData()
        {
            Aggregate<Wrap<int>, int>(
                x => Enumerable.Sum(x, y => y.Value),
                x => DataProducerExt.Sum(x, y => y.Value),
                new Wrap<int>(5), null, new Wrap<int>(1));
        }

        [Test]
        public void SumProjectionSparseNullableData()
        {
            Aggregate<Wrap<int?>, int?>(
                x => Enumerable.Sum(x, y => y.Value),
                x => DataProducerExt.Sum(x, y => y.Value),
                new Wrap<int?>(5), null, new Wrap<int?>(1));
        }

        [Test]
        public void AverageEmpty()
        {
            Aggregate<int?, double?>(Enumerable.Average, DataProducerExt.Average);
        }

        [Test]
        public void AverageNull()
        {
            Aggregate<int?, double?>(Enumerable.Average, DataProducerExt.Average, null, null);
        }

        [Test]
        public void AverageData()
        {
            Aggregate<int?, double?>(Enumerable.Average, DataProducerExt.Average, 1, 2, 3, 4, 5);
        }

        [Test]
        public void AverageDataNonNullEmpty()
        {
            Aggregate<int, double>(Enumerable.Average, DataProducerExt.Average);
        }

        [Test]
        public void AverageDataNonNullWithData()
        {
            Aggregate<int, double>(Enumerable.Average, DataProducerExt.Average, 1, 2, 3, 4, 5);
        }

        [Test]
        public void AverageDataWithNull()
        {
            Aggregate<int?, double?>(Enumerable.Average, DataProducerExt.Average, 1, null, 3, null, 5);
        }

        [Test]
        public void AverageProjectionEmpty()
        {
            Aggregate<Wrap<int>, double>(
                x => Enumerable.Average(x, y => y.Value),
                x => DataProducerExt.Average(x, y => y.Value));

        }

        [Test]
        public void SumInference()
        {
            Assert.IsTrue(
                new List<OrderTest>().GroupWithPipeline(
                    x => x.Value1,
                    y => y.Sum(z => z.Value2))
                    is IEnumerable<KeyValueTuple<int, int>>);
        }

        [Test]
        public void SumSelectorInference()
        {
            Assert.IsTrue(
                new List<OrderTest>().GroupWithPipeline(
                    x => x.Value1,
                    y => y.Select(z => z.Value2).Sum())
                    is IEnumerable<KeyValueTuple<int, int>>);
        }

        [Test]
        public void AverageInference()
        {
            Assert.IsTrue(
                new List<OrderTest>().GroupWithPipeline(
                    x => x.Value1,
                    y => y.Average(z => z.Value2))
                    is IEnumerable<KeyValueTuple<int, double>>);

        }

        [Test]
        public void AverageSelectorInference()
        {
            Assert.IsTrue(
                new List<OrderTest>().GroupWithPipeline(
                    x => x.Value1,
                    y => y.Select(z => z.Value2).Average())
                    is IEnumerable<KeyValueTuple<int, double>>);
        }

        [Test]
        public void AverageSelectorInference2()
        {
            Assert.IsTrue(
                new List<decimal>().GroupWithPipeline(
                    x => x,
                    y => y.Average())
                    is IEnumerable<KeyValueTuple<decimal, decimal>>);
        }

        [Test]
        public void AverageProjectionDenseData()
        {
            Aggregate<Wrap<int>, double>(
                x => Enumerable.Average(x, y => y.Value),
                x => DataProducerExt.Average(x, y => y.Value),
                new Wrap<int>(5), new Wrap<int>(2), new Wrap<int>(1));
        }

        [Test]
        public void AverageProjectionDenseDataWithNulls()
        {
            Aggregate<Wrap<int?>, double?>(
                x => Enumerable.Average(x, y => y.Value),
                x => DataProducerExt.Average(x, y => y.Value),
                new Wrap<int?>(5), new Wrap<int?>(null), new Wrap<int?>(1));
        }

        [Test]
        public void AverageProjectionSparseData()
        {
            Aggregate<Wrap<int>, double>(
                x => Enumerable.Average(x, y => y.Value),
                x => DataProducerExt.Average(x, y => y.Value),
                new Wrap<int>(5), null, new Wrap<int>(1));
        }

        private static void Aggregate<TData, TResult>(
            Func<IEnumerable<TData>, TResult> baseLine,
            Func<IDataProducer<TData>, IFuture<TResult>> test,
            params TData[] data)
        {
            if (baseLine == null) throw new ArgumentNullException("baseLine");
            if (test == null) throw new ArgumentNullException("test");
            if (data == null) throw new ArgumentNullException("data");

            GenericTest("Aggregate", data, baseLine, test);
        }
    }
}
#endif