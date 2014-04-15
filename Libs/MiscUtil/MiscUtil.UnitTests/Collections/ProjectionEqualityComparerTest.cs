using System;
using System.Collections.Generic;
using MiscUtil.Collections;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Collections
{
    [TestFixture]
    public class ProjectionEqualityComparerTest
    {
        static readonly NameAndNumber A10 = new NameAndNumber { Name = "Aaaa", Number = 10 };
        static readonly NameAndNumber A15 = new NameAndNumber { Name = "Aaaa", Number = 15 };
        static readonly NameAndNumber B10 = new NameAndNumber { Name = "Bbbb", Number = 10 };
        static readonly NameAndNumber B15 = new NameAndNumber { Name = "Bbbb", Number = 15 };

        [Test]
        public void ProjectToStringWithIgnoredParameter()
        {
            TestNameComparison(ProjectionEqualityComparer.Create(A10, x => x.Name));
        }

        [Test]
        public void ProjectToStringWithExplicitType()
        {
            TestNameComparison(ProjectionEqualityComparer.Create((NameAndNumber x) => x.Name));
        }

        [Test]
        public void ProjectToStringWithGenericType()
        {
            TestNameComparison(ProjectionEqualityComparer<NameAndNumber>.Create(x => x.Name));
        }

        [Test]
        public void ProjectToNumberWithIgnoredParameter()
        {
            TestNumberComparison(ProjectionEqualityComparer.Create(A10, x => x.Number));
        }

        [Test]
        public void ProjectToNumberWithExplicitType()
        {
            TestNumberComparison(ProjectionEqualityComparer.Create((NameAndNumber x) => x.Number));
        }

        [Test]
        public void ProjectToNumberWithGenericType()
        {
            TestNumberComparison(ProjectionEqualityComparer<NameAndNumber>.Create(x => x.Number));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullProjection()
        {
            new ProjectionEqualityComparer<NameAndNumber,string>(null);
        }

        [Test]
        public void ExplicitComparerTest()
        {
            NameAndNumber lowerA = new NameAndNumber { Name = "a", Number = 10 };
            NameAndNumber upperA = new NameAndNumber { Name = "A", Number = 10 };

            IEqualityComparer<NameAndNumber> ordinalComparer = new ProjectionEqualityComparer<NameAndNumber, string>
                (z => z.Name, StringComparer.Ordinal);

            IEqualityComparer<NameAndNumber> insensitiveComparer = new ProjectionEqualityComparer<NameAndNumber, string>
                (z => z.Name, StringComparer.OrdinalIgnoreCase);

            AssertCompareNotEqual(ordinalComparer, lowerA, upperA);
            AssertCompareEqual(insensitiveComparer, lowerA, upperA);
        }

        static void AssertCompareEqual(IEqualityComparer<NameAndNumber> comparer, NameAndNumber x, NameAndNumber y)
        {
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.IsTrue(comparer.Equals(y, x));
            if (x != null && y != null)
            {
                Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));
            }
        }

        static void AssertCompareNotEqual(IEqualityComparer<NameAndNumber> comparer, NameAndNumber x, NameAndNumber y)
        {
            Assert.IsFalse(comparer.Equals(x, y));
            Assert.IsFalse(comparer.Equals(y, x));
            if (x != null && y != null)
            {
                Assert.AreNotEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));
            }
        }

        private static void TestNameComparison(IEqualityComparer<NameAndNumber> comparer)
        {
            TestComparisons(comparer);
            AssertCompareEqual(comparer, A10, A15);
            AssertCompareEqual(comparer, B10, B15);
            AssertCompareNotEqual(comparer, A10, B10);
            AssertCompareNotEqual(comparer, A15, B15);
        }

        private static void TestNumberComparison(IEqualityComparer<NameAndNumber> comparer)
        {
            TestComparisons(comparer);
            AssertCompareEqual(comparer, A10, B10);
            AssertCompareEqual(comparer, A15, B15);
            AssertCompareNotEqual(comparer, A10, A15);
            AssertCompareNotEqual(comparer, B10, B15);
        }

        /// <summary>
        /// Utility method to perform appropriate comparisons with the given comparer.
        /// For all tests:
        /// A10 == A10
        /// B15 == B15
        /// A10 != B15
        /// A10 != null
        /// B15 != null
        /// A10 might equal B10, and A10 might equal A15, depending on the comparer -
        /// this is in each individual test.
        /// </summary>
        static void TestComparisons(IEqualityComparer<NameAndNumber> comparer)
        {
            AssertCompareEqual(comparer, A10, A10);
            AssertCompareEqual(comparer, B15, B15);
            AssertCompareEqual(comparer, null, null);
            AssertCompareNotEqual(comparer, A10, B15);
            AssertCompareNotEqual(comparer, A10, null);
            AssertCompareNotEqual(comparer, B15, null);
            AssertCompareNotEqual(comparer, null, B15);
            AssertCompareNotEqual(comparer, null, A10);
            try
            {
                comparer.GetHashCode(null);
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        class NameAndNumber
        {
            public int Number { get; set; }
            public string Name { get; set; }
        }
    }
}
