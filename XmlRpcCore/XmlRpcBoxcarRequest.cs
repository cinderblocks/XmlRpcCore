// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

using System.Collections.Generic;

namespace XmlRpcCore
{
    /// <summary>Class that collects individual <c>XmlRpcRequest</c> objects and submits them as a <i>boxcarred</i> request.</summary>
    /// <remarks>
    ///     A boxcared request is when a number of request are collected before being sent via XML-RPC, and then are sent via
    ///     a single HTTP connection. This results in a speed up from reduced connection time.  The results are then returned
    ///     collectively as well.
    /// </remarks>
    /// <seealso cref="XmlRpcRequest" />
    public class XmlRpcBoxcarRequest : XmlRpcRequest
    {
        /// <summary>List to collect the requests to boxcar.</summary>
        public readonly IList<XmlRpcRequest> Requests = new List<XmlRpcRequest>();

        /// <summary>Returns the <c>String</c> "system.multiCall" which is the server method that handles boxcars.</summary>
        public override string MethodName => "system.multiCall";

        /// <summary>The list of boxcarred <paramref>Requests</paramref> as properly formed parameters.</summary>
        public override IList<object> Params
        {
            get
            {
                var reqList = new List<object>();
                foreach (XmlRpcRequest request in Requests)
                {
                    var requestEntry = new Dictionary<string, object>
                    {
                        { XmlRpcXmlTokens.METHOD_NAME, request.MethodName },
                        { XmlRpcXmlTokens.PARAMS, request.Params }
                    };
                    reqList.Add(requestEntry);
                }

                return reqList;
            }
        }
    }
}