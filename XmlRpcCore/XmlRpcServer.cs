using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace XmlRpcCore
{
    /// <summary>A restricted HTTP server for use with XML-RPC.</summary>
    /// <remarks>
    ///     It only handles POST requests, and only POSTs representing XML-RPC calls.
    ///     In addition to dispatching requests it also provides a registry for request handlers.
    /// </remarks>
    public class XmlRpcServer : IEnumerable
    {
        private const int RESPONDER_COUNT = 10;
        private readonly IDictionary _handlers;
        private readonly int _port;
        private readonly WaitCallback _wc;
        private IPAddress _address;
        private TcpListener _myListener;
        private XmlRpcSystemObject _system;

        /// <summary>Constructor with port and address.</summary>
        /// <remarks>
        ///     This constructor sets up a TcpListener listening on the
        ///     given port and address. It also calls a Thread on the method StartListen().
        /// </remarks>
        /// <param name="address"><c>IPAddress</c> value of the address to listen on.</param>
        /// <param name="port"><c>Int</c> value of the port to listen on.</param>
        public XmlRpcServer(IPAddress address, int port)
        {
            _port = port;
            _address = address;
            _handlers = new Hashtable();
            _system = new XmlRpcSystemObject(this);
            _wc = WaitCallback;
        }

        /// <summary>Basic constructor.</summary>
        /// <remarks>
        ///     This constructor sets up a TcpListener listening on the
        ///     given port. It also calls a Thread on the method StartListen(). IPAddress.Any
        ///     is assumed as the address here.
        /// </remarks>
        /// <param name="port"><c>Int</c> value of the port to listen on.</param>
        public XmlRpcServer(int port) : this(IPAddress.Any, port)
        {
        }

        /// <summary>Retrieve a handler by name.</summary>
        /// <param name="name"><c>String</c> naming a handler</param>
        /// <returns><c>Object</c> that is the handler.</returns>
        public object this[string name]
        {
            get { return _handlers[name]; }
        }

        /// <summary>Get an enumeration of my XML-RPC handlers.</summary>
        /// <returns><c>IEnumerable</c> the handler enumeration.</returns>
        public IEnumerator GetEnumerator()
        {
            return _handlers.GetEnumerator();
        }

        /// <summary>Start the server.</summary>
        public void Start()
        {
            try
            {
                Stop();
                //start listing on the given port
                //	    IPAddress addr = IPAddress.Parse("127.0.0.1");
                lock (this)
                {
                    _myListener = new TcpListener(IPAddress.Any, _port);
                    _myListener.Start();
                    //start the thread which calls the method 'StartListen'
                    var th = new Thread(StartListen);
                    th.Start();
                }
            }
            catch (Exception e)
            {
                Logger.WriteEntry("An Exception Occurred while Listening :" + e, LogLevel.Error);
            }
        }

        /// <summary>Stop the server.</summary>
        public void Stop()
        {
            try
            {
                if (_myListener != null)
                    lock (this)
                    {
                        _myListener.Stop();
                        _myListener = null;
                    }
            }
            catch (Exception e)
            {
                Logger.WriteEntry("An Exception Occurred while stopping :" +
                                  e, LogLevel.Error);
            }
        }

        /// <summary>
        ///     This method Accepts new connections and dispatches them when appropriate.
        /// </summary>
        public void StartListen()
        {
            while (true && _myListener != null)
            {
                //Accept a new connection
                var responder = new XmlRpcResponder(this, _myListener.AcceptTcpClient());
                ThreadPool.QueueUserWorkItem(_wc, responder);
            }
        }


        /// <summary>
        ///     Add an XML-RPC handler object by name.
        /// </summary>
        /// <param name="name"><c>String</c> XML-RPC dispatch name of this object.</param>
        /// <param name="obj"><c>Object</c> The object that is the XML-RPC handler.</param>
        public void Add(string name, object obj)
        {
            _handlers.Add(name, obj);
        }

        ///<summary>Return a C# object.method name for and XML-RPC object.method name pair.</summary>
        ///<param name="methodName">The XML-RPC object.method.</param>
        ///<returns><c>String</c> of form object.method for the underlying C# method.</returns>
        public string MethodName(string methodName)
        {
            var dotAt = methodName.LastIndexOf('.');

            if (dotAt == -1)
                throw new XmlRpcException(XmlRpcErrorCodes.SERVER_ERROR_METHOD,
                    XmlRpcErrorCodes.SERVER_ERROR_METHOD_MSG + ": Bad method name " + methodName);

            var objectName = methodName.Substring(0, dotAt);
            var target = _handlers[objectName];

            if (target == null)
                throw new XmlRpcException(XmlRpcErrorCodes.SERVER_ERROR_METHOD,
                    XmlRpcErrorCodes.SERVER_ERROR_METHOD_MSG + ": Object " + objectName + " not found");

            return target.GetType().FullName + "." + methodName.Substring(dotAt + 1);
        }

        /// <summary>Invoke a method described in a request.</summary>
        /// <param name="req"><c>XmlRpcRequest</c> containing a method descriptions.</param>
        /// <seealso cref="XmlRpcSystemObject.Invoke" />
        /// <seealso cref="XmlRpcServer.Invoke(string,string,IList)" />
        public object Invoke(XmlRpcRequest req)
        {
            return Invoke(req.MethodNameObject, req.MethodNameMethod, req.Params);
        }

        /// <summary>Invoke a method on a named handler.</summary>
        /// <param name="objectName"><c>String</c> The name of the handler.</param>
        /// <param name="methodName"><c>String</c> The name of the method to invoke on the handler.</param>
        /// <param name="parameters"><c>IList</c> The parameters to invoke the method with.</param>
        /// <seealso cref="XmlRpcSystemObject.Invoke" />
        public object Invoke(string objectName, string methodName, IList parameters)
        {
            var target = _handlers[objectName];

            if (target == null)
                throw new XmlRpcException(XmlRpcErrorCodes.SERVER_ERROR_METHOD,
                    XmlRpcErrorCodes.SERVER_ERROR_METHOD_MSG + ": Object " + objectName + " not found");

            return XmlRpcSystemObject.Invoke(target, methodName, parameters);
        }

        /// <summary>The method the thread pool invokes when a thread is available to handle an HTTP request.</summary>
        /// <param name="responder">TcpClient from the socket accept.</param>
        public void WaitCallback(object responder)
        {
            var resp = (XmlRpcResponder) responder;

            if (resp.HttpReq.HttpMethod == "POST")
                try
                {
                    resp.Respond();
                }
                catch (Exception e)
                {
                    Logger.WriteEntry("Failed on post: " + e, LogLevel.Error);
                }
            else
                Logger.WriteEntry("Only POST methods are supported: " + resp.HttpReq.HttpMethod +
                                  " ignored", LogLevel.Error);

            resp.Close();
        }

        /// <summary>
        ///     This function send the Header Information to the client (Browser)
        /// </summary>
        /// <param name="sHttpVersion">HTTP Version</param>
        /// <param name="sMIMEHeader">Mime Type</param>
        /// <param name="iTotBytes">Total Bytes to be sent in the body</param>
        /// <param name="sStatusCode"></param>
        /// <param name="output">Socket reference</param>
        public static void HttpHeader(string sHttpVersion, string sMIMEHeader, long iTotBytes, string sStatusCode,
            TextWriter output)
        {
            var sBuffer = "";

            // if Mime type is not provided set default to text/html
            if (sMIMEHeader.Length == 0)
                sMIMEHeader = "text/html"; // Default Mime Type is text/html

            sBuffer += sHttpVersion + sStatusCode + "\r\n";
            sBuffer += "Connection: close\r\n";
            if (iTotBytes > 0)
                sBuffer += "Content-Length: " + iTotBytes + "\r\n";
            sBuffer += "Server: XmlRpcServer \r\n";
            sBuffer += "Content-Type: " + sMIMEHeader + "\r\n";
            sBuffer += "\r\n";

            output.Write(sBuffer);
        }
    }
}