using System;
using System.Net.Sockets;
using System.Xml;

namespace XmlRpcCore
{
    /// <summary>The class is a container of the context of an XML-RPC dialog on the server side.</summary>
    /// <remarks>
    ///     Instances of this class maintain the context for an individual XML-RPC server
    ///     side dialog. Namely they manage an inbound deserializer and an outbound serializer.
    /// </remarks>
    public class XmlRpcResponder
    {
        private TcpClient _client;
        private readonly XmlRpcRequestDeserializer _deserializer = new XmlRpcRequestDeserializer();
        private readonly XmlRpcResponseSerializer _serializer = new XmlRpcResponseSerializer();
        private readonly XmlRpcServer _server;

        /// <summary>Basic constructor.</summary>
        /// <param name="server">XmlRpcServer that this XmlRpcResponder services.</param>
        /// <param name="client">TcpClient with the connection.</param>
        public XmlRpcResponder(XmlRpcServer server, TcpClient client)
        {
            _server = server;
            _client = client;
            HttpReq = new SimpleHttpRequest(_client);
        }

        /// <summary>The SimpleHttpRequest based on the TcpClient.</summary>
        public SimpleHttpRequest HttpReq { get; private set; }

        /// <summary>Call close to insure proper shutdown.</summary>
        ~XmlRpcResponder()
        {
            Close();
        }

        ///<summary>Respond using this responders HttpReq.</summary>
        public void Respond()
        {
            Respond(HttpReq);
        }

        /// <summary>Handle an HTTP request containing an XML-RPC request.</summary>
        /// <remarks>
        ///     This method deserializes the XML-RPC request, invokes the
        ///     described method, serializes the response (or fault) and sends the XML-RPC response
        ///     back as a valid HTTP page.
        /// </remarks>
        /// <param name="httpReq"><c>SimpleHttpRequest</c> containing the request.</param>
        public void Respond(SimpleHttpRequest httpReq)
        {
            var xmlRpcReq = (XmlRpcRequest) _deserializer.Deserialize(httpReq.Input);
            var xmlRpcResp = new XmlRpcResponse();

            try
            {
                xmlRpcResp.Value = _server.Invoke(xmlRpcReq);
            }
            catch (XmlRpcException e)
            {
                xmlRpcResp.SetFault(e.FaultCode, e.FaultString);
            }
            catch (Exception e2)
            {
                xmlRpcResp.SetFault(XmlRpcErrorCodes.APPLICATION_ERROR,
                    XmlRpcErrorCodes.APPLICATION_ERROR_MSG + ": " + e2.Message);
            }

            if (Logger.Delegate != null)
                Logger.WriteEntry(xmlRpcResp.ToString(), LogLevel.Information);

            XmlRpcServer.HttpHeader(httpReq.Protocol, "text/xml", 0, " 200 OK", httpReq.Output);
            httpReq.Output.Flush();
            var xml = new XmlTextWriter(httpReq.Output);
            _serializer.Serialize(xml, xmlRpcResp);
            xml.Flush();
            httpReq.Output.Flush();
        }

        ///<summary>Close all contained resources, both the HttpReq and client.</summary>
        public void Close()
        {
            if (HttpReq != null)
            {
                HttpReq.Close();
                HttpReq = null;
            }

            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }
    }
}