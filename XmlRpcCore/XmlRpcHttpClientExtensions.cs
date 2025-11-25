// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace XmlRpcCore
{
    public static class XmlRpcHttpClientExtensions
    {
        /// <summary>Send a XmlRpcRequest to the server using the provided serializer.</summary>
        public static Task<XmlRpcResponse> PostAsXmlRpcAsync(this HttpClient client, string requestUri,
            XmlRpcRequest rpcRequest, IXmlRpcSerializer serializer = null, CancellationToken cancellationToken = default)
        {
            return PostAsXmlRpcAsync(client, new Uri(requestUri), rpcRequest, serializer, cancellationToken);
        }

        public static async Task<XmlRpcResponse> PostAsXmlRpcAsync(this HttpClient client, Uri requestUri,
            XmlRpcRequest rpcRequest, IXmlRpcSerializer serializer = null, CancellationToken cancellationToken = default)
        {
            IXmlRpcSerializer ser = serializer ?? new XmlRpcNetSerializer();

            using (var content = new XmlRpcStreamContent(rpcRequest, ser))
            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content })
            using (var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var resp = await ser.DeserializeResponseAsync(stream, cancellationToken).ConfigureAwait(false);
                    if (resp == null)
                        return null;

                    if (resp.IsFault)
                    {
                        return new XmlRpcResponse { IsFault = true, FaultCode = resp.FaultCode, FaultString = resp.FaultString };
                    }

                    var core = new XmlRpcResponse();
                    core.Value = resp.Value;
                    return core;
                }
            }
        }
    }
}