using System;

using MiscUtil.Linq;
using MiscUtil.Linq.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Linq.Extensions
{
    public partial class DataProducerExtTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleCountWithNullSource()
        {
            NullDataProducer.Count();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalCountWithNullSource()
        {
            NullDataProducer.Count(x => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalCountWithNullCondition()
        {
            NonNullDataProducer.Count(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleLongCountWithNullSource()
        {
            NullDataProducer.LongCount();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalLongCountWithNullSource()
        {
            NullDataProducer.LongCount(x => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalLongCountWithNullCondition()
        {
            NonNullDataProducer.LongCount(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleFirstWithNullSource()
        {
            NullDataProducer.First();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalFirstWithNullSource()
        {
            NullDataProducer.First(x => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalFirstWithNullCondition()
        {
            NonNullDataProducer.First(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleLastWithNullSource()
        {
            NullDataProducer.Last();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalLastWithNullSource()
        {
            NullDataProducer.Last(x => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalLastWithNullCondition()
        {
            NonNullDataProducer.Last(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleFirstOrDefaultWithNullSource()
        {
            NullDataProducer.FirstOrDefault();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalFirstOrDefaultWithNullSource()
        {
            NullDataProducer.FirstOrDefault(x => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalFirstOrDefaultWithNullCondition()
        {
            NonNullDataProducer.FirstOrDefault(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleLastOrDefaultWithNullSource()
        {
            NullDataProducer.LastOrDefault();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalLastOrDefaultWithNullSource()
        {
            NullDataProducer.LastOrDefault(x => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalLastOrDefaultWithNullCondition()
        {
            NonNullDataProducer.LastOrDefault(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleSingleWithNullSource()
        {
            NullDataProducer.Single();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalSingleWithNullSource()
        {
            NullDataProducer.Single(x => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalSingleWithNullCondition()
        {
            NonNullDataProducer.Single(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleSingleOrDefaultWithNullSource()
        {
            NullDataProducer.SingleOrDefault();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalSingleOrDefaultWithNullSource()
        {
            NullDataProducer.SingleOrDefault(x => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalSingleOrDefaultWithNullCondition()
        {
            NonNullDataProducer.SingleOrDefault(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ElementAtWithNullSource()
        {
            NullDataProducer.ElementAt(0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ElementAtOrDefaultWithNullSource()
        {
            NullDataProducer.ElementAtOrDefault(0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ElementAtWithNegativeIndex()
        {
            NonNullDataProducer.ElementAt(-1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ElementAtOrDefaultWithNegativeIndex()
        {
            NonNullDataProducer.ElementAtOrDefault(-1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AllWithNullSource()
        {
            NullDataProducer.All(x => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AllWithNullCondition()
        {
            NonNullDataProducer.All(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleAnyWithNullSource()
        {
            NullDataProducer.Any();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalAnyWithNullSource()
        {
            NullDataProducer.Any(x => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConditionalAnyWithNullCondition()
        {
            NonNullDataProducer.Any(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleContainsWithNullSource()
        {
            NullDataProducer.Contains("x");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComparerSpecifiedContainsWithNullSource()
        {
            NullDataProducer.Contains("x", StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComparerSpecifiedContainsWithNullCondition()
        {
            NonNullDataProducer.Contains("x", null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleAggregateWithNullSource()
        {
            NullDataProducer.Aggregate((x, y) => x + y);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleAggregateWithNullAggregation()
        {
            NonNullDataProducer.Aggregate(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SeededAggregateWithNullSource()
        {
            NullDataProducer.Aggregate("", (x, y) => x + y);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SeededAggregateWithNullAggregation()
        {
            NonNullDataProducer.Aggregate("", null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SeededProjectionAggregateWithNullSource()
        {
            NullDataProducer.Aggregate("", (x, y) => x + y, x => x);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SeededProjectionAggregateWithNullAggregation()
        {
            NonNullDataProducer.Aggregate("", null, x => x);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SeededProjectionAggregateWithNullProjection()
        {
            NonNullDataProducer.Aggregate<string, string, string>("", (x, y) => x + y, null);
        }

#if DOTNET35
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleSumWithNullSource()
        {
            NullDataProducer.Sum();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProjectedSumWithNullSource()
        {
            NullDataProducer.Sum(x => x.Length);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProjectedSumWithNullProjection()
        {
            NonNullDataProducer.Sum<string, string>(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleAverageWithNullSource()
        {
            NullDataProducer.Average();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProjectedAverageWithNullSource()
        {
            NullDataProducer.Average(x => x);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProjectedAverageWithNullProjection()
        {
            NonNullDataProducer.Average<string, string>(null);
        }
#endif
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleMaxWithNullSource()
        {
            NullDataProducer.Max();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProjectedMaxWithNullSource()
        {
            NullDataProducer.Max(x => x.Length);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProjectedMaxWithNullProjection()
        {
            NonNullDataProducer.Max<string, string>(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleMinWithNullSource()
        {
            NullDataProducer.Min();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProjectedMinWithNullSource()
        {
            NullDataProducer.Min(x => x.Length);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProjectedMinWithNullProjection()
        {
            NonNullDataProducer.Min<string, string>(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GroupByNullSource()
        {
            NullDataProducer.GroupBy(x => x, x => x, (x, y) => x, StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GroupByNullKeySelector()
        {
            NonNullDataProducer.GroupBy(null, x => x, (x, y) => x, StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GroupByNullElementSelector()
        {
            NonNullDataProducer.GroupBy<string, string, string, string>(x => x, null, (x, y) => x, StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GroupByNullResultSelector()
        {
            NonNullDataProducer.GroupBy<string, string, string, string>(x => x, x => x, null, StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GroupByNullComparer()
        {
            NonNullDataProducer.GroupBy(x => x, x => x, (x, y) => x, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleSelectNullSource()
        {
            NullDataProducer.Select(x => x);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleSelectNullProjection()
        {
            NonNullDataProducer.Select((Func<string, string>)null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SelectWithIndexNullSource()
        {
            NullDataProducer.Select((x, i) => x);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SelectWithIndexNullProjection()
        {
            NonNullDataProducer.Select((Func<string, int, string>)null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleWhereNullSource()
        {
            NullDataProducer.Where(x => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleWhereNullProjection()
        {
            NonNullDataProducer.Where((Func<string, bool>)null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhereWithIndexNullSource()
        {
            NullDataProducer.Where((x, i) => false);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhereWithIndexNullProjection()
        {
            NonNullDataProducer.Where((Func<string, int, bool>)null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleDefaultIfEmptyNullSource()
        {
            NullDataProducer.DefaultIfEmpty();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DefaultIfEmptyWithDefaultNullSource()
        {
            NullDataProducer.DefaultIfEmpty("");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TakeNullSource()
        {
            NullDataProducer.Take(1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleTakeWhileNullSource()
        {
            NullDataProducer.TakeWhile(x => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleTakeWhileNullCondition()
        {
            NonNullDataProducer.TakeWhile((Func<string, bool>)null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TakeWhileWithIndexNullSource()
        {
            NullDataProducer.TakeWhile((x, i) => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TakeWhileWithIndexNullCondition()
        {
            NonNullDataProducer.TakeWhile((Func<string, int, bool>)null);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        public void SkipNullSource()
        {
            NullDataProducer.Skip(1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleSkipWhileNullSource()
        {
            NullDataProducer.SkipWhile(x => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleSkipWhileNullCondition()
        {
            NonNullDataProducer.SkipWhile((Func<string, bool>)null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SkipWhileWithIndexNullSource()
        {
            NullDataProducer.SkipWhile((x, i) => x.Length == 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SkipWhileWithIndexNullCondition()
        {
            NonNullDataProducer.SkipWhile((Func<string, int, bool>)null);
        }

#if DOTNET35
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SimpleDistinctNullSource()
        {
            NullDataProducer.Distinct();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DistinctWithComparerNullSource()
        {
            NullDataProducer.Distinct(StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DistinctWithComparerNullComparer()
        {
            NonNullDataProducer.Distinct(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReverseNullSource()
        {
            NullDataProducer.Reverse();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrderByNullSource()
        {
            NullDataProducer.OrderBy(x => x, StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrderByNullSelector()
        {
            NonNullDataProducer.OrderBy(null, StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrderByNullComparer()
        {
            NonNullDataProducer.OrderBy(x => x, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThenByNullSelector()
        {
            NonNullDataProducer.OrderBy(x => x).ThenBy(null, StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThenByNullComparer()
        {
            NonNullDataProducer.OrderBy(x => x).ThenBy(x => x, null);
        }
#endif

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AsFutureEnumerableNullSource()
        {
            NullDataProducer.AsFutureEnumerable();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AsEnumerableNullSource()
        {
            NullDataProducer.AsEnumerable();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToListNullSource()
        {
            NullDataProducer.ToList();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToFutureArrayNullSource()
        {
            NullDataProducer.ToFutureArray();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToLookupNullSource()
        {
            NullDataProducer.ToLookup(x => x, x => x, StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToLookupNullKeySelector()
        {
            NonNullDataProducer.ToLookup(null, x => x, StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToLookupNullElementSelector()
        {
            NonNullDataProducer.ToLookup(x => x, (Func<string, string>)null, StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToLookupNullComparer()
        {
            NonNullDataProducer.ToLookup(x => x, x => x, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToDictionaryNullSource()
        {
            NullDataProducer.ToDictionary(x => x, x => x, StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToDictionaryNullKeySelector()
        {
            NonNullDataProducer.ToDictionary(null, x => x, StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToDictionaryNullElementSelector()
        {
            NonNullDataProducer.ToDictionary(x => x, (Func<string, string>)null, StringComparer.CurrentCulture);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToDictionaryNullComparer()
        {
            NonNullDataProducer.ToDictionary(x => x, x => x, null);
        }

        DataProducer<string> NullDataProducer
        {
            get { return null; }
        }

        DataProducer<string> NonNullDataProducer
        {
            get { return new DataProducer<string>(); }
        }
    }
}
