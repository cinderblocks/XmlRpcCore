using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;

namespace XmlRpcCore
{
    /// <summary>Class supporting the request side of an XML-RPC transaction.</summary>
    public class XmlRpcRequest
    {
        private readonly Lazy<XmlRpcRequestSerializer> _serializer =
            new Lazy<XmlRpcRequestSerializer>(() => new XmlRpcRequestSerializer());

        // XXX: workaround virtual method call in constructor
        private string _methodName = "";

        /// <summary>Instantiate an <c>XmlRpcRequest</c></summary>
        public XmlRpcRequest()
        {
            Params = new ArrayList();
        }

        /// <summary>Instantiate an <c>XmlRpcRequest</c> for a specified method and parameters.</summary>
        /// <param name="methodName">
        ///     <c>String</c> designating the <i>object.method</i> on the server the request
        ///     should be directed to.
        /// </param>
        /// <param name="parameters"><c>ArrayList</c> of XML-RPC type parameters to invoke the request with.</param>
        public XmlRpcRequest(string methodName, IList parameters)
        {
            _methodName = methodName;
            Params = parameters;
        }

        /// <summary><c>ArrayList</c> containing the parameters for the request.</summary>
        public virtual IList Params { get; }

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

                if (index == -1)
                    return MethodName;

                return MethodName.Substring(0, index);
            }
        }

        /// <summary><c>String</c> method name portion of the object.method name.</summary>
        public string MethodNameMethod
        {
            get
            {
                var index = MethodName.IndexOf(".", StringComparison.Ordinal);

                if (index == -1)
                    return MethodName;

                return MethodName.Substring(index + 1, MethodName.Length - index - 1);
            }
        }

        /// <summary>Invoke this request on the server.</summary>
        /// <param name="client"><c>HttpClient</c> HttpClient object to use for the request.</param>
        /// <param name="url"><c>String</c> The url of the XML-RPC server.</param>
        /// <returns><c>Object</c> The value returned from the method invocation on the server.</returns>
        /// <exception cref="XmlRpcException">If an exception generated on the server side.</exception>
        public async Task<object> Invoke(HttpClient client, string url)
        {
            var res = await client.PostAsXmlRpcAsync(url, this);

            if (res.IsFault)
                throw new XmlRpcException(res.FaultCode, res.FaultString);

            return res.Value;
        }

        /// <summary>Produce <c>String</c> representation of the object.</summary>
        /// <returns><c>String</c> representation of the object.</returns>
        public override string ToString()
        {
            return _serializer.Value.Serialize(this);
        }
    }
}