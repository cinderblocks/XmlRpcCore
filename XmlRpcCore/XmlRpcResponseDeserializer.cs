using System;
using System.IO;
using System.Xml;

namespace XmlRpcCore
{
    /// <summary>Class to deserialize XML data representing a response.</summary>
    public class XmlRpcResponseDeserializer : XmlRpcDeserializer
    {
        private static XmlRpcResponseDeserializer _singleton;

        /// <summary>A static singleton instance of this deserializer.</summary>
        [Obsolete("This object is now thread safe, just use an instance.", false)]
        public static XmlRpcResponseDeserializer Singleton =>
            _singleton ?? (_singleton = new XmlRpcResponseDeserializer());

        /// <summary>Static method that parses XML data into a response using the Singleton.</summary>
        /// <param name="xmlData"><c>StreamReader</c> containing an XML-RPC response.</param>
        /// <returns><c>XmlRpcResponse</c> object resulting from the parse.</returns>
        public override object Deserialize(TextReader xmlData)
        {
            var reader = XmlReader.Create(xmlData);
            var response = new XmlRpcResponse();

            lock (this)
            {
                Reset();

                while (reader.Read())
                {
                    DeserializeNode(reader); // Parent parse...
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.EndElement:
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

                            break;
                    }
                }
            }

            return response;
        }
    }
}