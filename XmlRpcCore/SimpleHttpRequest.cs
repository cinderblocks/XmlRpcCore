using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;

namespace XmlRpcCore
{
    /// <summary>Very basic HTTP request handler.</summary>
    /// <remarks>
    ///     This class is designed to accept a TcpClient and treat it as an HTTP request.
    ///     It will do some basic header parsing and manage the input and output streams associated
    ///     with the request.
    /// </remarks>
    public class SimpleHttpRequest
    {
        private string __filePath;
        private string _filePathDir;
        private string _filePathFile;
        private Hashtable _headers;

        /// <summary>A constructor which accepts the TcpClient.</summary>
        /// <remarks>
        ///     It creates the associated input and output streams, determines the request type,
        ///     and parses the remaining HTTP header.
        /// </remarks>
        /// <param name="client">The <c>TcpClient</c> associated with the HTTP connection.</param>
        public SimpleHttpRequest(TcpClient client)
        {
            Client = client;
            Output = new StreamWriter(client.GetStream());
            Input = new StreamReader(client.GetStream());
            GetRequestMethod();
            GetRequestHeaders();
        }

        /// <summary>The output <c>StreamWriter</c> associated with the request.</summary>
        public StreamWriter Output { get; }

        /// <summary>The input <c>StreamReader</c> associated with the request.</summary>
        public StreamReader Input { get; }

        /// <summary>The <c>TcpClient</c> with the request.</summary>
        public TcpClient Client { get; }

        private string _filePath
        {
            get => __filePath;
            set
            {
                __filePath = value;
                _filePathDir = null;
                _filePathFile = null;
            }
        }

        /// <summary>The type of HTTP request (i.e. PUT, GET, etc.).</summary>
        public string HttpMethod { get; private set; }

        /// <summary>The level of the HTTP protocol.</summary>
        public string Protocol { get; private set; }

        /// <summary>The "path" which is part of any HTTP request.</summary>
        public string FilePath => _filePath;

        /// <summary>The file portion of the "path" which is part of any HTTP request.</summary>
        public string FilePathFile
        {
            get
            {
                if (_filePathFile != null)
                    return _filePathFile;

                var i = FilePath.LastIndexOf("/", StringComparison.Ordinal);

                if (i == -1)
                    return "";

                i++;
                _filePathFile = FilePath.Substring(i, FilePath.Length - i);
                return _filePathFile;
            }
        }

        /// <summary>The directory portion of the "path" which is part of any HTTP request.</summary>
        public string FilePathDir
        {
            get
            {
                if (_filePathDir != null)
                    return _filePathDir;

                var i = FilePath.LastIndexOf("/", StringComparison.Ordinal);

                if (i == -1)
                    return "";

                i++;
                _filePathDir = FilePath.Substring(0, i);
                return _filePathDir;
            }
        }

        private void GetRequestMethod()
        {
            var req = Input.ReadLine();
            if (req == null)
                throw new ApplicationException("Void request.");

            if (0 == string.Compare("GET ", req.Substring(0, 4), StringComparison.Ordinal))
                HttpMethod = "GET";
            else if (0 == string.Compare("POST ", req.Substring(0, 5), StringComparison.Ordinal))
                HttpMethod = "POST";
            else
                throw new InvalidOperationException("Unrecognized method in query: " + req);

            req = req.TrimEnd();
            var idx = req.IndexOf(' ') + 1;
            if (idx >= req.Length)
                throw new ApplicationException("What do you want?");

            var page_protocol = req.Substring(idx);
            var idx2 = page_protocol.IndexOf(' ');
            if (idx2 == -1)
                idx2 = page_protocol.Length;

            _filePath = page_protocol.Substring(0, idx2).Trim();
            Protocol = page_protocol.Substring(idx2).Trim();
        }

        private void GetRequestHeaders()
        {
            string line;
            int idx;

            _headers = new Hashtable();

            while ((line = Input.ReadLine()) != "")
            {
                if (line == null)
                    break;

                idx = line.IndexOf(':');
                if (idx == -1 || idx == line.Length - 1)
                {
                    Logger.WriteEntry("Malformed header line: " + line, LogLevel.Information);
                    continue;
                }

                var key = line.Substring(0, idx);
                var value = line.Substring(idx + 1);

                try
                {
                    _headers.Add(key, value);
                }
                catch (Exception)
                {
                    Logger.WriteEntry("Duplicate header key in line: " + line, LogLevel.Information);
                }
            }
        }

        /// <summary>
        ///     Format the object contents into a useful string representation.
        /// </summary>
        /// <returns><c>String</c> representation of the <c>SimpleHttpRequest</c> as the <i>HttpMethod FilePath Protocol</i>.</returns>
        public override string ToString()
        {
            return HttpMethod + " " + FilePath + " " + Protocol;
        }

        /// <summary>
        ///     Close the <c>SimpleHttpRequest</c>. This flushes and closes all associated io streams.
        /// </summary>
        public void Close()
        {
            Output.Flush();
            Output.Close();
            Input.Close();
            Client.Close();
        }
    }
}