using System;
using System.IO;
using System.Text;
using MiscUtil.Conversion;
using MiscUtil.IO;
using NUnit.Framework;

namespace MiscUtil.UnitTests.IO
{
    [TestFixture]
    public class EndianBinaryReaderTest
    {
        const string TestString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmopqrstuvwxyz";
        static readonly byte[] TestBytes = Encoding.ASCII.GetBytes(TestString);

        /// <summary>
        /// Check fix to bug found by Jamie Rothfeder
        /// </summary>
        [Test]
        public void ReadCharsBeyondInternalBufferSize()
        {
            MemoryStream stream = new MemoryStream(TestBytes);
            EndianBinaryReader subject = new EndianBinaryReader(EndianBitConverter.Little, stream);

            char[] chars = new char[TestString.Length];
            subject.Read(chars, 0, chars.Length);
            Assert.AreEqual(TestString, new string(chars));
        }

        [Test]
        public void ReadCharsBeyondProvidedBufferSize()
        {
            MemoryStream stream = new MemoryStream(TestBytes);
            EndianBinaryReader subject = new EndianBinaryReader(EndianBitConverter.Little, stream);

            char[] chars = new char[TestString.Length-1];
            try
            {
                subject.Read(chars, 0, TestString.Length);
                Assert.Fail("Expected exception");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }
    }
}
