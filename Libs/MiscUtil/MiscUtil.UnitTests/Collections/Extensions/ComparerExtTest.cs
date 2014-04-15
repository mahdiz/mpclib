using System;
using System.Collections.Generic;
using System.Linq;
using MiscUtil.Collections;
using MiscUtil.Collections.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Collections.Extensions
{
    [TestFixture]
    public class ComparerExtTest
    {
        [Test]
        public void DoubleReverseIsNoOp()
        {
            var original = StringComparer.Ordinal;

            Assert.AreSame(original, original.Reverse().Reverse());
        }

        [Test]
        public void SingleReverseReverses()
        {
            var original = StringComparer.Ordinal;
            var subject = original.Reverse();

            Assert.AreEqual(original.Compare("x", "y"), subject.Compare("y", "x"));
            Assert.AreEqual(0, subject.Compare("x", "x"));
        }

        [Test]
        public void ThenByWithComparer()
        {
            var data = SampleType.SampleData;
            IComparer<SampleType> primary = ProjectionComparer<SampleType>.Create(t => t.First);
            IComparer<SampleType> secondary = ProjectionComparer<SampleType>.Create(t => t.Second);

            data.Sort(primary.ThenBy(secondary));

            Assert.IsTrue(new[]{2, 10, 5}.SequenceEqual(data.Select(x => x.Second)));
        }

        [Test]
        public void ThenByWithProjection()
        {
            var data = SampleType.SampleData;
            IComparer<SampleType> primary = ProjectionComparer<SampleType>.Create(t => t.First);

            data.Sort(primary.ThenBy(t => t.Second));

            Assert.IsTrue(new[] { 2, 10, 5 }.SequenceEqual(data.Select(x => x.Second)));
        }

        class SampleType
        {
            public int First { get; set; }
            public int Second { get; set; }

            public static List<SampleType> SampleData
            {
                get
                {
                    return new List<SampleType>
                    {
                        new SampleType { First=1, Second=10 },
                        new SampleType { First=5, Second=5 },
                        new SampleType { First=1, Second=2 }
                    };
                }
            }
        }
    }
}
