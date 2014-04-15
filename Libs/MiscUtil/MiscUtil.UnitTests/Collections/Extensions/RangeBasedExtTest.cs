using System.Linq;
using MiscUtil.Collections;
using MiscUtil.Collections.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Extensions
{
    [TestFixture]
    public class RangeBasedExtTest
    {
#if DOTNET35
        [Test]
        public void ToInt32()
        {
            Range<int> range = 1.To(5);

            Assert.AreEqual(1, range.Start);
            Assert.AreEqual(5, range.End);
        }

        [Test]
        public void ToInt32Step()
        {
            var subject = 1.To(5).Step(2);

            Assert.IsTrue(subject.SequenceEqual(new[] { 1, 3, 5 }));
        }

        [Test]
        public void DecimalStep()
        {
            var subject = 0.5m.To(1m).Step(.1m);

            Assert.IsTrue(subject.SequenceEqual(new[] { 0.5m, 0.6m, 0.7m, 0.8m, 0.9m, 1m }));
        }

#endif

        [Test]
        public void StepChar() {
            var subject = 'A'.To('G').StepChar(2);
            Assert.IsTrue(subject.SequenceEqual(new[] { 'A', 'C', 'E', 'G' }));
        }      
    }
}
