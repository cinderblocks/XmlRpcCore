using System.Collections;

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
        /// <summary>ArrayList to collect the requests to boxcar.</summary>
        public readonly IList Requests = new ArrayList();

        /// <summary>Returns the <c>String</c> "system.multiCall" which is the server method that handles boxcars.</summary>
        public override string MethodName => "system.multiCall";

        /// <summary>The <c>ArrayList</c> of boxcarred <paramref>Requests</paramref> as properly formed parameters.</summary>
        public override IList Params
        {
            get
            {
                var reqArray = new ArrayList();
                foreach (XmlRpcRequest request in Requests)
                {
                    var requestEntry = new Hashtable
                    {
                        { XmlRpcXmlTokens.METHOD_NAME, request.MethodName },
                        { XmlRpcXmlTokens.PARAMS, request.Params }
                    };
                    reqArray.Add(requestEntry);
                }

                return reqArray;
            }
        }
    }
}