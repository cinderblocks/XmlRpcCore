using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace XmlRpcCore.Tests
{
    [TestFixture]
    public class XmlRpcSerializerTests
    {
        [Test]
        public void SerializeAndDeserialize_NonGenericParams_Works()
        {
            // Arrange - legacy non-generic API
            var req = new XmlRpcRequest("example.sum", new ArrayList { 1, 2, 3 });
            var serializer = new XmlRpcRequestSerializer();
            var deserializer = new XmlRpcRequestDeserializer();

            // Act - serialize to string
            var xml = serializer.Serialize(req);

            // Deserialize back
            var parsed = (XmlRpcRequest)deserializer.Deserialize(new StringReader(xml));

            // Assert - constraint model
            Assert.That(parsed.MethodName, Is.EqualTo("example.sum"));
            Assert.That(parsed.Params, Is.Not.Null);
            Assert.That(parsed.Params, Is.InstanceOf<IList>());
            Assert.That(parsed.Params.Count, Is.EqualTo(3));
            Assert.That(Convert.ToInt32(parsed.Params[0]), Is.EqualTo(1));
            Assert.That(Convert.ToInt32(parsed.Params[1]), Is.EqualTo(2));
            Assert.That(Convert.ToInt32(parsed.Params[2]), Is.EqualTo(3));
        }

        [Test]
        public void SerializeAndDeserialize_GenericParams_Works()
        {
            // Arrange - new generic API
            var req = new XmlRpcRequest();
            // Use the ParamsGeneric helper to set values
            var generic = req.ParamsGeneric as List<object> ?? new List<object>();
            generic.Add(1);
            generic.Add(2);
            generic.Add(3);

            // If Params is still non-generic underlying collection, attempt to copy back
            if (req.Params is IList legacy)
            {
                legacy.Clear();
                foreach (var v in generic) legacy.Add(v);
            }

            var serializer = new XmlRpcRequestSerializer();
            var deserializer = new XmlRpcRequestDeserializer();

            // Act
            var xml = serializer.Serialize(req);
            var parsed = (XmlRpcRequest)deserializer.Deserialize(new StringReader(xml));

            // Assert - constraint model
            Assert.That(parsed.MethodName, Is.EqualTo(req.MethodName));
            // We can use the generic accessor on the parsed request
            var parsedGeneric = parsed.ParamsGeneric;
            Assert.That(parsedGeneric.Count, Is.EqualTo(3));
            Assert.That(Convert.ToInt32(parsedGeneric[0]), Is.EqualTo(1));
            Assert.That(Convert.ToInt32(parsedGeneric[1]), Is.EqualTo(2));
            Assert.That(Convert.ToInt32(parsedGeneric[2]), Is.EqualTo(3));
        }
    }
}
