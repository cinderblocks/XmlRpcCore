using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using XmlRpcCore;

namespace XmlRpcCore.Tests
{
    public class PersonCtor
    {
        public string Name { get; }
        public int Age { get; }

        public PersonCtor(string name, int age)
        {
            Name = name;
            Age = age;
        }
    }

    public class PersonAttr
    {
        [XmlRpcName("fullname")]
        public string FullName { get; set; }
        public int Age { get; set; }
    }

    [TestFixture]
    public class XmlRpcPocoBinderConstructorTests
    {
        [Test]
        public void Deserialize_To_Constructor_Binding_Works()
        {
            var req = new XmlRpcRequest();
            var param = new Hashtable
            {
                { "name", "carol" },
                { "age", 28 }
            };
            req.Params.Add(param);
            var xml = new XmlRpcRequestSerializer().Serialize(req);

            var person = new XmlRpcRequestDeserializer().Deserialize<PersonCtor>(new StringReader(xml));

            Assert.That(person, Is.Not.Null);
            Assert.That(person.Name, Is.EqualTo("carol"));
            Assert.That(person.Age, Is.EqualTo(28));
        }

        [Test]
        public void Deserialize_With_Attribute_Mapping_Works()
        {
            var req = new XmlRpcRequest();
            var param = new Hashtable
            {
                { "fullname", "dan" },
                { "age", 33 }
            };
            req.Params.Add(param);
            var xml = new XmlRpcRequestSerializer().Serialize(req);

            var person = new XmlRpcRequestDeserializer().Deserialize<PersonAttr>(new StringReader(xml));

            Assert.That(person, Is.Not.Null);
            Assert.That(person.FullName, Is.EqualTo("dan"));
            Assert.That(person.Age, Is.EqualTo(33));
        }
    }
}
