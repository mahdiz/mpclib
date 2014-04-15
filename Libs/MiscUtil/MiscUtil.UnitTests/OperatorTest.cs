#if DOTNET35
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace MiscUtil.UnitTests
{
    [TestFixture]
    public class OperatorTest
    {
        [Test]
        public void ConvertInt32ToDouble()
        {
            int from = 280;
            double d = Operator.Convert<int, double>(from);
            int i = Operator.Convert<double, int>(d);
            Assert.AreEqual(i, from);
            Assert.AreEqual(d, (double)i);
        }

        [Test]
        public void XorInt32()
        {
            Assert.AreEqual(270 ^ 54, Operator.Xor(270, 54));
        }


        [Test]
        public void SubtractInt32()
        {
            Assert.AreEqual(270 - 54, Operator.Subtract(270, 54));
        }

        [Test]
        public void OrInt32()
        {
            Assert.AreEqual(270 | 54, Operator.Or(270, 54));
        }

        [Test]
        public void NotEqualInt32()
        {
            Assert.IsTrue(Operator.NotEqual(270, 54));
            Assert.IsFalse(Operator.NotEqual(270, 270));

        }


        [Test]
        public void EqualInt32()
        {
            Assert.IsFalse(Operator.Equal(54, 270));
            Assert.IsTrue(Operator.Equal(54, 54));
        }

        [Test]
        public void NotInt32()
        {
            Assert.AreEqual(~270, Operator.Not(270));
        }

        [Test]
        public void NegateInt32()
        {
            Assert.AreEqual(-270, Operator.Negate(270));
        }

        [Test]
        public void MultiplyInt32()
        {
            Assert.AreEqual(270 * 54, Operator.Multiply(270, 54));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void MultiplyString()
        {
            string prod = Operator.Multiply("abc","def");
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void NegateString()
        {
            string neg = Operator.Negate("abc");
        }

        [Test]
        public void LessThanOrEqualInt32()
        {
            Assert.IsTrue(Operator.LessThanOrEqual(54, 270));
            Assert.IsTrue(Operator.LessThanOrEqual(270, 270));
            Assert.IsFalse(Operator.LessThanOrEqual(270, 54));
        }


        [Test]
        public void LessThanInt32()
        {
            Assert.IsTrue(Operator.LessThan(54, 270));
            Assert.IsFalse(Operator.LessThan(270, 270));
            Assert.IsFalse(Operator.LessThan(270, 54));
        }

        [Test]
        public void GreaterThanOrEqualInt32()
        {
            Assert.IsFalse(Operator.GreaterThanOrEqual(54, 270));
            Assert.IsTrue(Operator.GreaterThanOrEqual(270, 270));
            Assert.IsTrue(Operator.GreaterThanOrEqual(270, 54));
        }

        [Test]
        public void GreaterThanInt32()
        {
            Assert.IsFalse(Operator.GreaterThan(54, 270));
            Assert.IsFalse(Operator.GreaterThan(270, 270));
            Assert.IsTrue(Operator.GreaterThan(270, 54));
        }

        [Test]
        public void DivideInt32DoubleTest()
        {
            Assert.AreEqual(14514.7 / 45, Operator.DivideInt32(14514.7, 45));
        }

        [Test]
        public void DivideDouble()
        {
            Assert.AreEqual(14514.7 / 45.2, Operator.Divide(14514.7, 45.2));
        }

        [Test]
        public void Zero()
        {
            Assert.AreEqual(Operator<int>.Zero, (int)0);
            Assert.AreEqual(Operator<float>.Zero, (float)0);
            Assert.AreEqual(Operator<decimal>.Zero, (decimal)0);
            Assert.AreEqual(Operator<string>.Zero, null);
        }
        
        [Test]
        public void AndInt32()
        {
            Assert.AreEqual(270 & 54, Operator.And(270, 54));
        }

        [Test]
        public void AddInt32()
        {
            Assert.AreEqual(270 + 54, Operator.Add(270, 54));
        }

        [Test]
        public void AddDateTimeTimeSpan()
        {
            DateTime from = DateTime.Today;
            TimeSpan delta = TimeSpan.FromHours(73.5);
            Assert.AreEqual(from + delta, Operator.AddAlternative(from, delta));
        }
        
        [Test]
        public void MultiplyFloatInt32()
        {
            float from = 123.43F;
            int factor = 12;
            Assert.AreEqual(from * factor, Operator.MultiplyAlternative(from, factor));
        }

        [Test]
        public void DivideFloatInt32()
        {
            float from = 123.43F;
            int divisor = 12;
            Assert.AreEqual(from / divisor, Operator.DivideAlternative(from, divisor));
            Assert.AreEqual(from / divisor, Operator.DivideInt32(from, divisor));
        }

        [Test]
        public void DivideInt32()
        {
            float from = 123.43F;
            int divisor = 12;
            Assert.AreEqual(from / divisor, Operator.DivideAlternative(from, divisor));
        }


        [Test]
        public void SubtractDateTimeTimeSpan()
        {
            DateTime from = DateTime.Today;
            TimeSpan delta = TimeSpan.FromHours(73.5);
            Assert.AreEqual(from - delta, Operator.SubtractAlternative(from, delta));
        }

        [Test]
        public void AddTestComplex()
        {
            Complex a = new Complex(12, 3);
            Complex b = new Complex(2, 5);

            Assert.AreEqual(a + b, Operator.Add(a, b));
        }

        [Test]
        public void SubtractTestComplex()
        {
            Complex a = new Complex(12, 3);
            Complex b = new Complex(2, 5);

            Assert.AreEqual(a - b, Operator.Subtract(a, b));
        }

        /// <summary>
        /// Complex number struct created *solely* for test purposes - hence the lack of completeness
        /// </summary>
        
    }

    
    public struct Complex : IEquatable<Complex>
    {
        readonly decimal real;
        readonly decimal imaginary;
        public decimal Real { get { return real; } }
        public decimal Imaginary { get { return imaginary; } }
        public Complex(decimal real, decimal imaginary)
        {
            this.real = real;
            this.imaginary = imaginary;
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", real, imaginary);
        }
        public override int GetHashCode()
        {
            return (real.GetHashCode() * 7) + imaginary.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Complex))
            {
                return false;
            }
            return Equals((Complex)obj);
        }

        public bool Equals(Complex other)
        {
            return this.real == other.real &&
                   this.imaginary == other.imaginary;
        }

        public static Complex operator +(Complex first, Complex second)
        {
            return new Complex(first.real + second.real, first.imaginary + second.imaginary);
        }

        public static Complex operator -(Complex first, Complex second)
        {
            return new Complex(first.real - second.real, first.imaginary - second.imaginary);
        }
        public static Complex operator /(Complex first, int second)
        {
            return new Complex(first.real / second, first.imaginary / second);
        }
        public static IComparer<Complex> MagnitudeComparer
        {
            get { return ComplexMagnitudeComparer.Singleton; }
        }

        class ComplexMagnitudeComparer : IComparer<Complex>
        {
            private ComplexMagnitudeComparer() { }
            internal static readonly IComparer<Complex> Singleton = new ComplexMagnitudeComparer();
            int IComparer<Complex>.Compare(Complex lhs, Complex rhs)
            {
                return Comparer<decimal>.Default.Compare(
                    lhs.real * lhs.real + lhs.imaginary * lhs.imaginary,
                    rhs.real * rhs.real + rhs.imaginary * rhs.imaginary);
            }
        }
    }
}
#endif