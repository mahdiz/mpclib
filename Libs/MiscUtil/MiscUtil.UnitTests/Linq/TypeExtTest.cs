#if DOTNET35
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using MiscUtil.Linq.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Linq
{
    [TestFixture]
    public class TypeExtTest
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullType0()
        {
            Type t = null;
            var ctor = t.Ctor<object>();
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullType1()
        {
            Type t = null;
            var ctor = t.Ctor<int, object>();
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullType2()
        {
            Type t = null;
            var ctor = t.Ctor<int, float, object>();
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullType3()
        {
            Type t = null;
            var ctor = t.Ctor<int, float, string, object>();
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void NullType4()
        {
            Type t = null;
            var ctor = t.Ctor<int, float, string, decimal, object>();
        }
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Invalid0()
        {
            Type t = typeof(char[]);
            var ctor = t.Ctor<string>();
        }
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Invalid1()
        {
            var ctor = typeof(string).Ctor<int, string>();
        }
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Invalid2()
        {
            var ctor = typeof(string).Ctor<int, float, string>();
        }
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Invalid3()
        {
            var ctor = typeof(string).Ctor<int, float, string, string>();
        }
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Invalid4()
        {
            var ctor = typeof(string).Ctor<int, float, string, decimal, string>();
        }

        [Test]
        public void Valid0()
        {
            var ctor = typeof(StringBuilder).Ctor<StringBuilder>();
            StringBuilder sb = ctor();
            Assert.IsNotNull(sb);
        }
        [Test]
        public void Valid1()
        {
            var ctor = typeof(char[]).Ctor<int, Array>();
            Array data = ctor(10);
            Assert.AreEqual(10, data.Length);
            Assert.IsNotNull(data);
        }
        [Test]
        public void Valid2()
        {
            var ctor = typeof(Complex).Ctor<decimal, decimal, Complex>();
            Complex c = ctor(4, 4);
            Assert.AreEqual(new Complex(4, 4), c);
        }
        [Test]
        public void Valid3()
        {
            var ctor = typeof(SqlCommand).Ctor<string, SqlConnection, SqlTransaction, IDbCommand>();
            IDbCommand c = ctor("test", null, null);
            Assert.IsNotNull(c);
            Assert.AreEqual("test", c.CommandText);
        }
        [Test]
        public void Valid4()
        {
            var ctor = typeof(DateTime).Ctor<int, int, int, Calendar, DateTime>();
            DateTime dt = ctor(2001, 2, 28, CultureInfo.CurrentCulture.Calendar);
            Assert.AreEqual(2001, dt.Year);
            Assert.AreEqual(2, dt.Month);
            Assert.AreEqual(28, dt.Day);
        }
    }
}
#endif