#if DOTNET35
using NUnit.Framework;

namespace MiscUtil.UnitTests
{
    [TestFixture]
    public class GenericMathTest
    {
        [Test]
        public void DecimalAbs()
        {
            Assert.AreEqual(5m, GenericMath.Abs(-5m));
            Assert.AreEqual(5m, GenericMath.Abs(5m));
        }

        [Test]
        public void FloatAbs()
        {
            Assert.AreEqual(5f, GenericMath.Abs(-5f));
            Assert.AreEqual(5f, GenericMath.Abs(5f));
        }

        [Test]
        public void DecimalDelta()
        {
            Assert.IsTrue(GenericMath.WithinDelta(10m, 15m, 6m));
            Assert.IsTrue(GenericMath.WithinDelta(15m, 10m, 6m));

            Assert.IsTrue(GenericMath.WithinDelta(10m, 15m, 5m));
            Assert.IsTrue(GenericMath.WithinDelta(15m, 10m, 5m));

            Assert.IsFalse(GenericMath.WithinDelta(10m, 15m, 4m));
            Assert.IsFalse(GenericMath.WithinDelta(15m, 10m, 4m));
        }
    }
}
#endif
