#if DOTNET35
using System;
using MiscUtil.Reflection;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Reflection
{
    [TestFixture]
    public class PropertyCopyTest
    {
        [Test]
        public void CopyToSameType()
        {
            Simple source = new Simple { Value = "test" };
            Simple target = PropertyCopy<Simple>.CopyFrom(source);
            Assert.AreEqual("test", target.Value);
        }

        [Test]
        public void CopyToSimilarType()
        {
            Simple source = new Simple { Value = "test" };
            OtherSimple target = PropertyCopy<OtherSimple>.CopyFrom(source);
            Assert.AreEqual("test", target.Value);
        }

        [Test]
        public void CopyAnonymousType()
        {
            Simple target = PropertyCopy<Simple>.CopyFrom(new { Value = "anon" });
            Assert.AreEqual("anon", target.Value);
        }

        [Test]
        public void CopyFromGenericType()
        {
            Generic<string> source = new Generic<string> { Value = "value" };
            Simple target = PropertyCopy<Simple>.CopyFrom(source);
            Assert.AreEqual("value", target.Value);
        }

        [Test]
        public void CopyToGenericType()
        {
            Simple source = new Simple { Value = "value" };
            Generic<string> target = PropertyCopy<Generic<string>>.CopyFrom(source);
            Assert.AreEqual("value", target.Value);
        }

        [Test]
        public void MissingProperty()
        {
            try
            {
                PropertyCopy<Simple>.CopyFrom(new { Missing = "ah!" });
                Assert.Fail("Expected exception");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [Test]
        public void ReadOnlyTargetPropertyThrowsException()
        {
            try
            {
                PropertyCopy<ReadOnly>.CopyFrom(new { Value = "bang!" });
                Assert.Fail("Expected exception");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [Test]
        public void WriteOnlyTargetPropertyIsIgnored()
        {
            WriteOnly source = new WriteOnly { Value = "copied", Write = "ignored" };
            Simple target = PropertyCopy<Simple>.CopyFrom(source);
            Assert.AreEqual("copied", target.Value);
        }

        [Test]
        public void IncorrectTypeThrowsException()
        {
            try
            {
                Simple target = PropertyCopy<Simple>.CopyFrom(new { Value = 10 });
                Assert.Fail("Expected exception");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [Test]
        public void MultipleProperties()
        {
            ThreeProperties target = PropertyCopy<ThreeProperties>.CopyFrom(new { Third = true, Second = 20, First = "multiple" });
            Assert.AreEqual("multiple", target.First);
            Assert.AreEqual(20, target.Second);
            Assert.IsTrue(target.Third);
        }

        [Test]
        public void DerivedTypeIsAccepted()
        {
            Generic<Derived> source = new Generic<Derived> { Value = new Derived() };
            Generic<Base> target = PropertyCopy<Generic<Base>>.CopyFrom(source);
            Assert.AreSame(source.Value, target.Value);
        }

        [Test]
        public void BaseTypeIsRejected()
        {
            Generic<Base> source = new Generic<Base> { Value = new Base() };
            try
            {
                PropertyCopy<Generic<Derived>>.CopyFrom(source);
                Assert.Fail("Expected exception");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        private class Base { }

        private class Derived : Base { }

        private class Simple
        {
            public string Value { get; set; }
        }

        private class OtherSimple
        {
            public string Value { get; set; }
        }

        private class ReadOnly
        {
            public string Value { get { return "readonly"; } }
        }

        private class WriteOnly
        {
            public string writeField;
            public string Write { set { writeField = value; } }

            public string Value { get; set; }
        }

        private class Generic<T>
        {
            public T Value { get; set; }
        }

        private class ThreeProperties
        {
            public string First { get; set; }
            public int Second { get; set; }
            public bool Third { get; set; }
        }
    }
}
#endif