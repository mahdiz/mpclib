using System;
using System.Linq;
using MiscUtil.Linq;
using MiscUtil.Linq.Extensions;
using NUnit.Framework;


namespace MiscUtil.UnitTests.Linq.Extensions
{
    public partial class DataProducerExtTest
    {
        [Test]
        public void GroupBySimple()
        {
            DataProducer<string> producer = new DataProducer<string>();

            var query = producer.GroupBy(word => word[0])
                                .Select(group => new { group.Key, Words = group.AsFutureEnumerable() })
                                .AsFutureEnumerable();

            producer.ProduceAndEnd("One", "Two", "Three", "Four", "Five", "Six", "Seven", "others");

            var results = query.Value.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual('O', results[0].Key);
            Assert.IsTrue(results[0].Words.Value.SequenceEqual(new[] { "One" }));

            Assert.AreEqual('T', results[1].Key);
            Assert.IsTrue(results[1].Words.Value.SequenceEqual(new[] { "Two", "Three" }));

            Assert.AreEqual('F', results[2].Key);
            Assert.IsTrue(results[2].Words.Value.SequenceEqual(new[] { "Four", "Five" }));

            Assert.AreEqual('S', results[3].Key);
            Assert.IsTrue(results[3].Words.Value.SequenceEqual(new[] { "Six", "Seven" }));

            Assert.AreEqual('o', results[4].Key);
            Assert.IsTrue(results[4].Words.Value.SequenceEqual(new[] { "others" }));
        }

        [Test]
        public void GroupByWithComparer()
        {
            DataProducer<string> producer = new DataProducer<string>();

            var query = producer.GroupBy(word => word[0].ToString(), StringComparer.OrdinalIgnoreCase)
                                .Select(group => new { group.Key, Words = group.AsFutureEnumerable() })
                                .AsFutureEnumerable();

            producer.ProduceAndEnd("one", "Two", "three", "Four", "Five", "six", "seven", "Others");

            var results = query.Value.ToList();
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("o", results[0].Key);
            Assert.IsTrue(results[0].Words.Value.SequenceEqual(new[] { "one", "Others" }));

            Assert.AreEqual("T", results[1].Key);
            Assert.IsTrue(results[1].Words.Value.SequenceEqual(new[] { "Two", "three" }));

            Assert.AreEqual("F", results[2].Key);
            Assert.IsTrue(results[2].Words.Value.SequenceEqual(new[] { "Four", "Five" }));

            Assert.AreEqual("s", results[3].Key);
            Assert.IsTrue(results[3].Words.Value.SequenceEqual(new[] { "six", "seven" }));
        }

        [Test]
        public void GroupByWithElementProjection()
        {
            DataProducer<string> producer = new DataProducer<string>();

            var query = producer.GroupBy(word => word[0], word => word + word.Length)
                                .Select(group => new { group.Key, Words = group.AsFutureEnumerable() })
                                .AsFutureEnumerable();

            producer.ProduceAndEnd("One", "Two", "Three", "Four", "Five", "Six", "Seven", "others");

            var results = query.Value.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual('O', results[0].Key);
            Assert.IsTrue(results[0].Words.Value.SequenceEqual(new[] { "One3" }));

            Assert.AreEqual('T', results[1].Key);
            Assert.IsTrue(results[1].Words.Value.SequenceEqual(new[] { "Two3", "Three5" }));

            Assert.AreEqual('F', results[2].Key);
            Assert.IsTrue(results[2].Words.Value.SequenceEqual(new[] { "Four4", "Five4" }));

            Assert.AreEqual('S', results[3].Key);
            Assert.IsTrue(results[3].Words.Value.SequenceEqual(new[] { "Six3", "Seven5" }));

            Assert.AreEqual('o', results[4].Key);
            Assert.IsTrue(results[4].Words.Value.SequenceEqual(new[] { "others6" }));
        }

        [Test]
        public void GroupByWithComparerAndElementProjection()
        {
            DataProducer<string> producer = new DataProducer<string>();

            var query = producer.GroupBy(word => word[0].ToString(), word => word + word.Length, StringComparer.OrdinalIgnoreCase)
                                .Select(group => new { group.Key, Words = group.AsFutureEnumerable() })
                                .AsFutureEnumerable();

            producer.ProduceAndEnd("one", "Two", "three", "Four", "Five", "six", "seven", "Others");

            var results = query.Value.ToList();
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("o", results[0].Key);
            Assert.IsTrue(results[0].Words.Value.SequenceEqual(new[] { "one3", "Others6" }));

            Assert.AreEqual("T", results[1].Key);
            Assert.IsTrue(results[1].Words.Value.SequenceEqual(new[] { "Two3", "three5" }));

            Assert.AreEqual("F", results[2].Key);
            Assert.IsTrue(results[2].Words.Value.SequenceEqual(new[] { "Four4", "Five4" }));

            Assert.AreEqual("s", results[3].Key);
            Assert.IsTrue(results[3].Words.Value.SequenceEqual(new[] { "six3", "seven5" }));
        }


        [Test]
        public void GroupByWithResultProjection()
        {
            DataProducer<string> producer = new DataProducer<string>();

            var query = producer.GroupBy(word => word[0].ToString(),
                                         (key, words) => new { Key = key, MaxLength = words.Max(word => word.Length) })
                                .AsFutureEnumerable();

            producer.ProduceAndEnd("One", "Two", "Three", "Four", "Five", "Six", "Seven", "others");

            var results = query.Value.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual("O", results[0].Key);
            Assert.AreEqual(3, results[0].MaxLength.Value);

            Assert.AreEqual("T", results[1].Key);
            Assert.AreEqual(5, results[1].MaxLength.Value);

            Assert.AreEqual("F", results[2].Key);
            Assert.AreEqual(4, results[2].MaxLength.Value);

            Assert.AreEqual("S", results[3].Key);
            Assert.AreEqual(5, results[3].MaxLength.Value);

            Assert.AreEqual("o", results[4].Key);
            Assert.AreEqual(6, results[4].MaxLength.Value);
        }

        [Test]
        public void GroupByWithElementProjectionAndResultProjection()
        {
            DataProducer<string> producer = new DataProducer<string>();

            var query = producer.GroupBy(word => word[0].ToString(),
                                         word => word + word,
                                         (key, words) => new { Key = key, MaxLength = words.Max(word => word.Length) })
                                .AsFutureEnumerable();

            producer.ProduceAndEnd("One", "Two", "Three", "Four", "Five", "Six", "Seven", "others");

            var results = query.Value.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual("O", results[0].Key);
            Assert.AreEqual(6, results[0].MaxLength.Value);

            Assert.AreEqual("T", results[1].Key);
            Assert.AreEqual(10, results[1].MaxLength.Value);

            Assert.AreEqual("F", results[2].Key);
            Assert.AreEqual(8, results[2].MaxLength.Value);

            Assert.AreEqual("S", results[3].Key);
            Assert.AreEqual(10, results[3].MaxLength.Value);

            Assert.AreEqual("o", results[4].Key);
            Assert.AreEqual(12, results[4].MaxLength.Value);
        }

        [Test]
        public void GroupByWithResultProjectionAndComparer()
        {
            DataProducer<string> producer = new DataProducer<string>();

            var query = producer.GroupBy(word => word[0].ToString(),
                                         (key, words) => new { Key = key, MaxLength = words.Max(word => word.Length) },
                                         StringComparer.OrdinalIgnoreCase)
                                .AsFutureEnumerable();

            producer.ProduceAndEnd("one", "Two", "three", "Four", "Five", "six", "seven", "Others");

            var results = query.Value.ToList();
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("o", results[0].Key);
            Assert.AreEqual(6, results[0].MaxLength.Value);

            Assert.AreEqual("T", results[1].Key);
            Assert.AreEqual(5, results[1].MaxLength.Value);

            Assert.AreEqual("F", results[2].Key);
            Assert.AreEqual(4, results[2].MaxLength.Value);

            Assert.AreEqual("s", results[3].Key);
            Assert.AreEqual(5, results[3].MaxLength.Value);
        }

        [Test]
        public void GroupByWithResultProjectionAndElementProjectionAndComparer()
        {
            DataProducer<string> producer = new DataProducer<string>();

            var query = producer.GroupBy(word => word[0].ToString(),
                                         word => word + word,
                                         (key, words) => new { Key = key, MaxLength = words.Max(word => word.Length) },
                                         StringComparer.OrdinalIgnoreCase)
                                .AsFutureEnumerable();

            producer.ProduceAndEnd("one", "Two", "three", "Four", "Five", "six", "seven", "Others");

            var results = query.Value.ToList();
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("o", results[0].Key);
            Assert.AreEqual(12, results[0].MaxLength.Value);

            Assert.AreEqual("T", results[1].Key);
            Assert.AreEqual(10, results[1].MaxLength.Value);

            Assert.AreEqual("F", results[2].Key);
            Assert.AreEqual(8, results[2].MaxLength.Value);

            Assert.AreEqual("s", results[3].Key);
            Assert.AreEqual(10, results[3].MaxLength.Value);
        }

    }
}