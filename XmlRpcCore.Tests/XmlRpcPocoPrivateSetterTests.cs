using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using XmlRpcCore;

namespace XmlRpcCore.Tests
{
    public class PersonWithPrivateSetter
    {
        public string Name { get; private set; }
        private int Age { get; set; }

        public int GetAge() => Age;
    }

    public class PersonWithField
    {
        public string name;
        private int age;
        public int GetAge() => age;
    }

    [TestFixture]
    public class XmlRpcPocoPrivateSetterTests
    {
        [Test]
        public void PrivateSetter_Property_Is_Set()
        {
            var req = new XmlRpcRequest();
            var ht = new Hashtable { { "Name", "eve" }, { "Age", 37 } };
            req.Params.Add(ht);
            var xml = new XmlRpcRequestSerializer().Serialize(req);

            var person = new XmlRpcRequestDeserializer().Deserialize<PersonWithPrivateSetter>(new StringReader(xml));

            Assert.That(person, Is.Not.Null);
            Assert.That(person.Name, Is.EqualTo("eve"));
            Assert.That(person.GetAge(), Is.EqualTo(37));
        }

        [Test]
        public void Private_Field_Is_Set()
        {
            var req = new XmlRpcRequest();
            var ht = new Hashtable { { "name", "frank" }, { "age", 55 } };
            req.Params.Add(ht);
            var xml = new XmlRpcRequestSerializer().Serialize(req);

            var person = new XmlRpcRequestDeserializer().Deserialize<PersonWithField>(new StringReader(xml));

            Assert.That(person, Is.Not.Null);
            Assert.That(person.name, Is.EqualTo("frank"));
            Assert.That(person.GetAge(), Is.EqualTo(55));
        }
    }
}
