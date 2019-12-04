using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace XmlRpcCore
{
    /// <summary>Base class of classes serializing data to XML-RPC's XML format.</summary>
    /// <remarks>This class handles the basic type conversions like Integer to &lt;i4&gt;. </remarks>
    /// <seealso cref="XmlRpcXmlTokens" />
    public abstract class XmlRpcSerializer : XmlRpcXmlTokens
    {
        /// <summary>Serialize the <c>XmlRpcRequest</c> to the output stream.</summary>
        /// <param name="output">An <c>XmlWriter</c> stream to write data to.</param>
        /// <param name="obj">An <c>Object</c> to serialize.</param>
        /// <seealso cref="XmlRpcRequest" />
        public abstract void Serialize(XmlWriter output, object obj);

        /// <summary>Serialize the <c>XmlRpcRequest</c> to a String.</summary>
        /// <remarks>Note this may represent a real memory hog for a large request.</remarks>
        /// <param name="obj">An <c>Object</c> to serialize.</param>
        /// <returns><c>String</c> containing XML-RPC representation of the request.</returns>
        /// <seealso cref="XmlRpcRequest" />
        public string Serialize(object obj)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream,
                    new XmlWriterSettings {Encoding = Encoding.ASCII, Indent = true}))
                {
                    Serialize(writer, obj);
                }

                // reset position
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <remarks>Serialize the object to the output stream.</remarks>
        /// <param name="output">An <c>XmlWriter</c> stream to write data to.</param>
        /// <param name="obj">An <c>Object</c> to serialize.</param>
        public void SerializeObject(XmlWriter output, object obj)
        {
            if (obj == null)
                return;

            if (obj is byte[] ba)
            {
                output.WriteStartElement(BASE64);
                output.WriteBase64(ba, 0, ba.Length);
                output.WriteEndElement();
            }
            else if (obj is string)
            {
                output.WriteElementString(STRING, obj.ToString());
            }
            else if (obj is int)
            {
                output.WriteElementString(INT, obj.ToString());
            }
            else if (obj is DateTime time)
            {
                output.WriteElementString(DATETIME, time.ToString(ISO_DATETIME));
            }
            else if (obj is double)
            {
                output.WriteElementString(DOUBLE, obj.ToString());
            }
            else if (obj is bool b)
            {
                output.WriteElementString(BOOLEAN, b ? "1" : "0");
            }
            else if (obj is IList)
            {
                output.WriteStartElement(ARRAY);
                output.WriteStartElement(DATA);
                if (((ArrayList) obj).Count > 0)
                    foreach (var member in (IList) obj)
                    {
                        output.WriteStartElement(VALUE);
                        SerializeObject(output, member);
                        output.WriteEndElement();
                    }

                output.WriteEndElement();
                output.WriteEndElement();
            }
            else if (obj is IDictionary)
            {
                var h = (IDictionary) obj;
                output.WriteStartElement(STRUCT);
                foreach (string key in h.Keys)
                {
                    output.WriteStartElement(MEMBER);
                    output.WriteElementString(NAME, key);
                    output.WriteStartElement(VALUE);
                    SerializeObject(output, h[key]);
                    output.WriteEndElement();
                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }
        }
    }
}