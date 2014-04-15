using System;
using System.Collections.Generic;
using MiscUtil.Collections.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Collections.Extensions
{
    [TestFixture]
    public class DictionaryExtTest
    {
        [Test]
        public void ExistingValueIsReturnedWithConstructorConstraint()
        {
            var dict = new Dictionary<string, int>();
            dict["foo"] = 5000;
            Assert.AreEqual(5000, dict.GetOrCreate("foo"));
        }

        [Test]
        public void MissingValueIsCreatedAndStoredWithConstructorConstraint()
        {
            var dict = new Dictionary<string, List<int>>();
            var list = dict.GetOrCreate("foo");
            Assert.AreSame(list, dict["foo"]);
        }

        [Test]
        public void WithExistingKeyValueIsReturnedAndDelegateNotCalled()
        {
            var dict = new Dictionary<string, string>();
            dict["foo"] = "bar";
            Assert.AreEqual("bar", dict.GetOrCreate("foo", () => { throw new Exception(); }));
        }

        [Test]
        public void WithMissingKeyDelegateIsCalledAndResultStoredAndReturned()
        {
            var dict = new Dictionary<string, string>();
            Assert.AreEqual("bar", dict.GetOrCreate("foo", () => "bar"));
            Assert.AreEqual("bar", dict["foo"]);            
        }

        [Test]
        public void SpecifiedValueIsIgnoredStoredWhenKeyIsPresent()
        {
            var dict = new Dictionary<string, string>();
            dict["foo"] = "bar";
            Assert.AreEqual("bar", dict.GetOrCreate("foo", "ignored"));
        }

        [Test]
        public void SpecifiedValueIsReturnedAndStoredWhenKeyIsMissing()
        {
            var dict = new Dictionary<string,string>();
            Assert.AreEqual("bar", dict.GetOrCreate("foo", "bar"));
            Assert.AreEqual("bar", dict["foo"]);
        }
    }
}
