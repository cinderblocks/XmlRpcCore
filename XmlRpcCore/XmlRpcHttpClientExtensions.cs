using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XmlRpcCore
{
    public static class XmlRpcHttpClientExtensions
    {
        private static readonly XmlRpcResponseDeserializer Deserializer = new XmlRpcResponseDeserializer();
        private static readonly XmlRpcRequestSerializer Serializer = new XmlRpcRequestSerializer();
        
        /// <summary>Send a XmlRpcRequest to the server.</summary>
        /// <param name="client"><c>HttpClient</c> HttpClient used to submit the request</param>
        /// <param name="requestUri"><c>String</c> The uri of the XML-RPC server.</param>
        /// <param name="rpcRequest"><c>XmlRpcRequest</c> The rpc request object to send</param>
        /// <returns><c>XmlRpcResponse</c> The response generated.</returns>
        public static async Task<XmlRpcResponse> PostAsXmlRpcAsync(this HttpClient client, string requestUri, XmlRpcRequest rpcRequest)
        {
            await using var stream = new MemoryStream();
            using (var writer = new XmlTextWriter(stream, Encoding.ASCII))
            {
                Serializer.Serialize(writer, rpcRequest);
            }

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(requestUri),
                Content = new StreamContent(stream)
            };
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");

            var response = await client.SendAsync(request).ConfigureAwait(false);

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(contentStream);
            return (XmlRpcResponse)Deserializer.Deserialize(streamReader);
        }
    }
}