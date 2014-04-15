using System;
using System.Collections.Generic;
using MiscUtil.Collections;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Collections
{
    [TestFixture]
    public class ProjectionComparerTest
    {
        static readonly NameAndNumber A10 = new NameAndNumber { Name = "Aaaa", Number = 5 };
        static readonly NameAndNumber B15 = new NameAndNumber { Name = "Bbbb", Number = 15 };

        [Test]
        public void ProjectToStringWithIgnoredParameter()
        {
            IComparer<NameAndNumber> comparer = ProjectionComparer.Create(A10, x => x.Name);
            TestComparisons(comparer);
        }

        [Test]
        public void ProjectToStringWithExplicitType()
        {
            IComparer<NameAndNumber> comparer = ProjectionComparer.Create((NameAndNumber x) => x.Name);
            TestComparisons(comparer);
        }

        [Test]
        public void ProjectToStringWithGenericType()
        {
            IComparer<NameAndNumber> comparer = ProjectionComparer<NameAndNumber>.Create(x => x.Name);
            TestComparisons(comparer);
        }

        [Test]
        public void ProjectToNumberWithIgnoredParameter()
        {
            IComparer<NameAndNumber> comparer = ProjectionComparer.Create(A10, x => x.Number);
            TestComparisons(comparer);
        }

        [Test]
        public void ProjectToNumberWithExplicitType()
        {
            IComparer<NameAndNumber> comparer = ProjectionComparer.Create((NameAndNumber x) => x.Number);
            TestComparisons(comparer);
        }

        [Test]
        public void ProjectToNumberWithGenericType()
        {
            IComparer<NameAndNumber> comparer = ProjectionComparer<NameAndNumber>.Create(x => x.Number);
            TestComparisons(comparer);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullProjection()
        {
            new ProjectionComparer<NameAndNumber,string>(null);
        }

        [Test]
        public void ExplicitComparerTest()
        {
            // a is greater than Z with an ordinal comparison, but not with a case-insensitive ordinal comparison
            NameAndNumber lowerA = new NameAndNumber { Name = "a", Number = 10 };
            NameAndNumber upperZ = new NameAndNumber { Name = "Z", Number = 10 };

            IComparer<NameAndNumber> ordinalComparer = new ProjectionComparer<NameAndNumber, string>
                (z => z.Name, StringComparer.Ordinal);

            IComparer<NameAndNumber> insensitiveComparer = new ProjectionComparer<NameAndNumber, string>
                (z => z.Name, StringComparer.OrdinalIgnoreCase);

            Assert.Greater(ordinalComparer.Compare(lowerA, upperZ), 0);
            Assert.Less(insensitiveComparer.Compare(lowerA, upperZ), 0);
        }

        /// <summary>
        /// Utility method to perform appropriate comparisons with the given comparer. A10 should
        /// be less than B15 for all our tests
        /// </summary>
        static void TestComparisons(IComparer<NameAndNumber> comparer)
        {
            Assert.AreEqual(0, comparer.Compare(A10, A10));
            Assert.AreEqual(0, comparer.Compare(B15, B15));
            Assert.AreEqual(0, comparer.Compare(null, null));

            Assert.Less(comparer.Compare(null, A10), 0);
            Assert.Greater(comparer.Compare(A10, null), 0);

            Assert.Less(comparer.Compare(A10, B15), 0);
            Assert.Greater(comparer.Compare(B15, A10), 0);
        }

        class NameAndNumber
        {
            public int Number { get; set; }
            public string Name { get; set; }
        }
    }
}
