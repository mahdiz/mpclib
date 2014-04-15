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
        static void GenericTest<TSource, TResult>(string message, TSource[] data,
                                                  Func<IEnumerable<TSource>, TResult> baseline,
                                                  Func<IDataProducer<TSource>, IFuture<TResult>> testcase)
        {
            GenericTest(message, data, baseline, testcase,
                (x, y) =>
                {
                    Assert.AreEqual(x, y.Value, message);
                    return true;
                });
        }
        static void GenericTest<TSource, TResult>(
            string message, TSource[] data,
            Func<IEnumerable<TSource>, TResult> baseline,
            Func<IDataProducer<TSource>, TResult> testcase)
        {
            GenericTest(message, data, baseline, testcase,
                (x, y) =>
                {
                    Assert.AreEqual(x, y, message);
                    return true;
                });
        }


        static bool GenericTest<TSource, TResult1, TResult2>(
            string message, TSource[] data,
                Func<IEnumerable<TSource>, TResult1> baseline,
                Func<IDataProducer<TSource>, TResult2> testcase,
                Func<TResult1, TResult2, bool> verify)
        {

            TResult1 result1;
            Exception ex1;
            try
            {
                result1 = baseline(data);
                ex1 = null;
            }
            catch (Exception ex)
            {
                ex1 = ex;
                result1 = default(TResult1);
            }

            TResult2 result2;
            Exception ex2;
            try
            {
                // init
                DataProducer<TSource> subject = new DataProducer<TSource>();
                result2 = testcase(subject);
                // run data
                subject.ProduceAndEnd(data);
                ex2 = null;
            }
            catch (Exception ex)
            {
                ex2 = ex;
                result2 = default(TResult2);
            }

            if (ex1 != null)
            {
                if (ex2 != null)
                {
                    Assert.AreEqual(ex1.GetType(), ex2.GetType(), message);
                }
                else
                {
                    Assert.AreEqual(ex1.GetType(), result2, message);
                }
            }
            else
            {
                if (ex2 != null)
                {
                    Assert.AreEqual(result1, ex2.GetType(), message);
                }
                else
                {
                    Assert.IsTrue(verify(result1, result2), message);
                }
            }
            return ex2 != null;

        }

        static void TestDictionary<TSource, TKey, TElement>(
            string message, TSource[] data, bool exceptionExected,
            Func<IEnumerable<TSource>, IDictionary<TKey, TElement>> baseline,
            Func<IDataProducer<TSource>, IDictionary<TKey, TElement>> testcase)
        {
            bool threw = GenericTest(message, data, baseline, testcase,
                (x, y) =>
                {
                    Assert.AreEqual(x.Count, y.Count, message + " [count]");
                    foreach (KeyValuePair<TKey, TElement> pair in x)
                    {
                        TElement val;
                        Assert.IsTrue(y.TryGetValue(pair.Key, out val),
                            message + " [not found] " + Convert.ToString(pair.Key));
                        Assert.AreEqual(pair.Value, val,
                            message + " [val] " + Convert.ToString(pair.Key));
                    }
                    return true;
                });
            Assert.AreEqual(exceptionExected, threw, message + " [exception]");

        }
        static void TestLookup<TSource, TKey, TElement>(
            string message, TSource[] data,
            Func<IEnumerable<TSource>, ILookup<TKey, TElement>> baseline,
            Func<IDataProducer<TSource>, ILookup<TKey, TElement>> testcase)
        {
            GenericTest(message, data,
                baseline, testcase, (x, y) =>
                {
                    int items = CompareLookups(message, x, y);
                    Assert.AreEqual(items, data.Length, message + " [count]");
                    return true;
                });
        }

        static int CompareLookups<TKey, TElement>(string message, ILookup<TKey, TElement> lhs, ILookup<TKey, TElement> rhs)
        {
            int count = 0;
            // check same # groups
            Assert.AreEqual(lhs.Count, rhs.Count, message);
            foreach (var lhGroup in lhs)
            {
                // check each group matches by key
                Assert.IsTrue(rhs.Contains(lhGroup.Key), message);
                var rhGroup = rhs[lhGroup.Key];

                // check group items
                var list = lhGroup.ToList();
                foreach (var rhItem in rhGroup)
                {
                    Assert.IsTrue(list.Contains(rhItem), message);
                    Assert.IsTrue(list.Remove(rhItem), message);
                    count++;
                }
                // check no spares
                Assert.AreEqual(0, list.Count, message);
            }
            return count;
        }

        [Test]
        public void AsEnumerable()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IEnumerable<string> result = subject.AsEnumerable();
            subject.ProduceAndEnd("a", "b", "c", "d");

            Assert.IsTrue(result.SequenceEqual(new[] { "a", "b", "c", "d" }));
        }

        [Test]
        public void AsEnumerableWithModificationsInPlace()
        {
            DataProducer<int> subject = new DataProducer<int>();
            IEnumerable<int> result = subject.AsEnumerable();

            Assert.IsTrue(result.SequenceEqual(new int[0]));

            subject.Produce(10);
            Assert.IsTrue(result.SequenceEqual(new[] { 10 }));

            subject.Produce(20);
            Assert.IsTrue(result.SequenceEqual(new[] { 10, 20 }));

            subject.End();
            Assert.IsTrue(result.SequenceEqual(new[] { 10, 20 }));
        }

        [Test]
        public void AsIFutureEnumerable()
        {
            DataProducer<string> subject = new DataProducer<string>();
            IFuture<IEnumerable<string>> result = subject.AsFutureEnumerable();
            subject.ProduceAndEnd("a", "b", "c", "d");

            Assert.IsTrue(result.Value.SequenceEqual(new[] { "a", "b", "c", "d" }));
        }

        [Test]
        public void ToList()
        {
            DataProducer<string> subject = new DataProducer<string>();

            List<string> result = subject.ToList();
            Assert.AreEqual(0, result.Count);

            subject.ProduceAndEnd("a", "b", "c", "d");

            Assert.IsTrue(result.SequenceEqual(new[] { "a", "b", "c", "d" }));
        }

        [Test]
        public void ToFutureArray()
        {
            DataProducer<string> subject = new DataProducer<string>();

            IFuture<string[]> result = subject.ToFutureArray();
            subject.ProduceAndEnd("a", "b", "c", "d");

            Assert.IsTrue(result.Value.SequenceEqual(new[] { "a", "b", "c", "d" }));
        }

        [Test]
        public void ToLookup()
        {
            // note duplicate values, duplicate keys (first char)
            string[] data = { "Foo", "bar", "Fred", "Fred", "Barney" };
            TestLookup<string, char, string>("simple", data,
                x => x.ToLookup(y => y[0]),
                x => x.ToLookup(y => y[0]));
            TestLookup<string, string, string>("key-comparer", data,
                x => x.ToLookup(y => y.Substring(0, 1), StringComparer.CurrentCultureIgnoreCase),
                x => x.ToLookup(y => y.Substring(0, 1), StringComparer.CurrentCultureIgnoreCase));
            TestLookup<string, char, string>("element-selector", data,
                x => x.ToLookup(y => y[0], y => y.ToUpper()),
                x => x.ToLookup(y => y[0], y => y.ToUpper()));
            TestLookup<string, string, string>("key-comparer, element-selector", data,
                x => x.ToLookup(y => y.Substring(0, 1), y => y.ToUpper(), StringComparer.CurrentCultureIgnoreCase),
                x => x.ToLookup(y => y.Substring(0, 1), y => y.ToUpper(), StringComparer.CurrentCultureIgnoreCase));
        }

        [Test]
        public void ToDictionary()
        {
            TestDictionary<string, char, string>("simple (empty)",
                new string[0], false,
                x => x.ToDictionary(y => y[0]),
                x => x.ToDictionary(y => y[0]));
            TestDictionary<string, char, string>("simple (no dups)",
                new[] { "Fred", "Barney", "Wilma", "fred" }, false,
                x => x.ToDictionary(y => y[0]),
                x => x.ToDictionary(y => y[0]));
            TestDictionary<string, char, string>("simple (dups)",
                new[] { "Fred", "Barney", "Wilma", "Frodo" }, true,
                x => x.ToDictionary(y => y[0]),
                x => x.ToDictionary(y => y[0]));


            TestDictionary<string, string, string>("key-comparer (empty)",
                new string[0], false,
                x => x.ToDictionary(y => y.Substring(0, 1), StringComparer.CurrentCultureIgnoreCase),
                x => x.ToDictionary(y => y.Substring(0, 1), StringComparer.CurrentCultureIgnoreCase));
            TestDictionary<string, string, string>("key-comparer (no dups)",
                new[] { "Fred", "Barney", "Wilma" }, false,
                x => x.ToDictionary(y => y.Substring(0, 1), StringComparer.CurrentCultureIgnoreCase),
                x => x.ToDictionary(y => y.Substring(0, 1), StringComparer.CurrentCultureIgnoreCase));
            TestDictionary<string, string, string>("key-comparer (dups)",
                new[] { "Fred", "Barney", "Wilma", "frodo" }, true,
                x => x.ToDictionary(y => y.Substring(0, 1), StringComparer.CurrentCultureIgnoreCase),
                x => x.ToDictionary(y => y.Substring(0, 1), StringComparer.CurrentCultureIgnoreCase));

            TestDictionary<string, char, string>("element-selector (empty)",
                new string[0], false,
                x => x.ToDictionary(y => y[0], y => y.ToUpper()),
                x => x.ToDictionary(y => y[0], y => y.ToUpper()));
            TestDictionary<string, char, string>("element-selector (no dups)",
                new[] { "Fred", "Barney", "Wilma", "fred", }, false,
                x => x.ToDictionary(y => y[0], y => y.ToUpper()),
                x => x.ToDictionary(y => y[0], y => y.ToUpper()));
            TestDictionary<string, char, string>("element-selector (dups)",
                new[] { "Fred", "Barney", "Wilma", "Frodo" }, true,
                x => x.ToDictionary(y => y[0], y => y.ToUpper()),
                x => x.ToDictionary(y => y[0], y => y.ToUpper()));

            TestDictionary<string, string, string>("key-comparer, element-selector (empty)",
                new string[0], false,
                x => x.ToDictionary(y => y.Substring(0, 1), y => y.ToUpper(), StringComparer.CurrentCultureIgnoreCase),
                x => x.ToDictionary(y => y.Substring(0, 1), y => y.ToUpper(), StringComparer.CurrentCultureIgnoreCase));
            TestDictionary<string, string, string>("key-comparer, element-selector (no dups)",
                new[] { "Fred", "Barney", "Wilma" }, false,
                x => x.ToDictionary(y => y.Substring(0, 1), y => y.ToUpper(), StringComparer.CurrentCultureIgnoreCase),
                x => x.ToDictionary(y => y.Substring(0, 1), y => y.ToUpper(), StringComparer.CurrentCultureIgnoreCase));
            TestDictionary<string, string, string>("key-comparer, element-selector (dups)",
                new[] { "Fred", "Barney", "Wilma", "frodo" }, true,
                x => x.ToDictionary(y => y.Substring(0, 1), y => y.ToUpper(), StringComparer.CurrentCultureIgnoreCase),
                x => x.ToDictionary(y => y.Substring(0, 1), y => y.ToUpper(), StringComparer.CurrentCultureIgnoreCase));
        }
    }
}
#endif