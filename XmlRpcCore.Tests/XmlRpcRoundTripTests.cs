using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace XmlRpcCore.Tests
{
    [TestFixture]
    public class XmlRpcRoundTripTests
    {
        [Test]
        public void Request_RoundTrip_String_Int_Bool()
        {
            var reqString = new XmlRpcCore.XmlRpcRequest("m", new List<object> { "hello" });
            var xml = reqString.ToString();
            var parsed = (XmlRpcCore.XmlRpcRequest)new XmlRpcCore.XmlRpcRequestDeserializer().Deserialize(xml);
            Assert.That(parsed.Params[0], Is.EqualTo("hello"));

            var reqInt = new XmlRpcCore.XmlRpcRequest("m", new List<object> { 42 });
            xml = reqInt.ToString();
            parsed = (XmlRpcCore.XmlRpcRequest)new XmlRpcCore.XmlRpcRequestDeserializer().Deserialize(xml);
            Assert.That(parsed.Params[0], Is.EqualTo(42));

            var reqBool = new XmlRpcCore.XmlRpcRequest("m", new List<object> { true });
            xml = reqBool.ToString();
            parsed = (XmlRpcCore.XmlRpcRequest)new XmlRpcCore.XmlRpcRequestDeserializer().Deserialize(xml);
            Assert.That(parsed.Params[0], Is.EqualTo(true));
        }

        [Test]
        public void Request_RoundTrip_Array_Struct()
        {
            var inner = new List<object> { 1, "two", false };
            var reqArray = new XmlRpcCore.XmlRpcRequest("m", new List<object> { inner });
            var xml = reqArray.ToString();
            var parsed = (XmlRpcCore.XmlRpcRequest)new XmlRpcCore.XmlRpcRequestDeserializer().Deserialize(xml);
            Assert.That(parsed.Params[0], Is.TypeOf<List<object>>());
            var arr = (List<object>)parsed.Params[0];
            Assert.That(arr[0], Is.EqualTo(1));
            Assert.That(arr[1], Is.EqualTo("two"));
            Assert.That(arr[2], Is.EqualTo(false));

            var dict = new Dictionary<string, object> { { "a", 1 }, { "b", "two" } };
            var reqStruct = new XmlRpcCore.XmlRpcRequest("m", new List<object> { dict });
            xml = reqStruct.ToString();
            parsed = (XmlRpcCore.XmlRpcRequest)new XmlRpcCore.XmlRpcRequestDeserializer().Deserialize(xml);
            Assert.That(parsed.Params[0], Is.TypeOf<Dictionary<string, object>>());
            var st = (Dictionary<string, object>)parsed.Params[0];
            Assert.That(st["a"], Is.EqualTo(1));
            Assert.That(st["b"], Is.EqualTo("two"));
        }

        [Test]
        public void Request_RoundTrip_Base64_DateTime()
        {
            var data = new byte[] { 1, 2, 3, 4 };
            var reqBase64 = new XmlRpcCore.XmlRpcRequest("m", new List<object> { data });
            var xml = reqBase64.ToString();
            var parsed = (XmlRpcCore.XmlRpcRequest)new XmlRpcCore.XmlRpcRequestDeserializer().Deserialize(xml);
            Assert.That(parsed.Params[0], Is.TypeOf<byte[]>());
            Assert.That((byte[])parsed.Params[0], Is.EqualTo(data));

            var dt = new DateTime(2020, 12, 31, 23, 59, 59);
            var reqDate = new XmlRpcCore.XmlRpcRequest("m", new List<object> { dt });
            xml = reqDate.ToString();
            parsed = (XmlRpcCore.XmlRpcRequest)new XmlRpcCore.XmlRpcRequestDeserializer().Deserialize(xml);
            Assert.That(parsed.Params[0], Is.TypeOf<DateTime>());
            Assert.That((DateTime)parsed.Params[0], Is.EqualTo(dt));
        }

        [Test]
        public void Response_RoundTrip_Primitives_And_Struct()
        {
            var resp = new XmlRpcCore.XmlRpcResponse { Value = "ok" };
            var xml = new XmlRpcCore.XmlRpcResponseSerializer().Serialize(resp);
            var parsed = (XmlRpcCore.XmlRpcResponse)new XmlRpcCore.XmlRpcResponseDeserializer().Deserialize(xml);
            Assert.That(parsed.Value, Is.EqualTo("ok"));

            var dict = new Dictionary<string, object> { { "x", 123 }, { "y", "z" } };
            resp = new XmlRpcCore.XmlRpcResponse { Value = dict };
            xml = new XmlRpcCore.XmlRpcResponseSerializer().Serialize(resp);
            parsed = (XmlRpcCore.XmlRpcResponse)new XmlRpcCore.XmlRpcResponseDeserializer().Deserialize(xml);
            Assert.That(parsed.Value, Is.TypeOf<Dictionary<string, object>>());
            var st = (Dictionary<string, object>)parsed.Value;
            Assert.That(st["x"], Is.EqualTo(123));
            Assert.That(st["y"], Is.EqualTo("z"));
        }
    }
}
