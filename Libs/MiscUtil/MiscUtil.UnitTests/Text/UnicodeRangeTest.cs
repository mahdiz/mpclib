#if DOTNET35
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiscUtil.Collections;
using MiscUtil.Text;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Text {
    [TestFixture]
    public class UnicodeRangeTest {

        [Test]
        public void RangesDoNotOverlap()
        {
            var dictionary = typeof(UnicodeRange).GetProperties(BindingFlags.Static | BindingFlags.Public)
                                                            .Select(prop => prop.GetValue(null, null))
                                                            .Cast<Range<char>>()
                                                            .ToDictionary(range => range.Start);
            var sortedList = new SortedList<char, Range<char>>(dictionary);

            int last = -1;
            foreach (Range<char> range in sortedList.Values)
            {
                Assert.IsTrue(range.Start > last);
                last = range.End;
            }
            Assert.AreEqual(0xffff, last);
        }

        [Test]
        public void FindRange()
        {
            // Just a few random examples
            Assert.AreSame(UnicodeRange.BasicLatin, UnicodeRange.GetRange('c'));
            Assert.AreSame(UnicodeRange.CurrencySymbols, UnicodeRange.GetRange('\u20ac'));
            Assert.AreSame(UnicodeRange.MiscellaneousTechnical, UnicodeRange.GetRange('\u2300'));
            Assert.IsNull(UnicodeRange.GetRange('\u0750'));
        }
    }
}
#endif