using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace XmlRpcCore
{
    /// <summary>Class supporting the request side of an XML-RPC transaction.</summary>
    public class XmlRpcRequest
    {
        private readonly XmlRpcResponseDeserializer _deserializer = new XmlRpcResponseDeserializer();
        private readonly Encoding _encoding = new ASCIIEncoding();

        /// <summary><c>ArrayList</c> containing the parameters.</summary>
        protected IList _params;

        private readonly XmlRpcRequestSerializer _serializer = new XmlRpcRequestSerializer();

        /// <summary>Instantiate an <c>XmlRpcRequest</c></summary>
        public XmlRpcRequest()
        {
            _params = new ArrayList();
        }

        /// <summary>Instantiate an <c>XmlRpcRequest</c> for a specified method and parameters.</summary>
        /// <param name="methodName">
        ///     <c>String</c> designating the <i>object.method</i> on the server the request
        ///     should be directed to.
        /// </param>
        /// <param name="parameters"><c>ArrayList</c> of XML-RPC type parameters to invoke the request with.</param>
        public XmlRpcRequest(string methodName, IList parameters)
        {
            MethodName = methodName;
            _params = parameters;
        }

        /// <summary><c>ArrayList</c> conntaining the parameters for the request.</summary>
        public virtual IList Params
        {
            get { return _params; }
        }

        /// <summary><c>String</c> conntaining the method name, both object and method, that the request will be sent to.</summary>
        public virtual string MethodName { get; set; }

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
        /// <param name="url"><c>String</c> The url of the XML-RPC server.</param>
        /// <returns><c>Object</c> The value returned from the method invocation on the server.</returns>
        /// <exception cref="XmlRpcException">If an exception generated on the server side.</exception>
        public object Invoke(string url)
        {
            var res = Send(url);

            if (res.IsFault)
                throw new XmlRpcException(res.FaultCode, res.FaultString);

            return res.Value;
        }

        /// <summary>Send the request to the server.</summary>
        /// <param name="url"><c>String</c> The url of the XML-RPC server.</param>
        /// <param name="timeout">AMount of time in milliseconds to wait for timeout</param>
        /// <returns><c>XmlRpcResponse</c> The response generated.</returns>
        public XmlRpcResponse Send(string url, int timeout = 0)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            if (request == null)
                throw new XmlRpcException(XmlRpcErrorCodes.TRANSPORT_ERROR,
                    XmlRpcErrorCodes.TRANSPORT_ERROR_MSG + ": Could not create request with " + url);
            request.Method = "POST";
            request.ContentType = "text/xml";
            request.AllowWriteStreamBuffering = true;
            if (timeout > 0)
                request.Timeout = timeout;

            var stream = request.GetRequestStream();
            var xml = new XmlTextWriter(stream, _encoding);
            _serializer.Serialize(xml, this);
            xml.Flush();
            xml.Close();

            var response = (HttpWebResponse) request.GetResponse();
            var input = new StreamReader(response.GetResponseStream());

            var resp = (XmlRpcResponse) _deserializer.Deserialize(input);
            input.Close();
            response.Close();
            return resp;
        }

        /// <summary>Produce <c>String</c> representation of the object.</summary>
        /// <returns><c>String</c> representation of the object.</returns>
        public override string ToString()
        {
            return _serializer.Serialize(this);
        }
    }
}