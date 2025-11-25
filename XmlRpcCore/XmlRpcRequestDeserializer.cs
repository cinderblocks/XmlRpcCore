using System;
using System.IO;
using System.Xml;

namespace XmlRpcCore
{
    /// <summary>Class to deserialize XML data representing a request.</summary>
    public class XmlRpcRequestDeserializer : XmlRpcDeserializer
    {
        /// <summary>Parses XML data into a request.</summary>
        public override object Deserialize(TextReader xmlData)
        {
            try
            {
                using (var reader = XmlReader.Create(xmlData, XmlRpcSettings.CreateReaderSettings()))
                {
                    var request = new XmlRpcRequest();
                    var done = false;

                    lock (_syncRoot)
                    {
                        Reset();
                        while (!done && reader.Read())
                        {
                            if (reader.Depth > XmlRpcSettingsManager.Options.MaxDepth)
                                throw new XmlRpcProtocolException($"XML depth exceeded maximum of {XmlRpcSettingsManager.Options.MaxDepth}");

                            DeserializeNode(reader);

                            if (reader.NodeType == XmlNodeType.EndElement)
                            {
                                switch (reader.Name)
                                {
                                    case METHOD_NAME:
                                        request.MethodName = _text;
                                        break;
                                    case METHOD_CALL:
                                        done = true;
                                        break;
                                    case PARAM:
                                        request.Params.Add(_value);
                                        _text = null;
                                        break;
                                }
                            }
                        }
                    }

                    return request;
                }
            }
            catch (XmlException xex)
            {
                throw new XmlRpcProtocolException("Failed to parse XML-RPC request", null, xex);
            }
            catch (Exception ex)
            {
                throw new XmlRpcTransportException("Failed to deserialize XML-RPC request", ex);
            }
        }
    }
}
