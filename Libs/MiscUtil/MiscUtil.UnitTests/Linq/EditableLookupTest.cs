using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiscUtil.Linq;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Linq
{
    [TestFixture]
    public class EditableLookupTest
    {
        [Test]
        public void EditableLookupBasic()
        {
            var lookup = new EditableLookup<string, int>();
            lookup.Add("a", 1);
            lookup.Add("a", 1);
            Assert.IsTrue(lookup.Contains("a"));
            Assert.IsFalse(lookup.Contains("A"));
            Assert.AreEqual(1, lookup.Count);
            Assert.AreEqual(2, lookup["a"].Count());
            Assert.AreEqual(0, lookup["foo"].Count());
        }
        [Test]
        public void EditableLookupNullComparer()
        {
            var lookup = new EditableLookup<string, int>(null);
            lookup.Add("a", 1);
            lookup.Add("a", 1);
            Assert.IsTrue(lookup.Contains("a"));
            Assert.IsFalse(lookup.Contains("A"));
            Assert.AreEqual(1, lookup.Count);
            Assert.AreEqual(2, lookup["a"].Count());
        }
        [Test]
        public void EditableLookupComparer()
        {
            var lookup = new EditableLookup<string, int>(StringComparer.OrdinalIgnoreCase);
            lookup.Add("a", 1);
            lookup.Add("A", 1);
            Assert.IsTrue(lookup.Contains("a"));
            Assert.IsTrue(lookup.Contains("A"));
            Assert.AreEqual(1, lookup.Count);
            Assert.AreEqual(2, lookup["a"].Count());
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void EditableLookupAddRangeNullKey()
        {
            var lookup = new EditableLookup<string, int>();
            IEnumerable<int> mate = null;
            lookup.AddRange("foo", mate);
        }

        [Test]
        public void EditableLookupAddRangeEmptyKey()
        {
            var lookup = new EditableLookup<string, int>();
            int[] data = new int[0];
            lookup.Add("foo", 1);
            Assert.AreEqual(1, lookup.Count);
            lookup.AddRange("bar", data);
            // want to check that empty insert hasn't left
            // a rogue group
            Assert.AreEqual(1, lookup.Count);
            Assert.IsFalse(lookup.Contains("bar"));
        }

#if DOTNET35
        [Test]
        public void EditableLookupAddRangeKey()
        {
            var lookup = new EditableLookup<string, int>();
            lookup.Add("a", 1);
            lookup.Add("b", 2);
            // add to existing key
            lookup.AddRange("a", new int[] { 3, 4, 5 });
            // add to new key
            lookup.AddRange("c", new int[] { 6, 7});

            Assert.AreEqual(3, lookup.Count);
            Assert.AreEqual(13, lookup["a"].Sum());
            Assert.AreEqual(13, lookup["c"].Sum());
        }

        [Test]
        public void EditableLookupAddRangeLookup()
        {
            var lookup = new EditableLookup<string, int>();
            lookup.Add("a", 1);
            lookup.Add("b", 2);

            var lookup2 = new EditableLookup<string, int>();
            lookup2.AddRange("a", new int[] { 3, 4, 5 });
            lookup2.AddRange("c", new int[] { 6, 7 });

            lookup.AddRange(lookup2);

            Assert.AreEqual(3, lookup.Count);
            Assert.AreEqual(13, lookup["a"].Sum());
            Assert.AreEqual(13, lookup["c"].Sum());
        }

        [Test]
        public void EditableLookupEnumerator()
        {
            var lookup = new EditableLookup<int, int>();
            lookup.Add(1, 1); // multipliers....
            lookup.Add(1, 2);
            lookup.Add(1, 3);
            lookup.Add(2, 4);
            lookup.Add(3, 5);
            int sum = lookup.Sum(x => x.Key * x.Sum());
            Assert.AreEqual(6 + 8 + 15, sum);
        }

        [Test]
        public void EditableLookupRemove()
        {
            var lookup = new EditableLookup<string, int>();
            lookup.Add("a", 1);
            lookup.Add("a", 2);
            lookup.Add("b", 3);
            lookup.Add("a", 4);
            Assert.IsTrue(lookup.Contains("a"));
            Assert.IsTrue(lookup.Contains("b"));
            Assert.IsTrue(lookup.Contains("b", 3));
            Assert.IsFalse(lookup.Contains("b", 17));

            Assert.IsTrue(lookup.Remove("b"));
            Assert.IsFalse(lookup.Remove("b")); // no longer there
            Assert.AreEqual(1, lookup.Count);
            Assert.AreEqual(3, lookup["a"].Count());
            Assert.IsTrue(lookup.Remove("a", 1));
            Assert.IsFalse(lookup.Remove("a", 1)); // no longer there
            Assert.IsFalse(lookup.Remove("foo", 99)); // never existed

            Assert.AreEqual(6, lookup["a"].Sum()); // 2 + 4
            lookup.Remove("a", 2);
            lookup.Remove("a", 4);
            Assert.IsFalse(lookup.Contains("a"));
            Assert.AreEqual(0, lookup.Count);            
        }

#endif
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void EditableLookupAddRangeNullLookup()
        {
            var lookup = new EditableLookup<string, int>();
            ILookup<string, int> mate = null;
            lookup.AddRange(mate);
        }

        [Test]
        public void EditableLookupUntypedEnumerator()
        {
            var lookup = new EditableLookup<int, int>();
            lookup.Add(1, 1); // multipliers....
            lookup.Add(1, 2);
            lookup.Add(1, 3);
            lookup.Add(1, 4);
            lookup.Add(2, 5);
            lookup.Add(3, 6);

            // test the lookup enumerator
            IEnumerable e = lookup;
            int i = 0;
            foreach (object obj in e)
            {
                i++;
            }
            Assert.AreEqual(3, i);

            // test the group enumerator
            e = lookup[1];
            i = 0;
            foreach (object obj in e)
            {
                i++;
            }
            Assert.AreEqual(4, i);
        }
    }
}
