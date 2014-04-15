using System;
using System.Collections.Generic;
using MiscUtil.Linq;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Linq
{
    [TestFixture]
    public class DataProducerTest
    {
        [Test]
        public void ValueThenEnd()
        {
            DataProducer<string> subject = new DataProducer<string>();
            string pushed = null;
            bool endReached = false;

            subject.DataProduced += val => pushed = val;
            subject.EndOfData += () => endReached = true;

            Assert.IsFalse(endReached);
            Assert.IsNull(pushed);

            subject.Produce("foo");
            Assert.IsFalse(endReached);
            Assert.AreEqual("foo", pushed);

            subject.End();
            Assert.IsTrue(endReached);
        }

        [Test]
        public void JustEnd()
        {
            DataProducer<string> subject = new DataProducer<string>();
            bool endReached = false;

            subject.DataProduced += val => { throw new Exception(); };
            subject.EndOfData += () => endReached = true;

            subject.End();
            Assert.IsTrue(endReached);
        }

        [Test]
        public void EndThenProduce()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.End();

            try
            {
                subject.Produce("foo");
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void EndWithinEnd()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.EndOfData += () => { subject.End(); };

            try
            {
                subject.End();
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void EndTwice()
        {
            DataProducer<string> subject = new DataProducer<string>();
            subject.End();

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
        public void ProduceAndEnd()
        {
            DataProducer<string> subject = new DataProducer<string>();
            List<string> pushed = new List<string>();
            bool endReached = false;

            subject.DataProduced += val => pushed.Add(val);
            subject.EndOfData += () => endReached = true;

            subject.ProduceAndEnd("foo", "bar");

            Assert.IsTrue(endReached);
            Assert.AreEqual(2, pushed.Count);
            Assert.AreEqual("foo", pushed[0]);
            Assert.AreEqual("bar", pushed[1]);
        }

        [Test]
        public void ProduceAndEndWithIEnumerable()
        {
            DataProducer<string> subject = new DataProducer<string>();
            List<string> pushed = new List<string>();
            bool endReached = false;

            subject.DataProduced += val => pushed.Add(val);
            subject.EndOfData += () => endReached = true;

            List<string> list = new List<string> { "foo", "bar" };
            subject.ProduceAndEnd(list);

            Assert.IsTrue(endReached);
            Assert.AreEqual(2, pushed.Count);
            Assert.AreEqual("foo", pushed[0]);
            Assert.AreEqual("bar", pushed[1]);
        }
    }
}
