using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace XmlRpcCore.Tests
{
    [TestFixture]
    public class XmlRpcRequestTests
    {
        [Test]
        public void Serialize_ToString_IncludesMethodName()
        {
            var req = new XmlRpcCore.XmlRpcRequest("demo.sayHello", new List<object> { "World" });
            var xml = req.ToString();
            Assert.That(xml, Does.Contain("<methodName>demo.sayHello</methodName>"));
        }

        [Test]
        public void MethodNameObjectAndMethod_AreSplitCorrectly_WithDot()
        {
            var req = new XmlRpcCore.XmlRpcRequest("obj.method", new List<object>());
            Assert.That(req.MethodNameObject, Is.EqualTo("obj"));
            Assert.That(req.MethodNameMethod, Is.EqualTo("method"));
        }

        [Test]
        public void MethodNameObjectAndMethod_AreWholeWhenNoDot()
        {
            var req = new XmlRpcCore.XmlRpcRequest("single", new List<object>());
            Assert.That(req.MethodNameObject, Is.EqualTo("single"));
            Assert.That(req.MethodNameMethod, Is.EqualTo("single"));
        }

        [Test]
        public void Invoke_ThrowsOnFaultResponse()
        {
            // simulate an HttpClient that returns a fault response
            var handler = new TestHttpMessageHandler((requestMsg) =>
            {
                var responseXml = "<?xml version=\"1.0\"?><methodResponse><fault><value><struct><member><name>faultCode</name><value><i4>4</i4></value></member><member><name>faultString</name><value><string>Too many params.</string></value></member></struct></value></fault></methodResponse>";
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent(responseXml, Encoding.UTF8, "text/xml")
                };
            });

            var client = new System.Net.Http.HttpClient(handler);
            var req = new XmlRpcCore.XmlRpcRequest("demo", new List<object>());

            Assert.ThrowsAsync<XmlRpcCore.XmlRpcException>(async () => await req.Invoke(client, "http://localhost"));
        }

        [Test]
        public void PostAsXmlRpcAsync_Serializes_Request_And_Parses_Response()
        {
            var handler = new TestHttpMessageHandler((httpReq) =>
            {
                // read request body to ensure it contains RPC payload
                var body = httpReq.Content.ReadAsStringAsync().Result;
                Assert.That(body, Does.Contain("<methodName>demo</methodName>"));

                var responseXml = "<?xml version=\"1.0\"?><methodResponse><params><param><value><string>OK</string></value></param></params></methodResponse>";
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent(responseXml, Encoding.UTF8, "text/xml")
                };
            });

            var client = new System.Net.Http.HttpClient(handler);
            var req = new XmlRpcCore.XmlRpcRequest("demo", new List<object>());

            var task = XmlRpcCore.XmlRpcHttpClientExtensions.PostAsXmlRpcAsync(client, "http://localhost", req);
            task.Wait();
            var res = task.Result;
            Assert.That(res.Value, Is.EqualTo("OK"));
        }
    }

    internal class TestHttpMessageHandler : System.Net.Http.HttpMessageHandler
    {
        private readonly Func<System.Net.Http.HttpRequestMessage, System.Net.Http.HttpResponseMessage> _responder;

        public TestHttpMessageHandler(Func<System.Net.Http.HttpRequestMessage, System.Net.Http.HttpResponseMessage> responder)
        {
            _responder = responder ?? throw new ArgumentNullException(nameof(responder));
        }

        protected override System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return System.Threading.Tasks.Task.FromResult(_responder(request));
        }
    }
}
