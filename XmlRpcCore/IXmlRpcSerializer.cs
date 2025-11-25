// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace XmlRpcCore
{
    public interface IXmlRpcSerializer
    {
        string SerializeRequest(XmlRpcRequest request);
        string SerializeResponse(XmlRpcResponse response);
        XmlRpcRequest DeserializeRequest(TextReader reader);
        XmlRpcResponse DeserializeResponse(TextReader reader);

        void SerializeRequest(XmlRpcRequest request, Stream output);
        void SerializeResponse(XmlRpcResponse response, Stream output);
        XmlRpcRequest DeserializeRequest(Stream input);
        XmlRpcResponse DeserializeResponse(Stream input);

        Task SerializeRequestAsync(XmlRpcRequest request, Stream output, CancellationToken cancellationToken = default);
        Task SerializeResponseAsync(XmlRpcResponse response, Stream output, CancellationToken cancellationToken = default);
        Task<XmlRpcRequest> DeserializeRequestAsync(Stream input, CancellationToken cancellationToken = default);
        Task<XmlRpcResponse> DeserializeResponseAsync(Stream input, CancellationToken cancellationToken = default);
    }
}
