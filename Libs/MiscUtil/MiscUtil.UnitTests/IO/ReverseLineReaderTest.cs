// The test uses LINQ even though the production class doesn't
#if DOTNET35
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MiscUtil.IO;
using NUnit.Framework;

namespace MiscUtil.UnitTests.IO
{
    [TestFixture]
    public class ReverseLineReaderTest
    {
        /// <summary>
        /// A bit like Enumerable.SequenceEquals but with more diagnostics
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        private static void AssertLines(IEnumerable<string> expected, IEnumerable<string> actual)
        {
            using (IEnumerator<string> expectedIterator = expected.GetEnumerator())
            {
                foreach (string actualLine in actual)
                {
                    if (!expectedIterator.MoveNext())
                    {
                        throw new AssertionException("Actual sequence too long. First extra line: " + actualLine);
                    }
                    Assert.AreEqual(expectedIterator.Current, actualLine);
                }
                if (expectedIterator.MoveNext())
                {
                    throw new AssertionException("Actual sequence too short. Next line expected: " + expectedIterator.Current);
                }
            }
        }

        private static void TestMultipleEncodings(string text)
        {
            TestSingleEncoding(text, 8, Encoding.ASCII);
            TestSingleEncoding(text, 8, Encoding.Unicode);
            TestSingleEncoding(text, 8, Encoding.UTF8);
            TestSingleEncoding(text, 8, Encoding.GetEncoding(28591));
        }

        private static void TestSingleEncoding(string text, int bufferSize, Encoding encoding)
        {
            DisposeCheckingMemoryStream stream = new DisposeCheckingMemoryStream(encoding.GetBytes(text));
            var reader = new ReverseLineReader(() => stream, encoding, bufferSize);
            AssertLines(new LineReader(() => new StringReader(text)).Reverse(), reader);
            Assert.IsTrue(stream.Disposed);
        }

        [Test]
        public void SingleLine()
        {
            TestMultipleEncodings("Foo");
        }

        [Test]
        public void SingleLineGreaterThanBufferSize()
        {
            TestMultipleEncodings("This is more than 8 bytes long.");
        }

        [Test]
        public void TwoLinesCRLF()
        {
            TestMultipleEncodings("Foo\r\nBar");
        }

        [Test]
        public void TwoLinesCR()
        {
            TestMultipleEncodings("Foo\rBar");
        }

        [Test]
        public void TwoLinesLF()
        {
            TestMultipleEncodings("Foo\nBar");
        }

        [Test]
        public void FourLinesIncludingTwoEmptyWithLineEndingsMixture()
        {
            TestMultipleEncodings("Foo\n\r\n\rBar");
            TestMultipleEncodings("Foo\r\n\r\n\r\nBar");
            TestMultipleEncodings("Foo\n\n\nBar");
            TestMultipleEncodings("Foo\r\r\rBar");
        }

        [Test]
        public void EmptyLineAtStart()
        {
            TestMultipleEncodings("\rFoo");
            TestMultipleEncodings("\nFoo");
            TestMultipleEncodings("\r\nFoo");
        }

        [Test]
        public void EmptyLineAtEnd()
        {
            TestMultipleEncodings("Foo\r");
            TestMultipleEncodings("Foo\n");
            TestMultipleEncodings("Foo\r\n");
        }

        [Test]
        public void EmptyString()
        {
            TestMultipleEncodings("");
        }

        [Test]
        public void MultipleEmptyStrings()
        {
            TestMultipleEncodings("\r");
            TestMultipleEncodings("\r\n");
            TestMultipleEncodings("\n");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void NonFixedNonUtf8EncodingThrowsArgumentException()
        {
            new ReverseLineReader(() => new MemoryStream(), Encoding.GetEncoding("shift-jis"));
        }

        [Test]
        public void UnreadableStreamThrowsExceptionEagerly()
        {
            var stream = new DisabledMemoryStream { canRead = false };
            var reader = new ReverseLineReader(() => stream);
            try
            {
                reader.GetEnumerator();
                Assert.Fail("Expected exception");
            }
            catch (NotSupportedException)
            {
                // Expected
            }
        }

        [Test]
        public void UnseekableStreamThrowsExceptionEagerly()
        {
            var stream = new DisabledMemoryStream { canSeek = false };
            var reader = new ReverseLineReader(() => stream);
            try
            {
                reader.GetEnumerator();
                Assert.Fail("Expected exception");
            }
            catch (NotSupportedException)
            {
                // Expected
            }
        }

        [Test]
        public void UnwritableStreamIsOkay()
        {
            var stream = new DisabledMemoryStream(Encoding.ASCII.GetBytes("foo")) { canWrite = false };
            var reader = new ReverseLineReader(() => stream);
            AssertLines(new[]{"foo"}, reader);
        }

        [Test]
        public void InvalidUtf8InMiddle()
        {            
            // One real character and four bytes which don't have any character start. Broken!
            var stream = new MemoryStream(new byte[] { 0x40, 0x80, 0x80, 0x80, 0x80 });
            AssertInvalidData(new ReverseLineReader(() => stream, Encoding.UTF8, 4));
        }

        [Test]
        public void InvalidUtf8AtStart()
        {
            // We've got the end of a character, but not the start! (There's some valid data as well.)
            var stream = new MemoryStream(new byte[] { 0x80, 0x40, 0x40, 0x40, 0x40, 0x40});
            AssertInvalidData(new ReverseLineReader(() => stream, Encoding.UTF8, 4));
        }

        [Test]
        public void InvalidUtf16Length()
        {
            // UTF-16 has to be an even number of bytes long.
            var stream = new MemoryStream(new byte[15]);
            AssertInvalidData(new ReverseLineReader(() => stream, Encoding.Unicode, 4));
        }

        [Test]
        public void Utf16WithBufferBreakInMiddle()
        {
            // 10 bytes, 5 byte buffer
            TestSingleEncoding("ABCDE", 5, Encoding.Unicode);
        }

        [Test]
        public void Utf8WithBufferBreaks()
        {
            // xxx\u20acyyy encodes to 9 bytes - 3 single characters,
            // then one character over 3 bytes, then 3 single characters.

            // All in one go.
            TestSingleEncoding("xxx\u20acyyy", 20, Encoding.UTF8);
            // Three easy chunks
            TestSingleEncoding("xxx\u20acyyy", 3, Encoding.UTF8);
            // Break just after start of Euro sign
            TestSingleEncoding("xxx\u20acyyy", 5, Encoding.UTF8);
            // Break at end of Euro sign
            TestSingleEncoding("xxx\u20acyyy", 4, Encoding.UTF8);
        }


        private static void AssertInvalidData(ReverseLineReader reader)
        {
            try
            {
                foreach (string ignored in reader)
                {
                }
                Assert.Fail("Expected exception");
            }
            catch (InvalidDataException)
            {
                // Expected
            }
        }

        class DisabledMemoryStream : MemoryStream
        {
            internal bool canRead = true;
            internal bool canWrite = true;
            internal bool canSeek = true;

            public override bool CanRead
            {
                get { return canRead; }
            }

            public override bool CanWrite
            {
                get { return canWrite; }
            }

            public override bool CanSeek
            {
                get { return canSeek; }
            }

            internal DisabledMemoryStream()
            {
            }

            internal DisabledMemoryStream(byte[] data) : base(data)
            {
            }
        }

        class DisposeCheckingMemoryStream : MemoryStream
        {
            private bool disposed = false;

            internal DisposeCheckingMemoryStream(byte[] data) : base(data)
            {
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                disposed = true;
            }

            internal bool Disposed
            {
                get { return disposed; }
            }
        }
    }
}
#endif