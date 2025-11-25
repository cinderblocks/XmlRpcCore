// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace XmlRpcCore
{
    internal class XmlRpcStreamContent : HttpContent
    {
        private readonly XmlRpcRequest _coreRequest;
        private readonly IXmlRpcSerializer _serializer;

        public XmlRpcStreamContent(XmlRpcRequest request, IXmlRpcSerializer serializer)
        {
            _coreRequest = request;
            _serializer = serializer;
            Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            await _serializer.SerializeRequestAsync(_coreRequest, stream).ConfigureAwait(false);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
