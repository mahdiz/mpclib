#if DOTNET35
using System;
using System.Collections.Generic;
using System.Linq;
using MiscUtil.Linq;
using MiscUtil.Linq.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Linq.Extensions
{
    /// <summary>
    /// Minimum implementation to test sum/avg with
    /// a reference-type
    /// </summary>
    sealed class Box<T> where T : struct
    {
        private readonly T value;
        public T Value { get { return value; } }
        public Box(T value)
        {
            this.value = value;
        }
        public override string ToString()
        {
            return value.ToString();
        }
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Box<T>)
            {
                return EqualityComparer<T>.Default.Equals(Value, ((Box<T>)obj).Value);
            }
            if (obj is T)
            {
                return EqualityComparer<T>.Default.Equals(Value, (T)obj);
            }
            return false;
        }
        public static Box<T> operator +(Box<T> x, Box<T> y)
        {
            if(x == null || y == null) return null;
            return new Box<T>(Operator.Add(x.Value, y.Value));
        }
        public static Box<T> operator /(Box<T> x, int y)
        {
            if (x == null) return null;
            return new Box<T>(Operator.DivideInt32(x.Value, y));
        }
    }

    [TestFixture]
    public class EnumerableExtTest
    {
        static Box<T>[] BoxData<T>(params T?[] values) where T : struct {
            return Array.ConvertAll<T?, Box<T>>(values, 
                x=> x.HasValue ? new Box<T>(x.Value) : null);
        }
        [Test]
        public void BoxSum()
        {
            var data = BoxData<int>(null, 1, null, 2, 3, 4, 5);

            Assert.AreEqual(new Box<int>(15), data.Sum());

            data = BoxData<int>();

            Assert.IsNull(data.Sum());

            data = BoxData<int>(null, null);

            Assert.IsNull(data.Sum());
        }

        [Test]
        public void BoxAverage()
        {
            var data = BoxData<float>(null, 1, null, 2, 3, 4, 5);

            Assert.AreEqual(new Box<float>(3), data.Average());

            data = BoxData<float>();

            Assert.IsNull(data.Average());

            data = BoxData<float>(null, null);

            Assert.IsNull(data.Average());
        }

        private static void Aggregate<TData, TResult>(
            string message,
            Func<IEnumerable<TData>, TResult> baseLine,
            Func<IEnumerable<TData>, TResult> test,
            params TData[] data)
        {
            if (baseLine == null) throw new ArgumentNullException("baseLine");
            if (test == null) throw new ArgumentNullException("test");
            if (data == null) throw new ArgumentNullException("data");

            Exception expectedEx;
            TResult expected;
            try
            {
                expected = baseLine(data);
                expectedEx = null;
            }
            catch (Exception ex)
            {
                expectedEx = ex;
                expected = default(TResult);
            }

            Exception actualEx;
            TResult actual;
            try
            {
                actual = test(data);
                actualEx = null;
            }
            catch (Exception ex)
            {
                actualEx = ex;
                actual = default(TResult);
            }

            if (expectedEx == null)
            {
                if (actualEx == null)
                {
                    Assert.AreEqual(expected, actual, message);
                }
                else
                {
                    Assert.AreEqual(expected, actualEx.GetType(), message);
                }
            }
            else
            {
                if (actualEx == null)
                {
                    Assert.AreEqual(expectedEx.GetType(), actual, message);
                }
                else
                {
                    Assert.AreEqual(expectedEx.GetType(), actualEx.GetType(), message);
                }
            }
        }

        [Test]
        public void MinInt32()
        {
            Aggregate<int, int>("int:empty", Enumerable.Min, x=>EnumerableExt.Min(x, Comparer<int>.Default));
            Aggregate<int?, int?>("int?:empty", Enumerable.Min, x=>EnumerableExt.Min(x, Comparer<int?>.Default));
            Aggregate<int?, int?>("int?:all null", Enumerable.Min, x=>EnumerableExt.Min(x, Comparer<int?>.Default), null, null, null);
            Aggregate<int?, int?>("int?:starts null", Enumerable.Min, x=>EnumerableExt.Min(x, Comparer<int?>.Default), null, 1, 2, 3, 1);
            Aggregate<int?, int?>("int?:with null", Enumerable.Min, x=>EnumerableExt.Min(x, Comparer<int?>.Default), 1, 2, null, 3, 1);
            Aggregate<int, int>("int:without null", Enumerable.Min, x=>EnumerableExt.Min(x, Comparer<int>.Default), 1, 2, 3, 1);
            Aggregate<int?, int?>("int?:without null", Enumerable.Min, x=>EnumerableExt.Min(x, Comparer<int?>.Default), 1, 2, 3, 1);

            Aggregate<int, int>("int [proj]:empty", x=>Enumerable.Min(x,y=>-y), x=>EnumerableExt.Min(x, y=>-y, Comparer<int>.Default));
            Aggregate<int?, int?>("int? [proj]:empty", x => Enumerable.Min(x, y => -y), x=>EnumerableExt.Min(x, y => -y, Comparer<int?>.Default));
            Aggregate<int?, int?>("int? [proj]:all null", x => Enumerable.Min(x, y => -y), x => EnumerableExt.Min(x, y => -y, Comparer<int?>.Default), null, null, null);
            Aggregate<int?, int?>("int? [proj]:starts null", x => Enumerable.Min(x, y => -y), x => EnumerableExt.Min(x, y => -y, Comparer<int?>.Default), null, 1, 2, 3, 1);
            Aggregate<int?, int?>("int? [proj]:with null", x => Enumerable.Min(x, y => -y), x => EnumerableExt.Min(x, y => -y, Comparer<int?>.Default), 1, 2, null, 3, 1);
            Aggregate<int, int>("int [proj]:without null", x => Enumerable.Min(x, y => -y), x => EnumerableExt.Min(x, y => -y, Comparer<int>.Default), 1, 2, 3, 1);
            Aggregate<int?, int?>("int? [proj]:without null", x => Enumerable.Min(x, y => -y), x => EnumerableExt.Min(x, y => -y, Comparer<int?>.Default), 1, 2, 3, 1);

        }

        [Test]
        public void MaxInt32()
        {
            Aggregate<int, int>("int:empty", Enumerable.Max, x => EnumerableExt.Max(x, Comparer<int>.Default));
            Aggregate<int?, int?>("int?:empty", Enumerable.Max, x => EnumerableExt.Max(x, Comparer<int?>.Default));
            Aggregate<int?, int?>("int?:all null", Enumerable.Max, x => EnumerableExt.Max(x, Comparer<int?>.Default), null, null, null);
            Aggregate<int?, int?>("int?:starts null", Enumerable.Max, x => EnumerableExt.Max(x, Comparer<int?>.Default), null, 1, 2, 3, 1);
            Aggregate<int?, int?>("int?:with null", Enumerable.Max, x => EnumerableExt.Max(x, Comparer<int?>.Default), 1, 2, null, 3, 1);
            Aggregate<int, int>("int:without null", Enumerable.Max, x => EnumerableExt.Max(x, Comparer<int>.Default), 1, 2, 3, 1);
            Aggregate<int?, int?>("int?:without null", Enumerable.Max, x => EnumerableExt.Max(x, Comparer<int?>.Default), 1, 2, 3, 1);

            Aggregate<int, int>("int [proj]:empty", x => Enumerable.Max(x, y => -y), x => EnumerableExt.Max(x, y => -y, Comparer<int>.Default));
            Aggregate<int?, int?>("int? [proj]:empty", x => Enumerable.Max(x, y => -y), x => EnumerableExt.Max(x, y => -y, Comparer<int?>.Default));
            Aggregate<int?, int?>("int? [proj]:all null", x => Enumerable.Max(x, y => -y), x => EnumerableExt.Max(x, y => -y, Comparer<int?>.Default), null, null, null);
            Aggregate<int?, int?>("int? [proj]:starts null", x => Enumerable.Max(x, y => -y), x => EnumerableExt.Max(x, y => -y, Comparer<int?>.Default), null, 1, 2, 3, 1);
            Aggregate<int?, int?>("int? [proj]:with null", x => Enumerable.Max(x, y => -y), x => EnumerableExt.Max(x, y => -y, Comparer<int?>.Default), 1, 2, null, 3, 1);
            Aggregate<int, int>("int [proj]:without null", x => Enumerable.Max(x, y => -y), x => EnumerableExt.Max(x, y => -y, Comparer<int>.Default), 1, 2, 3, 1);
            Aggregate<int?, int?>("int? [proj]:without null", x => Enumerable.Max(x, y => -y), x => EnumerableExt.Max(x, y => -y, Comparer<int?>.Default), 1, 2, 3, 1);

        }

        [Test]
        public void SumInt32()
        {
            Aggregate<int, int>("int:empty", Enumerable.Sum, EnumerableExt.Sum);
            Aggregate<int?, int?>("int?:empty", Enumerable.Sum, EnumerableExt.Sum);
            Aggregate<int?, int?>("int?:all null", Enumerable.Sum, EnumerableExt.Sum, null, null, null);
            Aggregate<int?, int?>("int?:starts null", Enumerable.Sum, EnumerableExt.Sum, null, 1, 2, 3, 1);
            Aggregate<int?, int?>("int?:with null", Enumerable.Sum, EnumerableExt.Sum, 1, 2, null, 3, 1);
            Aggregate<int, int>("int:without null", Enumerable.Sum, EnumerableExt.Sum, 1, 2, 3, 1);
            Aggregate<int?, int?>("int?:without null", Enumerable.Sum, EnumerableExt.Sum, 1, 2, 3, 1);

            Aggregate<int, int>("int [proj]:empty", x => Enumerable.Sum(x, y => -y), x => EnumerableExt.Sum(x, y => -y));
            Aggregate<int?, int?>("int? [proj]:empty", x => Enumerable.Sum(x, y => -y), x => EnumerableExt.Sum(x, y => -y));
            Aggregate<int?, int?>("int? [proj]:all null", x => Enumerable.Sum(x, y => -y), x => EnumerableExt.Sum(x, y => -y), null, null, null);
            Aggregate<int?, int?>("int? [proj]:starts null", x => Enumerable.Sum(x, y => -y), x => EnumerableExt.Sum(x, y => -y), null, 1, 2, 3, 1);
            Aggregate<int?, int?>("int? [proj]:with null", x => Enumerable.Sum(x, y => -y), x => EnumerableExt.Sum(x, y => -y), 1, 2, null, 3, 1);
            Aggregate<int, int>("int [proj]:without null", x => Enumerable.Sum(x, y => -y), x => EnumerableExt.Sum(x, y => -y), 1, 2, 3, 1);
            Aggregate<int?, int?>("int? [proj]:without null", x => Enumerable.Sum(x, y => -y), x => EnumerableExt.Sum(x, y => -y), 1, 2, 3, 1);
        }

        [Test]
        public void AverageDouble()
        {
            Aggregate<double, double>("double:empty", Enumerable.Average, EnumerableExt.Average);
            Aggregate<double?, double?>("double?:empty", Enumerable.Average, EnumerableExt.Average);
            Aggregate<double?, double?>("double?:all null", Enumerable.Average, EnumerableExt.Average, null, null, null);
            Aggregate<double?, double?>("double?:starts null", Enumerable.Average, EnumerableExt.Average, null, 1, 2, 3, 1);
            Aggregate<double?, double?>("double?:with null", Enumerable.Average, EnumerableExt.Average, 1, 2, null, 3, 1);
            Aggregate<double, double>("double:without null", Enumerable.Average, EnumerableExt.Average, 1, 2, 3, 1);
            Aggregate<double?, double?>("double?:without null", Enumerable.Average, EnumerableExt.Average, 1, 2, 3, 1);

            Aggregate<double, double>("double [proj]:empty", x => Enumerable.Average(x, y => -y), x => EnumerableExt.Average(x, y => -y));
            Aggregate<double?, double?>("double? [proj]:empty", x => Enumerable.Average(x, y => -y), x => EnumerableExt.Average(x, y => -y));
            Aggregate<double?, double?>("double? [proj]:all null", x => Enumerable.Average(x, y => -y), x => EnumerableExt.Average(x, y => -y), null, null, null);
            Aggregate<double?, double?>("double? [proj]:starts null", x => Enumerable.Average(x, y => -y), x => EnumerableExt.Average(x, y => -y), null, 1, 2, 3, 1);
            Aggregate<double?, double?>("double? [proj]:with null", x => Enumerable.Average(x, y => -y), x => EnumerableExt.Average(x, y => -y), 1, 2, null, 3, 1);
            Aggregate<double, double>("double [proj]:without null", x => Enumerable.Average(x, y => -y), x => EnumerableExt.Average(x, y => -y), 1, 2, 3, 1);
            Aggregate<double?, double?>("double? [proj]:without null", x => Enumerable.Average(x, y => -y), x => EnumerableExt.Average(x, y => -y), 1, 2, 3, 1);

        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void SumNullSource()
        {
            Complex[] data = null;
            Complex sum = data.Sum();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void SumNullProjection()
        {
            var data = new List<Complex>();
            data.Add(new Complex(1, 5));
            Func<Complex, Complex> projection = null;
            Complex sum = data.Sum(projection);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AverageNullSource()
        {
            Complex[] data = null;
            Complex avg = data.Average();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AverageNullProjection()
        {
            var data = new List<Complex>();
            data.Add(new Complex(1, 5));
            Func<Complex, Complex> projection = null;
            Complex avg = data.Average(projection);
        }

        [Test]
        public void ComplexAggregates()
        { // to test user-types
            var data = new[] {
                new Complex(1,1), new Complex(2,2), new Complex(-4,9)
            };
            Assert.AreEqual(new Complex(-1, 12), data.Sum(), "Sum");
            Assert.AreEqual(new Complex(-1M / 3M, 4), data.Average(), "Average");
            Assert.AreEqual(new Complex(-4, 9), data.Max(Complex.MagnitudeComparer), "Max");
            Assert.AreEqual(new Complex(1, 1), data.Min(Complex.MagnitudeComparer), "Min");
        }

        [Test]
        public void SinglePipelineNoComparer()
        {
            var source = Enumerable.Range(-3, 7);
            var result = source.GroupWithPipeline(x => Math.Abs(x), seq => seq.AsFutureEnumerable());

            var fetched = result.GetEnumerator();
            Assert.IsTrue(fetched.MoveNext());
            var first = fetched.Current;
            Assert.AreEqual(3, first.Key);
            Assert.IsTrue(first.Value.SequenceEqual(new[] {-3, 3}));

            Assert.IsTrue(fetched.MoveNext());
            var second = fetched.Current;
            Assert.AreEqual(2, second.Key);
            Assert.IsTrue(second.Value.SequenceEqual(new[] { -2, 2 }));

            Assert.IsTrue(fetched.MoveNext());
            var third = fetched.Current;
            Assert.AreEqual(1, third.Key);
            Assert.IsTrue(third.Value.SequenceEqual(new[] { -1, 1 }));

            Assert.IsTrue(fetched.MoveNext());
            var fourth = fetched.Current;
            Assert.AreEqual(0, fourth.Key);
            Assert.IsTrue(fourth.Value.SequenceEqual(new[] { 0 }));
        }

        [Test]
        public void SinglePipelineWithComparer()
        {
            var source = Enumerable.Range(-3, 7); // -3 to 3 inclusive
            var result = source.GroupWithPipeline(x => x, new AbsComparer(), seq => seq.AsFutureEnumerable());

            var fetched = result.GetEnumerator();
            Assert.IsTrue(fetched.MoveNext());
            var first = fetched.Current;
            Assert.AreEqual(-3, first.Key);
            Assert.IsTrue(first.Value.SequenceEqual(new[] { -3, 3 }));

            Assert.IsTrue(fetched.MoveNext());
            var second = fetched.Current;
            Assert.AreEqual(-2, second.Key);
            Assert.IsTrue(second.Value.SequenceEqual(new[] { -2, 2 }));

            Assert.IsTrue(fetched.MoveNext());
            var third = fetched.Current;
            Assert.AreEqual(-1, third.Key);
            Assert.IsTrue(third.Value.SequenceEqual(new[] { -1, 1 }));

            Assert.IsTrue(fetched.MoveNext());
            var fourth = fetched.Current;
            Assert.AreEqual(0, fourth.Key);
            Assert.IsTrue(fourth.Value.SequenceEqual(new[] { 0 }));
        }

        [Test]
        public void DoublePipelineWithComparer()
        {
            var source = Enumerable.Range(-2, 4); // -2 to 1 inclusive
            var result = source.GroupWithPipeline(x => x * 2, new AbsComparer(), seq => seq.Count(), seq => seq.Max())
                               .AsEnumerable()
                               .Select(x => new { Key=x.Key, Count=x.Value1, Max=x.Value2} )
                               .ToList();

            Assert.AreEqual(3, result.Count);

            // First group
            Assert.AreEqual(-4, result[0].Key);
            Assert.AreEqual(1, result[0].Count);
            Assert.AreEqual(-2, result[0].Max);


            // Second group
            Assert.AreEqual(-2, result[1].Key);
            Assert.AreEqual(2, result[1].Count);
            Assert.AreEqual(1, result[1].Max);

            // Third group
            Assert.AreEqual(0, result[2].Key);
            Assert.AreEqual(1, result[2].Count);
            Assert.AreEqual(0, result[2].Max);
        }

        [Test]
        public void TriplePipelineWithComparer()
        {
            var source = Enumerable.Range(-2, 4); // -2 to 1 inclusive
            var result = source.GroupWithPipeline(x => x * 2, new AbsComparer(), seq => seq.Count(), seq => seq.Max(), seq => seq.Min())
                               .AsEnumerable()
                               .Select(x => new { Key = x.Key, Count = x.Value1, Max = x.Value2, Min=x.Value3 })
                               .ToList();

            Assert.AreEqual(3, result.Count);

            // First group
            Assert.AreEqual(-4, result[0].Key);
            Assert.AreEqual(1, result[0].Count);
            Assert.AreEqual(-2, result[0].Max);
            Assert.AreEqual(-2, result[0].Min);


            // Second group
            Assert.AreEqual(-2, result[1].Key);
            Assert.AreEqual(2, result[1].Count);
            Assert.AreEqual(1, result[1].Max);
            Assert.AreEqual(-1, result[1].Min);

            // Third group
            Assert.AreEqual(0, result[2].Key);
            Assert.AreEqual(1, result[2].Count);
            Assert.AreEqual(0, result[2].Max);
            Assert.AreEqual(0, result[2].Min);
        }

        [Test]
        public void QuadruplePipelineWithComparer()
        {
            var source = Enumerable.Range(-2, 4); // -2 to 1 inclusive
            var result = source.GroupWithPipeline(x => x * 2, new AbsComparer(), seq => seq.Count(), seq => seq.Max(), seq => seq.Min(), seq => seq.Any(x => x < 0))
                               .AsEnumerable()
                               .Select(x => new { Key = x.Key, Count = x.Value1, Max = x.Value2, Min = x.Value3, AnyNegative = x.Value4})
                               .ToList();

            Assert.AreEqual(3, result.Count);

            // First group
            Assert.AreEqual(-4, result[0].Key);
            Assert.AreEqual(1, result[0].Count);
            Assert.AreEqual(-2, result[0].Max);
            Assert.AreEqual(-2, result[0].Min);
            Assert.IsTrue(result[0].AnyNegative);


            // Second group
            Assert.AreEqual(-2, result[1].Key);
            Assert.AreEqual(2, result[1].Count);
            Assert.AreEqual(1, result[1].Max);
            Assert.AreEqual(-1, result[1].Min);
            Assert.IsTrue(result[1].AnyNegative);

            // Third group
            Assert.AreEqual(0, result[2].Key);
            Assert.AreEqual(1, result[2].Count);
            Assert.AreEqual(0, result[2].Max);
            Assert.AreEqual(0, result[2].Min);
            Assert.IsFalse(result[2].AnyNegative);
        }

        private IEnumerable<KeyValuePair<string, int?>> GetPipelineSample()
        {
            var lookup = new EditableLookup<string, int?>();
            lookup.AddRange("foo", new int?[] { 1, 1, null, 1 });
            lookup.AddRange("bar", new int?[] { 2, 2, 2 });
            lookup.AddRange("test", new int?[] { 9, null, 3 });
            foreach (var group in lookup)
            {
                foreach (var item in group)
                {
                    yield return new KeyValuePair<string, int?>(group.Key, item);
                }
            }
        }
        [Test]
        public void GroupWithPipelineTuple1()
        {
            var query1 = GetPipelineSample().GroupWithPipeline(
                x => x.Key,
                y => y.Count());
            var tuple1 = query1.ToDictionary(
                x => x.Key, x => x);

            Assert.AreEqual(4, tuple1["foo"].Value); // count
        }
        [Test]
        public void GroupWithPipelineTupe2()
        {
            var query2 = GetPipelineSample().GroupWithPipeline(
                x => x.Key,
                y => y.Count(),
                y => y.Sum(z => z.Value));
            var tuple2 = query2.ToDictionary(
                 x => x.Key, x => x);

            Assert.AreEqual(4, tuple2["foo"].Value1); // count
            Assert.AreEqual(6, tuple2["bar"].Value2); // sum
        }
        [Test]
        public void GroupWithPipelineTuple3()
        {
            var query3 = GetPipelineSample().GroupWithPipeline(
                x => x.Key,
                y => y.Count(),
                y => y.Sum(z => z.Value),
                y => y.Average(z => z.Value));
            var tuple3 = query3.ToDictionary(
                x => x.Key, x => x);

            Assert.AreEqual(4, tuple3["foo"].Value1); // count
            Assert.AreEqual(6, tuple3["bar"].Value2); // sum
            Assert.AreEqual(6, tuple3["test"].Value3); // average
        }

        [Test]
        public void GroupWithPipelineTuple4()
        {
            var query4 = GetPipelineSample().GroupWithPipeline(
                x => x.Key,
                y => y.Count(),
                y => y.Sum(z => z.Value),
                y => y.Average(z => z.Value),
                y => y.Max(z => z.Value));
            var tuple4 = query4.ToDictionary(
                x => x.Key, x => x);

            Assert.AreEqual(4, tuple4["foo"].Value1); // count
            Assert.AreEqual(6, tuple4["bar"].Value2); // sum
            Assert.AreEqual(6, tuple4["test"].Value3); // average
            Assert.AreEqual(9, tuple4["test"].Value4); // max
        }

        class AbsComparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y)
            {
                return Math.Abs(x).Equals(Math.Abs(y));
            }

            public int GetHashCode(int obj)
            {
                return Math.Abs(obj).GetHashCode();
            }
        }
    }
}
#endif