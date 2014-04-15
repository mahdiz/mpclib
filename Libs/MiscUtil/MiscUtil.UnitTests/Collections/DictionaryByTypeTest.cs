using System;
using System.Collections.Generic;
using MiscUtil.Collections;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Collections
{
    [TestFixture]
    public class DictionaryByTypeTest
    {
        private DictionaryByType subject;

        [SetUp]
        public void SetUp()
        {
            subject = new DictionaryByType();
        }

        [Test]
        public void AddThenGet()
        {
            object o = new object();
            subject.Add("hi");
            subject.Add(10);
            subject.Add(o);

            Assert.AreEqual("hi", subject.Get<string>());
            Assert.AreEqual(10, subject.Get<int>());
            Assert.AreSame(o, subject.Get<object>());
        }

        [Test]
        public void PutThenGet()
        {
            object o = new object();
            subject.Put("hi");
            subject.Put(10);
            subject.Put(o);

            Assert.AreEqual("hi", subject.Get<string>());
            Assert.AreEqual(10, subject.Get<int>());
            Assert.AreSame(o, subject.Get<object>());
        }

        [Test]
        public void RepeatedAddForSameTypeThrowsException()
        {
            subject.Add("Hi");
            try
            {
                subject.Add("There");
                Assert.Fail("Expected exception");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [Test]
        public void RepeatedPutForSameTypeOverwritesValue()
        {
            subject.Put("Hi");
            Assert.AreEqual("Hi", subject.Get<string>());
            subject.Put("There");
            Assert.AreEqual("There", subject.Get<string>());
        }

        [Test]
        public void GetFailsForMissingType()
        {
            try
            {
                subject.Get<string>();
                Assert.Fail("Expected exception");
            }
            catch (KeyNotFoundException)
            {
                // Expected
            }
        }

        [Test]
        public void TryGetSucceedsForMissingType()
        {
            string x;
            Assert.IsFalse(subject.TryGet(out x));
            Assert.IsNull(x);
        }

        [Test]
        public void TryGetFillsInValueForPresentType()
        {
            subject.Put("Hi");
            string x;
            Assert.IsTrue(subject.TryGet(out x));
            Assert.AreEqual("Hi", x);
        }
    }
}
