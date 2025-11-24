using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using XmlRpcCore;

namespace XmlRpcCore.Tests
{
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
    }

    public class PersonWithAddress
    {
        public string Name { get; set; }
        public Address Address { get; set; }
    }

    public class PersonSimple
    {
        public string name { get; set; }
        public int age { get; set; }
    }

    public class PersonCtorNoDefault
    {
        public string Name { get; }
        public int Age { get; }

        public PersonCtorNoDefault(string name, int age)
        {
            Name = name;
            Age = age;
        }
    }

    [TestFixture]
    public class XmlRpcComplexMappingTests
    {
        [Test]
        public void Nested_POCO_Mapping_Works()
        {
            var req = new XmlRpcRequest();
            var addr = new Hashtable { { "Street", "1st Ave" }, { "City", "Metropolis" } };
            var person = new Hashtable { { "Name", "Eve" }, { "Address", addr } };
            req.Params.Add(person);

            var xml = new XmlRpcRequestSerializer().Serialize(req);
            var mapped = new XmlRpcRequestDeserializer().Deserialize<PersonWithAddress>(new StringReader(xml));

            Assert.That(mapped, Is.Not.Null);
            Assert.That(mapped.Name, Is.EqualTo("Eve"));
            Assert.That(mapped.Address, Is.Not.Null);
            Assert.That(mapped.Address.Street, Is.EqualTo("1st Ave"));
            Assert.That(mapped.Address.City, Is.EqualTo("Metropolis"));
        }

        [Test]
        public void Array_To_Array_Mapping_Works()
        {
            var req = new XmlRpcRequest();
            var arr = new ArrayList { 1, 2, 3 };
            req.Params.Add(arr);

            var xml = new XmlRpcRequestSerializer().Serialize(req);
            var mapped = new XmlRpcRequestDeserializer().Deserialize<int[]>(new StringReader(xml));

            Assert.That(mapped, Is.Not.Null);
            Assert.That(mapped.Length, Is.EqualTo(3));
            Assert.That(mapped, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void ListOfPOCOs_Mapping_Works()
        {
            var req = new XmlRpcRequest();
            var list = new ArrayList();
            list.Add(new Hashtable { { "name", "alice" }, { "age", 20 } });
            list.Add(new Hashtable { { "name", "bob" }, { "age", 25 } });
            req.Params.Add(list);

            var xml = new XmlRpcRequestSerializer().Serialize(req);
            var mapped = new XmlRpcRequestDeserializer().Deserialize<List<PersonSimple>>(new StringReader(xml));

            Assert.That(mapped, Is.Not.Null);
            Assert.That(mapped.Count, Is.EqualTo(2));
            Assert.That(mapped[0].name, Is.EqualTo("alice"));
            Assert.That(mapped[0].age, Is.EqualTo(20));
            Assert.That(mapped[1].name, Is.EqualTo("bob"));
            Assert.That(mapped[1].age, Is.EqualTo(25));
        }

        [Test]
        public void Response_To_Dictionary_Works()
        {
            var resp = new XmlRpcResponse();
            var ht = new Hashtable { { "k", "v" } };
            resp.Value = ht;
            var xml = XmlRpcResponseSerializer.Singleton.Serialize(resp);

            var mapped = new XmlRpcResponseDeserializer().Deserialize<Dictionary<string, object>>(new StringReader(xml));

            Assert.That(mapped, Is.Not.Null);
            Assert.That(mapped.ContainsKey("k"));
            Assert.That(mapped["k"].ToString(), Is.EqualTo("v"));
        }

        [Test]
        public void Missing_Constructor_Parameters_Throws()
        {
            var req = new XmlRpcRequest();
            var ht = new Hashtable { { "name", "carol" } }; // missing age
            req.Params.Add(ht);
            var xml = new XmlRpcRequestSerializer().Serialize(req);

            var deser = new XmlRpcRequestDeserializer();

            Assert.That(() => deser.Deserialize<PersonCtorNoDefault>(new StringReader(xml)), Throws.InstanceOf<MissingMethodException>());
        }

        [Test]
        public void Invalid_Conversion_Throws_InvalidCastException()
        {
            var req = new XmlRpcRequest();
            var ht = new Hashtable { { "name", "dave" }, { "age", "notanumber" } };
            req.Params.Add(ht);
            var xml = new XmlRpcRequestSerializer().Serialize(req);

            var deser = new XmlRpcRequestDeserializer();

            Assert.That(() => deser.Deserialize<PersonSimple>(new StringReader(xml)), Throws.InstanceOf<InvalidCastException>());
        }
    }
}
