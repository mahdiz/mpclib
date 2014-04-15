using System;
using NUnit.Framework;

namespace MiscUtil.UnitTests
{
    [TestFixture]
    public class NonNullableTest
    {
        [Test]
        public void NonNullConstructionAndConversion()
        {
            string x = new string('x', 1);
            NonNullable<string> subject = new NonNullable<string>(x);
            Assert.AreSame(x, (string)subject);
            Assert.AreSame(x, subject.Value);
        }

        [Test]
        public void NonNullConversionFromAndTo()
        {
            string x = new string('x', 1);
            NonNullable<string> subject = x;
            Assert.AreSame(x, (string)subject);
            Assert.AreSame(x, subject.Value);
        }

        [Test]
        public void NullConstruction()
        {
            try
            {
                new NonNullable<string>(null);
                Assert.Fail("Expected exception");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [Test]
        public void NullConversion()
        {
            string x = null;
            try
            {
                NonNullable<string> y = x;
                Assert.Fail("Expected exception:" + y);
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [Test]
        public void DefaultConstructionAndValueProperty()
        {
            NonNullable<string> x = new NonNullable<string>();
            try
            {
                string y = x.Value;
                Assert.Fail("Expected exception:" + y);
            }
            catch (NullReferenceException)
            {
                // Expected
            }
        }

        [Test]
        public void DefaultConstructionAndImplicitConversion()
        {
            NonNullable<string> x = new NonNullable<string>();
            try
            {
                string y = x;
                Assert.Fail("Expected exception:" + y);
            }
            catch (NullReferenceException)
            {
                // Expected
            }
        }

        [Test]
        public void EqualityOperator()
        {
            string x = new string('1', 10);
            string y = new string('1', 10);
            string z = new string('z', 10);

            NonNullable<string> xx = x;
            NonNullable<string> yy = y;
            NonNullable<string> zz = z;
            NonNullable<string> nn = new NonNullable<string>();

#pragma warning disable 1718
            Assert.IsTrue(xx == xx);
            Assert.IsTrue(yy == yy);
            Assert.IsTrue(zz == zz);
            Assert.IsTrue(nn == nn);
#pragma warning restore 1718

            Assert.IsFalse(xx == yy);
            Assert.IsFalse(yy == zz);
            Assert.IsFalse(zz == nn);
            Assert.IsFalse(nn == xx);
        }

        [Test]
        public void InequalityOperator()
        {
            string x = new string('1', 10);
            string y = new string('1', 10);
            string z = new string('z', 10);

            NonNullable<string> xx = x;
            NonNullable<string> yy = y;
            NonNullable<string> zz = z;
            NonNullable<string> nn = new NonNullable<string>();

#pragma warning disable 1718
            Assert.IsFalse(xx != xx);
            Assert.IsFalse(yy != yy);
            Assert.IsFalse(zz != zz);
            Assert.IsFalse(nn != nn);
#pragma warning restore 1718

            Assert.IsTrue(xx != yy);
            Assert.IsTrue(yy != zz);
            Assert.IsTrue(zz != nn);
            Assert.IsTrue(nn != xx);
        }

        [Test]
        public void EqualityMethod()
        {
            string x = new string('1', 10);
            string y = new string('1', 10);
            string z = new string('z', 10);

            NonNullable<string> xx = x;
            NonNullable<string> yy = y;
            NonNullable<string> zz = z;
            NonNullable<string> nn = new NonNullable<string>();

            Assert.IsTrue(xx.Equals(xx));
            Assert.IsTrue(xx.Equals(yy));
            Assert.IsFalse(xx.Equals(zz));
            Assert.IsFalse(xx.Equals(nn));
            Assert.IsFalse(nn.Equals(xx));
        }

        [Test]
        public void TestGetHashCode()
        {
            Assert.AreEqual("hi".GetHashCode(), new NonNullable<string>("hi").GetHashCode());
            Assert.AreEqual(0, new NonNullable<string>().GetHashCode());
        }

        [Test]
        public void TestToString()
        {
            Assert.AreEqual("hi", new NonNullable<string>("hi").ToString());
            Assert.AreEqual("", new NonNullable<string>().ToString());
        }

        [Test]
        public void BoxingIsUnpleasant()
        {
            NonNullable<string> x = "hi";
            object y = x;
            Assert.IsFalse(y is string);
        }

        [Test]
        public void Demo()
        {
            SampleMethod("hello"); // No problems
            try
            {
                SampleMethod(null);
                Assert.Fail("Expected exception");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
            try
            {
                SampleMethod(new NonNullable<string>());
                Assert.Fail("Expected exception");
            }
            catch (NullReferenceException)
            {
                // Expected
            }
        }

        private static void SampleMethod(NonNullable<string> text)
        {
            // Shouldn't get here with usual conversions, but could do
            // through default construction. The conversion to string
            // will throw an exception anyway, so we're guaranteed
            // that foo is non-null afterwards.
            string foo = text;
            Assert.IsNotNull(foo);
        }
    }
}
