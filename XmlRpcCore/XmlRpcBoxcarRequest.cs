using System.Collections;
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

        /// <summary>The non-generic IList of boxcarred <paramref>Requests</paramref> as properly formed parameters.</summary>
        [System.Obsolete("Use ParamsGeneric for a generic collection. This non-generic property is preserved for backward compatibility.", false)]
        public override IList Params
        {
            get
            {
                var reqArray = new List<object>();
                foreach (XmlRpcRequest request in Requests)
                {
                    var requestEntry = new Dictionary<string, object>
                    {
                        { XmlRpcXmlTokens.METHOD_NAME, request.MethodName },
                        { XmlRpcXmlTokens.PARAMS, request.Params }
                    };
                    reqArray.Add(requestEntry);
                }

                // return a non-generic ArrayList for backward compatibility
                return new ArrayList(reqArray);
            }
        }

        /// <summary>Generic view of Params to aid migration to generics.</summary>
        public override IList<object> ParamsGeneric
        {
            get
            {
                var reqArray = new List<object>();
                foreach (XmlRpcRequest request in Requests)
                {
                    var requestEntry = new Dictionary<string, object>
                    {
                        { XmlRpcXmlTokens.METHOD_NAME, request.MethodName },
                        { XmlRpcXmlTokens.PARAMS, request.ParamsGeneric }
                    };
                    reqArray.Add(requestEntry);
                }

                return reqArray;
            }
        }
    }
}