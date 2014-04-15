using System;
using MiscUtil.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Extensions
{
    [TestFixture]
    public class ReferenceExtTest
    {
        [Test]
        public void ThrowIfNullOnNonNullWithName()
        {
            "fred".ThrowIfNull("name");
        }

        [Test]
        public void ThrowIfNullOnNullWithName()
        {
            string x = null;
            try
            {
                x.ThrowIfNull("name");
                Assert.Fail("Expected exception");
            }
            catch (ArgumentNullException e)
            {
                Assert.AreEqual("name", e.ParamName);
            }
        }

        [Test]
        public void ThrowIfNullOnNonNullWithoutName()
        {
            "fred".ThrowIfNull();
        }

        [Test]
        public void ThrowIfNullOnNullWithout()
        {
            string x = null;
            try
            {
                x.ThrowIfNull();
                Assert.Fail("Expected exception");
            }
            catch (ArgumentNullException e)
            {
                Assert.IsNull(e.ParamName);
            }
        }
    }
}
