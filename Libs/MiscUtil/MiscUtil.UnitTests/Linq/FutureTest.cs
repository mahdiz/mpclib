using System;
using MiscUtil.Linq;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Linq
{
    [TestFixture]
    public class FutureTest
    {
        [Test]
        public void SetThenFetchWorks()
        {
            Future<string> subject = new Future<string>();
            subject.Value = "foo";
            Assert.AreEqual("foo", subject.Value);
        }

        [Test]
        public void SetTwiceThrowsException()
        {
            Future<string> subject = new Future<string>();
            try
            {
                subject.Value = "foo";
                subject.Value = "bar";
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void GetBeforeSetThrowsException()
        {
            Future<string> subject = new Future<string>();
            try
            {
                subject.Value.ToString();
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }
        [Test]
        public void ToStringTestNotSet()
        {
            Future<int> future = new Future<int>();
            Assert.IsNull(future.ToString());
        }
        [Test]
        public void ToStringTestSet() {
            Future<int> future = new Future<int>();
            future.Value = 5;
            Assert.AreEqual("5", future.ToString());
        }
        [Test]
        public void ToStringTestSetNull()
        {
            Future<string> future = new Future<string>();
            future.Value = null;
            Assert.AreEqual("", future.ToString());
        }
        [Test]
        public void ToStringTestSetNullT()
        {
            Future<int?> future = new Future<int?>();
            future.Value = null;
            Assert.AreEqual("", future.ToString());
        }
        [Test]
        public void ToStringTestSetBlank()
        {
            Future<string> future = new Future<string>();
            future.Value = "";
            Assert.AreEqual("", future.ToString());
        }

    }
}
