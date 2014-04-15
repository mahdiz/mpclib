using System;
using MiscUtil.Linq;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Linq
{
    [TestFixture]
    public class FutureProxyTest
    {
        [Test]
        public void RequestingValueBeforeSourceHasDataPropagatesException()
        {
            Future<string> source = new Future<string>();
            FutureProxy<int> subject = FutureProxy<int>.FromFuture(source, x => x.Length);
            try
            {
                Console.WriteLine(subject.Value);
                Assert.Fail("Expected exception");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public void RequestingValueAfterSourceHasDataPerformsTransformation()
        {
            Future<string> source = new Future<string>();
            FutureProxy<int> subject = FutureProxy<int>.FromFuture(source, x => x.Length);
            source.Value = "hello";
            Assert.AreEqual(5, subject.Value);
        }
    }
}
