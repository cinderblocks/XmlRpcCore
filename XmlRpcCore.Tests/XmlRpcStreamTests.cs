using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace XmlRpcCore.Tests
{
    [TestFixture]
    public class XmlRpcStreamTests
    {
        [Test]
        public async Task SerializeRequestAsync_Writes_And_DeserializeRequestAsync_Reads()
        {
            var ser = new XmlRpcNetSerializer();
            var req = new XmlRpcRequest("demo.sayHello", new List<object> { "World", 42 });

            using (var ms = new MemoryStream())
            {
                await ser.SerializeRequestAsync(req, ms).ConfigureAwait(false);
                ms.Position = 0;
                var parsed = await ser.DeserializeRequestAsync(ms).ConfigureAwait(false);
                Assert.That(parsed.MethodName, Is.EqualTo("demo.sayHello"));
                Assert.That(parsed.Params, Has.Count.EqualTo(2));
                Assert.That(parsed.Params[0], Is.EqualTo("World"));
                Assert.That(parsed.Params[1], Is.EqualTo(42));
            }
        }

        [Test]
        public async Task SerializeResponseAsync_Writes_And_DeserializeResponseAsync_Reads()
        {
            var ser = new XmlRpcNetSerializer();
            var resp = new XmlRpcResponse { Value = "OK" };

            using (var ms = new MemoryStream())
            {
                await ser.SerializeResponseAsync(resp, ms).ConfigureAwait(false);
                ms.Position = 0;
                var parsed = await ser.DeserializeResponseAsync(ms).ConfigureAwait(false);
                Assert.That(parsed.Value, Is.EqualTo("OK"));
            }
        }

        [Test]
        public async Task HttpClient_PostAsXmlRpcAsync_Uses_Streamed_Serialization()
        {
            var ser = new XmlRpcNetSerializer();

            var handler = new TestHttpMessageHandler((request) =>
            {
                // read request stream and parse
                var reqStream = request.Content.ReadAsStreamAsync().Result;
                var parsedReq = ser.DeserializeRequestAsync(reqStream).GetAwaiter().GetResult();
                Assert.That(parsedReq.MethodName, Is.EqualTo("demo"));

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var ms = new MemoryStream();
                ser.SerializeResponseAsync(new XmlRpcResponse { Value = "OK" }, ms).GetAwaiter().GetResult();
                ms.Position = 0;
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");
                return response;
            });

            var client = new HttpClient(handler);
            var coreReq = new XmlRpcRequest("demo", new System.Collections.Generic.List<object>());

            var res = await XmlRpcCore.XmlRpcHttpClientExtensions.PostAsXmlRpcAsync(client, "http://localhost", coreReq, ser).ConfigureAwait(false);
            Assert.That(res.Value, Is.EqualTo("OK"));
        }

        [Test]
        public async Task XmlRpcRequest_InvokeAsync_Uses_Streamed_Serialization()
        {
            var ser = new XmlRpcNetSerializer();

            var handler = new TestHttpMessageHandler((request) =>
            {
                var reqStream = request.Content.ReadAsStreamAsync().Result;
                var parsedReq = ser.DeserializeRequestAsync(reqStream).GetAwaiter().GetResult();
                Assert.That(parsedReq.MethodName, Is.EqualTo("demo.invoke"));

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var ms = new MemoryStream();
                ser.SerializeResponseAsync(new XmlRpcResponse { Value = "RESULT" }, ms).GetAwaiter().GetResult();
                ms.Position = 0;
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");
                return response;
            });

            var client = new HttpClient(handler);
            var req = new XmlRpcRequest("demo.invoke", new System.Collections.Generic.List<object>());

            var result = await req.InvokeAsync(client, "http://localhost", ser).ConfigureAwait(false);
            Assert.That(result, Is.EqualTo("RESULT"));
        }

        // Copy of TestHttpMessageHandler to keep tests isolated
        internal class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

            public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            {
                _responder = responder ?? throw new ArgumentNullException(nameof(responder));
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                return Task.FromResult(_responder(request));
            }
        }
    }
}
