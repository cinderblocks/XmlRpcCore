// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace XmlRpcCore
{
    public partial class XmlRpcNetSerializer : IXmlRpcSerializer
    {
        // String-based APIs delegate to stream-based implementations
        public string SerializeRequest(XmlRpcRequest request)
        {
            using (var ms = new MemoryStream())
            {
                SerializeRequest(request, ms);
                ms.Position = 0;
                using (var sr = new StreamReader(ms, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public string SerializeResponse(XmlRpcResponse response)
        {
            using (var ms = new MemoryStream())
            {
                SerializeResponse(response, ms);
                ms.Position = 0;
                using (var sr = new StreamReader(ms, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public XmlRpcRequest DeserializeRequest(TextReader reader)
        {
            using (var xreader = XmlReader.Create(reader, XmlRpcSettings.CreateReaderSettings()))
            {
                return ParseRequest(xreader);
            }
        }

        public XmlRpcResponse DeserializeResponse(TextReader reader)
        {
            using (var xreader = XmlReader.Create(reader, XmlRpcSettings.CreateReaderSettings()))
            {
                return ParseResponse(xreader);
            }
        }

        // Stream-based implementations
        public void SerializeRequest(XmlRpcRequest request, Stream output)
        {
            using (var xw = XmlWriter.Create(output, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8, CloseOutput = false }))
            {
                xw.WriteStartDocument();
                xw.WriteStartElement("methodCall");
                xw.WriteElementString("methodName", request.MethodName);
                xw.WriteStartElement("params");
                foreach (var p in request.Params)
                {
                    xw.WriteStartElement("param");
                    xw.WriteStartElement("value");
                    WriteValue(xw, p);
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                }

                xw.WriteEndElement();
                xw.WriteEndElement();
                xw.Flush();
            }
        }

        public void SerializeResponse(XmlRpcResponse response, Stream output)
        {
            using (var xw = XmlWriter.Create(output, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8, CloseOutput = false }))
            {
                xw.WriteStartDocument();
                xw.WriteStartElement("methodResponse");

                if (response.IsFault)
                {
                    xw.WriteStartElement("fault");
                    xw.WriteStartElement("value");
                    var faultStruct = response.Value as IDictionary<string, object>;
                    if (faultStruct != null)
                    {
                        WriteValue(xw, faultStruct);
                    }
                    else
                    {
                        WriteValue(xw, response.Value);
                    }
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                }
                else
                {
                    xw.WriteStartElement("params");
                    xw.WriteStartElement("param");
                    xw.WriteStartElement("value");
                    WriteValue(xw, response.Value);
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                }

                xw.WriteEndElement();
                xw.Flush();
            }
        }

        public XmlRpcRequest DeserializeRequest(Stream input)
        {
            using (var xreader = XmlReader.Create(input, XmlRpcSettings.CreateReaderSettings()))
            {
                return ParseRequest(xreader);
            }
        }

        public XmlRpcResponse DeserializeResponse(Stream input)
        {
            using (var xreader = XmlReader.Create(input, XmlRpcSettings.CreateReaderSettings()))
            {
                return ParseResponse(xreader);
            }
        }

        // Async implementations: use MemoryStream + CopyToAsync to remain compatible with netstandard2.0
        public async Task SerializeRequestAsync(XmlRpcRequest request, Stream output, CancellationToken cancellationToken = default)
        {
            using (var ms = new MemoryStream())
            {
                SerializeRequest(request, ms);
                ms.Position = 0;
                await ms.CopyToAsync(output, 81920, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task SerializeResponseAsync(XmlRpcResponse response, Stream output, CancellationToken cancellationToken = default)
        {
            using (var ms = new MemoryStream())
            {
                SerializeResponse(response, ms);
                ms.Position = 0;
                await ms.CopyToAsync(output, 81920, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<XmlRpcRequest> DeserializeRequestAsync(Stream input, CancellationToken cancellationToken = default)
        {
            using (var ms = new MemoryStream())
            {
                await input.CopyToAsync(ms, 81920, cancellationToken).ConfigureAwait(false);
                ms.Position = 0;
                return DeserializeRequest(ms);
            }
        }

        public async Task<XmlRpcResponse> DeserializeResponseAsync(Stream input, CancellationToken cancellationToken = default)
        {
            using (var ms = new MemoryStream())
            {
                await input.CopyToAsync(ms, 81920, cancellationToken).ConfigureAwait(false);
                ms.Position = 0;
                return DeserializeResponse(ms);
            }
        }

        // Parsing helpers
        private XmlRpcRequest ParseRequest(XmlReader xreader)
        {
            string methodName = string.Empty;
            var parameters = new List<object>();

            while (xreader.Read())
            {
                if (xreader.NodeType == XmlNodeType.Element)
                {
                    if (xreader.Name == "methodName")
                    {
                        methodName = xreader.ReadElementContentAsString();
                    }
                    else if (xreader.Name == "value")
                    {
                        parameters.Add(ReadValue(xreader));
                    }
                }
            }

            return new XmlRpcRequest(methodName, parameters);
        }

        private XmlRpcResponse ParseResponse(XmlReader xreader)
        {
            var resp = new XmlRpcResponse();
            bool inFault = false;

            while (xreader.Read())
            {
                if (xreader.NodeType == XmlNodeType.Element)
                {
                    if (xreader.Name == "fault")
                    {
                        inFault = true;
                    }
                    else if (xreader.Name == "value")
                    {
                        var val = ReadValue(xreader);
                        resp.Value = val;
                        if (inFault)
                        {
                            resp.IsFault = true;
                            inFault = false;
                        }
                    }
                }
            }

            return resp;
        }

        private void WriteValue(XmlWriter xw, object value)
        {
            if (value == null) { xw.WriteElementString("string", string.Empty); return; }
            switch (value)
            {
                case string s: xw.WriteElementString("string", s); break;
                case int i: xw.WriteElementString("i4", i.ToString()); break;
                case bool b: xw.WriteElementString("boolean", b ? "1" : "0"); break;
                case double d: xw.WriteElementString("double", d.ToString(System.Globalization.CultureInfo.InvariantCulture)); break;
                case DateTime dt: xw.WriteElementString("dateTime.iso8601", dt.ToString("yyyyMMdd'T'HH:mm:ss")); break;
                case byte[] ba:
                    xw.WriteStartElement("base64");
                    xw.WriteBase64(ba, 0, ba.Length);
                    xw.WriteEndElement();
                    break;
                case IList<object> list:
                    xw.WriteStartElement("array");
                    xw.WriteStartElement("data");
                    foreach (var item in list) { xw.WriteStartElement("value"); WriteValue(xw, item); xw.WriteEndElement(); }
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    break;
                case IDictionary<string, object> dict:
                    xw.WriteStartElement("struct");
                    foreach (var kv in dict)
                    {
                        xw.WriteStartElement("member");
                        xw.WriteElementString("name", kv.Key);
                        xw.WriteStartElement("value");
                        WriteValue(xw, kv.Value);
                        xw.WriteEndElement();
                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                    break;
                default:
                    xw.WriteElementString("string", value.ToString());
                    break;
            }
        }

        private object ReadValue(XmlReader reader)
        {
            if (reader.IsEmptyElement) return string.Empty;
            if (reader.Name != "value")
            {
                // Ensure we are at a value element
                while (reader.Read() && !(reader.NodeType == XmlNodeType.Element && reader.Name == "value")) ;
                if (reader.EOF) return null;
            }

            // Now reader is on <value>
            var depth = reader.Depth;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "string": return reader.ReadElementContentAsString();
                        case "i4":
                        case "int": return reader.ReadElementContentAsInt();
                        case "boolean": return reader.ReadElementContentAsInt() == 1;
                        case "double": return reader.ReadElementContentAsDouble();
                        case "dateTime.iso8601": return DateTime.ParseExact(reader.ReadElementContentAsString(), "yyyyMMdd'T'HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        case "base64": return Convert.FromBase64String(reader.ReadElementContentAsString());
                        case "array":
                            var list = new List<object>();
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "value")
                                {
                                    list.Add(ReadValue(reader));
                                }
                                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "array")
                                    break;
                            }
                            return list;
                        case "struct":
                            var dict = new Dictionary<string, object>();
                            string currentName = null;
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "name")
                                {
                                    currentName = reader.ReadElementContentAsString();
                                }
                                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "value")
                                {
                                    dict[currentName] = ReadValue(reader);
                                }
                                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "struct")
                                    break;
                            }
                            return dict;
                    }
                }
                if (reader.NodeType == XmlNodeType.EndElement && reader.Depth <= depth) break;
            }

            return null;
        }
    }
}
