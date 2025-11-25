// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;

namespace XmlRpcCore
{
    /// <summary>Class supporting the request side of an XML-RPC transaction.</summary>
    public class XmlRpcRequest
    {
        private readonly IXmlRpcSerializer _injectedSerializer;
        private readonly ILogger _logger;

        // XXX: workaround virtual method call in constructor
        private string _methodName = "";

        /// <summary>Instantiate an <c>XmlRpcRequest</c></summary>
        public XmlRpcRequest()
        {
            Params = new List<object>();
        }

        /// <summary>Instantiate an <c>XmlRpcRequest</c> for a specified method and parameters.</summary>
        public XmlRpcRequest(string methodName, IEnumerable<object> parameters)
        {
            _methodName = methodName;
            Params = parameters == null ? new List<object>() : new List<object>(parameters);
        }

        /// <summary>Legacy constructor accepting non-generic ArrayList for migration. Use generic overload instead.</summary>
        [Obsolete("Use XmlRpcRequest(string, IEnumerable<object>) with a generic list instead.")]
        public XmlRpcRequest(string methodName, ArrayList parameters)
        {
            _methodName = methodName;
            if (parameters == null)
            {
                Params = new List<object>();
            }
            else
            {
                var list = new List<object>(parameters.Count);
                foreach (var o in parameters)
                    list.Add(o);
                Params = list;
            }
        }

        /// <summary>Construct with injected serializer and logger for DI scenarios.</summary>
        public XmlRpcRequest(string methodName, IEnumerable<object> parameters, IXmlRpcSerializer serializer, ILogger logger = null) : this(methodName, parameters)
        {
            _injectedSerializer = serializer;
            _logger = logger;
        }

        /// <summary><c>IList&lt;object&gt;</c> containing the parameters for the request.</summary>
        public virtual IList<object> Params { get; }

        /// <summary><c>String</c> containing the method name, both object and method, that the request will be sent to.</summary>
        public virtual string MethodName
        {
            get => _methodName;
            set => _methodName = value;
        }

        /// <summary><c>String</c> object name portion of the method name.</summary>
        public string MethodNameObject
        {
            get
            {
                var index = MethodName.IndexOf(".", StringComparison.Ordinal);

                return index == -1 ? MethodName : MethodName.Substring(0, index);
            }
        }

        /// <summary><c>String</c> method name portion of the object.method name.</summary>
        public string MethodNameMethod
        {
            get
            {
                var index = MethodName.IndexOf(".", StringComparison.Ordinal);

                return index == -1 
                    ? MethodName 
                    : MethodName.Substring(index + 1, MethodName.Length - index - 1);
            }
        }

        /// <summary>Invoke this request on the server.</summary>
        public async Task<object> Invoke(HttpClient client, string url)
        {
            var res = await client.PostAsXmlRpcAsync(url, this);

            if (res.IsFault)
            {
                throw new XmlRpcException(res.FaultCode, res.FaultString);
            }

            return res.Value;
        }

        /// <summary>Invoke async with cancellation and serializer injection.</summary>
        public async Task<object> InvokeAsync(HttpClient client, string url, IXmlRpcSerializer serializer = null, CancellationToken cancellationToken = default)
        {
            IXmlRpcSerializer ser = (serializer ?? _injectedSerializer) ?? new XmlRpcNetSerializer();
            if (_logger != null)
                _logger.LogDebug("Invoking XmlRpc method {Method}", MethodName);

            var res = await client.PostAsXmlRpcAsync(url, this, ser, cancellationToken).ConfigureAwait(false);

            if (res.IsFault)
            {
                if (_logger != null)
                    _logger.LogWarning("XmlRpc fault {FaultCode}: {FaultString}", res.FaultCode, res.FaultString);
                throw new XmlRpcException(res.FaultCode, res.FaultString);
            }

            return res.Value;
        }

        /// <summary>Generic Invoke that attempts to convert the returned value to T.</summary>
        public async Task<T> InvokeAsync<T>(HttpClient client, string url, IXmlRpcSerializer serializer = null, CancellationToken cancellationToken = default)
        {
            var result = await InvokeAsync(client, url, serializer, cancellationToken).ConfigureAwait(false);
            if (result == null) return default(T);
            return (T)Convert.ChangeType(result, typeof(T));
        }

        /// <summary>Produce <c>String</c> representation of the object using a serializer.</summary>
        public string ToString(IXmlRpcSerializer serializer)
        {
            var ser = serializer ?? _injectedSerializer ?? new XmlRpcNetSerializer();
            return ser.SerializeRequest(this);
        }

        public override string ToString()
        {
            return ToString(new XmlRpcNetSerializer());
        }
    }
}
