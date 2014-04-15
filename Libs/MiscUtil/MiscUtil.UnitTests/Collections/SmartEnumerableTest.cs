using System;
using System.Collections;
using System.Collections.Generic;
using MiscUtil.Collections;
using MiscUtil.Collections.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Collections
{
    [TestFixture]
    public class SmartEnumerableTest
    {
        [Test]
        public void NullEnumerableThrowsException()
        {
            try
            {
                new SmartEnumerable<string>(null);
                Assert.Fail("Expected exception");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [Test]
        public void EmptyEnumerable()
        {
            List<string> emptyList = new List<string>();

            SmartEnumerable<string> subject = new SmartEnumerable<string>(emptyList);
            using (IEnumerator<SmartEnumerable<string>.Entry> iterator = subject.GetEnumerator())
            {
                Assert.IsFalse(iterator.MoveNext());
            }
        }

        [Test]
        public void SingleEntryEnumerable()
        {
            List<string> list = new List<string>();
            list.Add("x");
            TestSingleEntry(new SmartEnumerable<string>(list));
        }


        [Test]
        public void SingleEntryEnumerableViaExtension()
        {
            List<string> list = new List<string>();
            list.Add("x");

            TestSingleEntry(list.AsSmartEnumerable());
        }

        [Test]
        public void SingleEntryEnumerableViaCreate()
        {
            List<string> list = new List<string>();
            list.Add("x");

            TestSingleEntry(SmartEnumerable.Create(list));
        }

        private static void TestSingleEntry(SmartEnumerable<string> subject)
        {
            using (IEnumerator<SmartEnumerable<string>.Entry> iterator = subject.GetEnumerator())
            {
                Assert.IsTrue(iterator.MoveNext());
                Assert.IsTrue(iterator.Current.IsFirst);
                Assert.IsTrue(iterator.Current.IsLast);
                Assert.AreEqual("x", iterator.Current.Value);
                Assert.AreEqual(0, iterator.Current.Index);
                Assert.IsFalse(iterator.MoveNext());
            }
        }

        [Test]
        public void SingleEntryUntypedEnumerable()
        {
            List<string> list = new List<string>();
            list.Add("x");
            IEnumerable subject = new SmartEnumerable<string>(list);
            
            int index = 0;
            foreach (SmartEnumerable<string>.Entry item in subject)
            { // only expecting 1
                Assert.AreEqual(0, index++);
                Assert.AreEqual("x", item.Value);
                Assert.IsTrue(item.IsFirst);
                Assert.IsTrue(item.IsLast);
                Assert.AreEqual(0, item.Index);
            }
            Assert.AreEqual(1, index);
        }

        [Test]
        public void DoubleEntryEnumerable()
        {
            List<string> list = new List<string>();
            list.Add("x");
            list.Add("y");

            SmartEnumerable<string> subject = new SmartEnumerable<string>(list);
            using (IEnumerator<SmartEnumerable<string>.Entry> iterator = subject.GetEnumerator())
            {
                Assert.IsTrue(iterator.MoveNext());
                Assert.IsTrue(iterator.Current.IsFirst);
                Assert.IsFalse(iterator.Current.IsLast);
                Assert.AreEqual("x", iterator.Current.Value);
                Assert.AreEqual(0, iterator.Current.Index);

                Assert.IsTrue(iterator.MoveNext());
                Assert.IsFalse(iterator.Current.IsFirst);
                Assert.IsTrue(iterator.Current.IsLast);
                Assert.AreEqual("y", iterator.Current.Value);
                Assert.AreEqual(1, iterator.Current.Index);
                Assert.IsFalse(iterator.MoveNext());
            }
        }

        [Test]
        public void TripleEntryEnumerable()
        {
            List<string> list = new List<string>();
            list.Add("x");
            list.Add("y");
            list.Add("z");

            SmartEnumerable<string> subject = new SmartEnumerable<string>(list);
            using (IEnumerator<SmartEnumerable<string>.Entry> iterator = subject.GetEnumerator())
            {
                Assert.IsTrue(iterator.MoveNext());
                Assert.IsTrue(iterator.Current.IsFirst);
                Assert.IsFalse(iterator.Current.IsLast);
                Assert.AreEqual("x", iterator.Current.Value);
                Assert.AreEqual(0, iterator.Current.Index);

                Assert.IsTrue(iterator.MoveNext());
                Assert.IsFalse(iterator.Current.IsFirst);
                Assert.IsFalse(iterator.Current.IsLast);
                Assert.AreEqual("y", iterator.Current.Value);
                Assert.AreEqual(1, iterator.Current.Index);

                Assert.IsTrue(iterator.MoveNext());
                Assert.IsFalse(iterator.Current.IsFirst);
                Assert.IsTrue(iterator.Current.IsLast);
                Assert.AreEqual("z", iterator.Current.Value);
                Assert.AreEqual(2, iterator.Current.Index);
                Assert.IsFalse(iterator.MoveNext());
            }
        }
    }
}
