using System;
using System.Collections.Generic;
using System.IO;
using MiscUtil.IO;
using NUnit.Framework;

namespace MiscUtil.UnitTests.IO
{
    [TestFixture]
    public class LineReaderTest
    {
        [Test]
        public void ConstructorUsesDelegateAtPointOfIteration()
        {
            bool called = false;
            Func<TextReader> del = () => { called = true; return new StringReader("1\r\n2\r\n3"); };

            LineReader subject = new LineReader(del);
            Assert.IsFalse(called);

            AssertAreEqual(new List<string> { "1", "2", "3" }, subject);
            Assert.IsTrue(called);
        }

        [Test]
        public void CanReadFile()
        {
            LineReader subject = new LineReader(Path.Combine("IO", "samplefile.txt"));
            AssertAreEqual(new List<string> { "First", "Second", "Third" }, subject);
        }

        [Test]
        public void FilenameIsNotUsedUntilIteration()
        {
            LineReader subject = new LineReader("missing.txt");

            try
            {
                foreach (string x in subject)
                {
                }
                Assert.Fail("Expected exception");
            }
            catch (FileNotFoundException)
            {
                // Expected
            }
        }

        void AssertAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            List<T> expectedAsList = new List<T> (expected);
            List<T> actualAsList = new List<T> (actual);

            Assert.AreEqual(expectedAsList.Count, actualAsList.Count);
            for (int i=0; i < expectedAsList.Count; i++)
            {
                Assert.AreEqual(expectedAsList[i], actualAsList[i]);
            }
        }
    }
}
