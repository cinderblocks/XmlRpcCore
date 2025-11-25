using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace XmlRpcCore
{
    internal static class XmlRpcParsing
    {
        public static XmlRpcRequest ParseRequest(TextReader reader)
        {
            var xreader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });

            string methodName = string.Empty;
            var parameters = new List<object>();

            while (xreader.Read())
            {
                if (xreader.NodeType == XmlNodeType.Element)
                {
                    if (xreader.Name == XmlRpcXmlTokens.METHOD_NAME)
                    {
                        methodName = xreader.ReadElementContentAsString();
                    }
                    else if (xreader.Name == XmlRpcXmlTokens.VALUE)
                    {
                        parameters.Add(ReadValue(xreader));
                    }
                }
            }

            return new XmlRpcRequest(methodName, parameters);
        }

        public static XmlRpcResponse ParseResponse(TextReader reader)
        {
            var xreader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
            var resp = new XmlRpcResponse();
            bool inFault = false;

            while (xreader.Read())
            {
                if (xreader.NodeType == XmlNodeType.Element)
                {
                    if (xreader.Name == XmlRpcXmlTokens.FAULT)
                    {
                        inFault = true;
                    }
                    else if (xreader.Name == XmlRpcXmlTokens.VALUE)
                    {
                        var val = ReadValue(xreader);
                        resp.Value = val;
                        if (inFault)
                        {
                            resp.IsFault = true;
                            inFault = false; // reset for subsequent values
                        }
                    }
                }
            }

            return resp;
        }

        private static object ReadValue(XmlReader reader)
        {
            if (reader.IsEmptyElement) return string.Empty;
            if (reader.Name != XmlRpcXmlTokens.VALUE)
            {
                while (reader.Read() && !(reader.NodeType == XmlNodeType.Element && reader.Name == XmlRpcXmlTokens.VALUE)) ;
                if (reader.EOF) return null;
            }

            var depth = reader.Depth;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case XmlRpcXmlTokens.STRING:
                            return reader.ReadElementContentAsString();
                        case XmlRpcXmlTokens.INT:
                        case XmlRpcXmlTokens.ALT_INT:
                            return reader.ReadElementContentAsInt();
                        case XmlRpcXmlTokens.BOOLEAN:
                            return reader.ReadElementContentAsInt() == 1;
                        case XmlRpcXmlTokens.DOUBLE:
                            return reader.ReadElementContentAsDouble();
                        case XmlRpcXmlTokens.DATETIME:
                            return DateTime.ParseExact(reader.ReadElementContentAsString(), XmlRpcXmlTokens.ISO_DATETIME, System.Globalization.CultureInfo.InvariantCulture);
                        case XmlRpcXmlTokens.BASE64:
                            return Convert.FromBase64String(reader.ReadElementContentAsString());
                        case XmlRpcXmlTokens.ARRAY:
                            var list = new List<object>();
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == XmlRpcXmlTokens.VALUE)
                                {
                                    list.Add(ReadValue(reader));
                                }
                                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == XmlRpcXmlTokens.ARRAY)
                                    break;
                            }
                            return list;
                        case XmlRpcXmlTokens.STRUCT:
                            var dict = new Dictionary<string, object>();
                            string currentName = null;
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == XmlRpcXmlTokens.NAME)
                                {
                                    currentName = reader.ReadElementContentAsString();
                                }
                                else if (reader.NodeType == XmlNodeType.Element && reader.Name == XmlRpcXmlTokens.VALUE)
                                {
                                    dict[currentName] = ReadValue(reader);
                                }
                                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == XmlRpcXmlTokens.STRUCT)
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
