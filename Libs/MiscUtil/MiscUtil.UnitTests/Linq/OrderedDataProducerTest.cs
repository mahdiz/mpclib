
using System;
using System.Collections.Generic;
using MiscUtil.Linq;
using MiscUtil.Linq.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Collections
{
    [TestFixture]
    public class OrderedDataProducerTest
    {
        class CompliantProducer<T> : IDataProducer<T>
        { // no error checking!
            public void Produce(T value)
            {
                if (DataProduced != null) DataProduced(value);
            }
            public void ProduceAndEnd(params T[] values)
            {
                foreach (T value in values)
                {
                    Produce(value);
                }
                End();
            }
            public event Action<T> DataProduced;
            public void End()
            {
                if (EndOfData != null) EndOfData();
            }
            public event Action EndOfData;
        }

        [Test]
        public void OrderedProducerNullComparer()
        {
            var producer = new DataProducer<int>();
            var ordered = new OrderedDataProducer<int>(producer, null).ToList();
            producer.ProduceAndEnd(1, 3, 0, 2, 4);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(i, ordered[i]);
            }
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullProducer()
        {
            var ordered = new OrderedDataProducer<int>(null, null);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void EndTwice()
        {
            var producer = new CompliantProducer<int>();
            var ordered = new OrderedDataProducer<int>(producer, null).ToList();
            producer.ProduceAndEnd(1, 3, 0, 2, 4);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(i, ordered[i], "verify working first");
            }
            producer.End(); // boom
        }
        
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ProduceAfterEnd()
        {
            var producer = new CompliantProducer<int>();
            var ordered = new OrderedDataProducer<int>(producer, null).ToList();
            producer.ProduceAndEnd(1, 3, 0, 2, 4);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(i, ordered[i], "verify working first");
            }
            producer.Produce(6); // boom
        }
        [Test]
        public void OrderedAggregate()
        {
            DataProducer<string> producer = new DataProducer<string>();
            var result = producer.OrderBy(s => s.Length).Count();
            producer.ProduceAndEnd("foo", "bar", "blip", "blop");
            Assert.AreEqual(4, result.Value);
        }
        /// <summary>
        /// Primarily for 2.0 unit test, to get a List<T> for Find
        /// </summary>
        private static List<T> ToConcreteList<T>(IEnumerable<T> items)
        {
            return new List<T>(items);
        }
        [Test]
        public void GroupedAggregate()
        {
            DataProducer<string> producer = new DataProducer<string>();
            var result = producer.GroupBy(s => s.Length)
                .Select(grp => new { grp.Key, Count = grp.Count() }).ToList();
            producer.ProduceAndEnd("blop", "foo", "bar", "blip", "blap");

            // use List<T>.Find so that we can test 2.0 (without Single)
            var final = ToConcreteList(result);
            // foo, bar
            Assert.AreEqual(final.Find(grp => grp.Key == 3).Count.Value, 2);
            // blop, blip, blap
            Assert.AreEqual(final.Find(grp => grp.Key == 4).Count.Value, 3);
        }
        [Test]
        public void GroupedOrderedAggregate()
        {
            DataProducer<string> producer = new DataProducer<string>();
            var result = producer.GroupBy(s => s.Length)
                .Select(grp => new { grp.Key, Count = grp.Count() })
                .OrderBy(item => item.Key).ToList();
            producer.ProduceAndEnd("blop", "foo", "bar", "blip", "blap");
            // foo, bar
            Assert.AreEqual(result[0].Key, 3);
            Assert.AreEqual(result[0].Count.Value, 2);
            // blop, blip, blap
            Assert.AreEqual(result[1].Key, 4);
            Assert.AreEqual(result[1].Count.Value, 3);
        }

        

    }
}
