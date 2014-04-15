#if DOTNET35
using System;
using System.Linq;
using System.Xml.Linq;
using MiscUtil.Xml.Linq.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Xml.Linq.Extensions
{
    [TestFixture]
    public class ObjectExtTest
    {
        [Test]
        public void AsXElementsStringProperties()
        {
            var obj = new { StringProp1 = "foo", StringProp2 = "bar" };

            var map = obj.AsXElements().ToDictionary(el => el.Name);
            Assert.AreEqual(2, map.Count);
            Assert.AreEqual("foo", map["StringProp1"].Value);
            Assert.AreEqual("bar", map["StringProp2"].Value);
        }

        [Test]
        public void AsXElementsNullValuedPropertyBecomesEmptyElement()
        {
            var obj = new { NullProp = (string)null };

            var list = obj.AsXElements().ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("NullProp", list[0].Name.LocalName);
            Assert.IsTrue(list[0].IsEmpty);
        }

        [Test]
        public void AsXElementsMixedProperties()
        {
            DateTime now = DateTime.Now;
            var obj = new { StringProp = "foo", DateTimeProp = now, Int32Prop = 32 };

            var map = obj.AsXElements().ToDictionary(attr => attr.Name);

            Assert.AreEqual(3, map.Count);
            Assert.AreEqual("foo", map["StringProp"].Value);
            Assert.AreEqual(new XElement("x", now).Value, map["DateTimeProp"].Value);
            Assert.AreEqual("32", map["Int32Prop"].Value);
        }

        [Test]
        public void AsXElementsNestedData()
        {
            var obj = new { StringProp = "foo", 
                            nestedElt = new XElement ("elt", "bar"),
                            nestedAttr = new XAttribute("attr", "baz") };

            var map = obj.AsXElements().ToDictionary(attr => attr.Name);

            Assert.AreEqual(3, map.Count);
            Assert.AreEqual("foo", map["StringProp"].Value);
            XElement nestedElt = map["nestedElt"];
            Assert.AreEqual("elt", nestedElt.Elements().Single().Name.LocalName);
            Assert.AreEqual("bar", nestedElt.Elements().Single().Value);

            XElement nestedAttr = map["nestedAttr"];
            Assert.AreEqual("attr", nestedAttr.Attributes().Single().Name.LocalName);
            Assert.AreEqual("baz", nestedAttr.Attributes().Single().Value);
        }

        [Test]
        public void AsXElementsConvertsUnderscoresToHyphens()
        {
            var obj = new { Separated_Name = "test" };

            var map = obj.AsXElements().ToDictionary(attr => attr.Name);

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual("test", map["Separated-Name"].Value);
        }

        [Test]
        public void AsXAttributesMixedProperties()
        {
            DateTime now = DateTime.Now;
            var obj = new { StringProp = "foo", DateTimeProp = now, Int32Prop = 32 };

            var map = obj.AsXAttributes().ToDictionary(attr => attr.Name);

            Assert.AreEqual(3, map.Count);
            Assert.AreEqual("foo", map["StringProp"].Value);
            Assert.AreEqual(now, (DateTime)map["DateTimeProp"]);
            Assert.AreEqual(32, (int)map["Int32Prop"]);
        }

        [Test]
        public void AsXAttributesReturnsEmptyAttributesForNulls()
        {
            var obj = new { NullProp = (string)null };

            var list = obj.AsXAttributes().ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("NullProp", list[0].Name.LocalName);
            Assert.AreEqual("", list[0].Value);
        }

        [Test]
        public void AsXAttributesConvertsUnderscoresToHyphens()
        {
            var obj = new { Separated_Name = "test" };

            var map = obj.AsXAttributes().ToDictionary(attr => attr.Name);

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual("test", map["Separated-Name"].Value);            
        }
    }
}
#endif