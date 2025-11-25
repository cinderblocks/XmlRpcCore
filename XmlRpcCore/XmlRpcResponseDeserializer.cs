using System;
using System.IO;
using System.Xml;

namespace XmlRpcCore
{
    /// <summary>Class to deserialize XML data representing a response.</summary>
    public class XmlRpcResponseDeserializer : XmlRpcDeserializer
    {
        /// <summary>Parses XML data into a response.</summary>
        public override object Deserialize(TextReader xmlData)
        {
            try
            {
                using (var reader = XmlReader.Create(xmlData, XmlRpcSettings.CreateReaderSettings()))
                {
                    var response = new XmlRpcResponse();

                    lock (_syncRoot)
                    {
                        Reset();

                        while (reader.Read())
                        {
                            if (reader.Depth > XmlRpcSettingsManager.Options.MaxDepth)
                                throw new XmlRpcProtocolException($"XML depth exceeded maximum of {XmlRpcSettingsManager.Options.MaxDepth}");

                            DeserializeNode(reader);

                            if (reader.NodeType == XmlNodeType.EndElement)
                            {
                                switch (reader.Name)
                                {
                                    case FAULT:
                                        response.Value = _value;
                                        response.IsFault = true;
                                        break;
                                    case PARAM:
                                        response.Value = _value;
                                        _value = null;
                                        _text = null;
                                        break;
                                }
                            }
                        }
                    }

                    return response;
                }
            }
            catch (XmlException xex)
            {
                throw new XmlRpcProtocolException("Failed to parse XML-RPC response", null, xex);
            }
            catch (Exception ex)
            {
                throw new XmlRpcTransportException("Failed to deserialize XML-RPC response", ex);
            }
        }
    }
}
