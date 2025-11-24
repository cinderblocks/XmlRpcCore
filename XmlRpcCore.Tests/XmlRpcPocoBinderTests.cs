using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using XmlRpcCore;

namespace XmlRpcCore.Tests
{
    public class SamplePerson
    {
        public string name { get; set; }
        public int age { get; set; }
    }

    [TestFixture]
    public class XmlRpcPocoBinderTests
    {
        [Test]
        public void Deserialize_ResponseValue_To_POCO_Works()
        {
            var resp = new XmlRpcResponse();
            var faultLike = new System.Collections.Hashtable();
            var value = new System.Collections.Hashtable
            {
                { "name", "alice" },
                { "age", 30 }
            };
            resp.Value = value;

            var xml = XmlRpcResponseSerializer.Singleton.Serialize(resp);

            var parsed = new XmlRpcResponseDeserializer().Deserialize(new StringReader(xml));
            // Use generic Deserialize<T> to map response value to SamplePerson
            var person = new XmlRpcResponseDeserializer().Deserialize<SamplePerson>(new StringReader(xml));

            Assert.That(person, Is.Not.Null);
            Assert.That(person.name, Is.EqualTo("alice"));
            Assert.That(person.age, Is.EqualTo(30));
        }

        [Test]
        public void Deserialize_RequestParam_To_POCO_Works()
        {
            var request = new XmlRpcRequest();
            var param = new System.Collections.Hashtable
            {
                { "name", "bob" },
                { "age", 42 }
            };
            request.Params.Add(param);
            var xml = new XmlRpcRequestSerializer().Serialize(request);

            var parsed = new XmlRpcRequestDeserializer().Deserialize(new StringReader(xml));
            var person = new XmlRpcRequestDeserializer().Deserialize<SamplePerson>(new StringReader(xml));

            Assert.That(person, Is.Not.Null);
            Assert.That(person.name, Is.EqualTo("bob"));
            Assert.That(person.age, Is.EqualTo(42));
        }
    }
}
