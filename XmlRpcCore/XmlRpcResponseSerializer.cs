using System.Xml;

namespace XmlRpcCore
{
    /// <summary>Class responsible for serializing an XML-RPC response.</summary>
    /// <remarks>
    ///     This class handles the response envelope, depending on XmlRpcSerializer
    ///     to serialize the payload.
    /// </remarks>
    /// <seealso cref="XmlRpcSerializer" />
    public class XmlRpcResponseSerializer : XmlRpcSerializer
    {
        private static XmlRpcResponseSerializer _singleton;

        /// <summary>A static singleton instance of this deserializer.</summary>
        public static XmlRpcResponseSerializer Singleton => _singleton ?? (_singleton = new XmlRpcResponseSerializer());

        /// <summary>Serialize the <c>XmlRpcResponse</c> to the output stream.</summary>
        /// <param name="output">An <c>XmlWriter</c> stream to write data to.</param>
        /// <param name="obj">An <c>Object</c> to serialize.</param>
        /// <seealso cref="XmlRpcResponse" />
        public override void Serialize(XmlWriter output, object obj)
        {
            var response = (XmlRpcResponse) obj;

            output.WriteStartDocument();
            output.WriteStartElement(METHOD_RESPONSE);

            if (response.IsFault)
            {
                output.WriteStartElement(FAULT);
            }
            else
            {
                output.WriteStartElement(PARAMS);
                output.WriteStartElement(PARAM);
            }

            output.WriteStartElement(VALUE);

            SerializeObject(output, response.Value);

            output.WriteEndElement();

            output.WriteEndElement();
            if (!response.IsFault)
                output.WriteEndElement();
            output.WriteEndElement();
        }
    }
}