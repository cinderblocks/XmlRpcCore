using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using XmlRpcCore;

namespace XmlRpcCore.Tests
{
    [TestFixture]
    public class XmlRpcAdditionalTests
    {
        [Test]
        public void DateTime_RoundTrip_Works()
        {
            var dt = new DateTime(2020, 12, 31, 23, 59, 59);
            var req = new XmlRpcRequest();
            if (req.Params is IList legacy)
            {
                legacy.Add(dt);
            }

            var xml = new XmlRpcRequestSerializer().Serialize(req);
            var parsed = (XmlRpcRequest)new XmlRpcRequestDeserializer().Deserialize(new StringReader(xml));

            Assert.That(parsed.Params, Is.Not.Null);
            Assert.That(parsed.Params.Count, Is.GreaterThanOrEqualTo(1));
            var parsedVal = parsed.Params[0];
            Assert.That(parsedVal, Is.TypeOf<DateTime>());
            Assert.That(((DateTime)parsedVal), Is.EqualTo(dt));
        }

        [Test]
        public void Struct_Serialization_Works()
        {
            var req = new XmlRpcRequest();

            var dict = new Dictionary<string, object>
            {
                { "name", "bob" },
                { "age", 42 }
            };

            if (req.Params is IList legacy)
            {
                legacy.Add(dict);
            }

            var xml = new XmlRpcRequestSerializer().Serialize(req);
            var parsed = (XmlRpcRequest)new XmlRpcRequestDeserializer().Deserialize(new StringReader(xml));

            Assert.That(parsed.Params.Count, Is.EqualTo(1));
            var item = parsed.Params[0];
            Assert.That(item, Is.InstanceOf<IDictionary>());

            var parsedDict = item as IDictionary;
            Assert.That(parsedDict.Contains("name"));
            Assert.That(parsedDict["name"].ToString(), Is.EqualTo("bob"));
            Assert.That(Convert.ToInt32(parsedDict["age"]), Is.EqualTo(42));
        }

        [Test]
        public void Array_Serialization_Works()
        {
            var req = new XmlRpcRequest();
            var array = new List<object> { "a", "b", 3 };

            if (req.Params is IList legacy)
            {
                legacy.Add(array);
            }

            var xml = new XmlRpcRequestSerializer().Serialize(req);
            var parsed = (XmlRpcRequest)new XmlRpcRequestDeserializer().Deserialize(new StringReader(xml));

            Assert.That(parsed.Params.Count, Is.EqualTo(1));
            var item = parsed.Params[0];
            Assert.That(item, Is.InstanceOf<IList>());

            var parsedList = item as IList;
            Assert.That(parsedList.Count, Is.EqualTo(3));
            Assert.That(parsedList[0].ToString(), Is.EqualTo("a"));
            Assert.That(parsedList[1].ToString(), Is.EqualTo("b"));
            Assert.That(Convert.ToInt32(parsedList[2]), Is.EqualTo(3));
        }

        [Test]
        public void FaultResponse_RoundTrip_Works()
        {
            var resp = new XmlRpcResponse();
            resp.SetFault(123, "failure reason");

            var xml = XmlRpcResponseSerializer.Singleton.Serialize(resp);
            var parsed = (XmlRpcResponse)new XmlRpcResponseDeserializer().Deserialize(new StringReader(xml));

            Assert.That(parsed.IsFault, Is.True);
            Assert.That(parsed.FaultCode, Is.EqualTo(123));
            Assert.That(parsed.FaultString, Is.EqualTo("failure reason"));
        }

        [Test]
        public void BoxcarRequest_Serialization_Works()
        {
            var box = new XmlRpcBoxcarRequest();
            var r1 = new XmlRpcRequest("one.sum", new ArrayList {1,2});
            var r2 = new XmlRpcRequest("two.concat", new ArrayList {"x","y"});
            box.Requests.Add(r1);
            box.Requests.Add(r2);

            var xml = new XmlRpcRequestSerializer().Serialize(box);
            var parsed = (XmlRpcRequest)new XmlRpcRequestDeserializer().Deserialize(new StringReader(xml));

            Assert.That(parsed.MethodName, Is.EqualTo("system.multiCall"));
            // parsed.Params should be a list of structs
            Assert.That(parsed.Params.Count, Is.EqualTo(2));

            var first = parsed.Params[0] as IDictionary;
            Assert.That(first, Is.Not.Null);
            Assert.That(first[XmlRpcXmlTokens.METHOD_NAME].ToString(), Is.EqualTo("one.sum"));
            Assert.That(first.Contains(XmlRpcXmlTokens.PARAMS));

            var second = parsed.Params[1] as IDictionary;
            Assert.That(second, Is.Not.Null);
            Assert.That(second[XmlRpcXmlTokens.METHOD_NAME].ToString(), Is.EqualTo("two.concat"));
        }
    }
}
