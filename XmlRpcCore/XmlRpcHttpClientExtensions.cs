using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
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
        /// <param name="cancellationToken">
        ///     <c>CancellationToken</c>
        /// </param>
        /// <returns><c>XmlRpcResponse</c> The response generated.</returns>
        public static Task<XmlRpcResponse> PostAsXmlRpcAsync(this HttpClient client, string requestUri,
            XmlRpcRequest rpcRequest, CancellationToken cancellationToken = default)
        {
            return PostAsXmlRpcAsync(client, new Uri(requestUri), rpcRequest, cancellationToken);
        }

        /// <summary>Send a XmlRpcRequest to the server.</summary>
        /// <param name="client"><c>HttpClient</c> HttpClient used to submit the request</param>
        /// <param name="requestUri"><c>Uri</c> The uri of the XML-RPC server.</param>
        /// <param name="rpcRequest"><c>XmlRpcRequest</c> The rpc request object to send</param>
        /// <param name="cancellationToken">
        ///     <c>CancellationToken</c>
        /// </param>
        /// <returns><c>XmlRpcResponse</c> The response generated.</returns>
        public static async Task<XmlRpcResponse> PostAsXmlRpcAsync(this HttpClient client, Uri requestUri,
            XmlRpcRequest rpcRequest, CancellationToken cancellationToken = default)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream,
                    new XmlWriterSettings {Encoding = Encoding.ASCII, Indent = true}))
                {
                    Serializer.Serialize(writer, rpcRequest);
                }

                // reset position
                stream.Position = 0;

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = requestUri,
                    Content = new StreamContent(stream)
                };
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");

                var response = await client.SendAsync(request, cancellationToken)
                    .ConfigureAwait(false);

                var contentStream = await response.Content.ReadAsStreamAsync();
                using (var streamReader = new StreamReader(contentStream))
                {
                    return (XmlRpcResponse) Deserializer.Deserialize(streamReader);
                }
            }
        }
    }
}