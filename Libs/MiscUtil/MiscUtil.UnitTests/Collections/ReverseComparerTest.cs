using System;
using MiscUtil.Collections;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Collections
{
    [TestFixture]
    public class ReverseComparerTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullComparer()
        {
            new ReverseComparer<string>(null);
        }

        [Test]
        public void NormalComparer()
        {
            var original = StringComparer.Ordinal;
            var subject = new ReverseComparer<string>(original);

            Assert.AreEqual(original.Compare("x", "y"), subject.Compare("y", "x"));
            Assert.AreEqual(0, subject.Compare("x", "x"));
        }
    }
}
