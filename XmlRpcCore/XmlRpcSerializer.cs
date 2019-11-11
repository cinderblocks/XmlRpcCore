using System;
using System.Collections;
using System.IO;
using System.Xml;

namespace XmlRpcCore
{
    /// <summary>Base class of classes serializing data to XML-RPC's XML format.</summary>
    /// <remarks>This class handles the basic type conversions like Integer to &lt;i4&gt;. </remarks>
    /// <seealso cref="XmlRpcXmlTokens" />
    public class XmlRpcSerializer : XmlRpcXmlTokens
    {
        /// <summary>Serialize the <c>XmlRpcRequest</c> to the output stream.</summary>
        /// <param name="output">An <c>XmlTextWriter</c> stream to write data to.</param>
        /// <param name="obj">An <c>Object</c> to serialize.</param>
        /// <seealso cref="XmlRpcRequest" />
        public virtual void Serialize(XmlTextWriter output, object obj)
        {
        }

        /// <summary>Serialize the <c>XmlRpcRequest</c> to a String.</summary>
        /// <remarks>Note this may represent a real memory hog for a large request.</remarks>
        /// <param name="obj">An <c>Object</c> to serialize.</param>
        /// <returns><c>String</c> containing XML-RPC representation of the request.</returns>
        /// <seealso cref="XmlRpcRequest" />
        public string Serialize(object obj)
        {
            var strBuf = new StringWriter();
            var xml = new XmlTextWriter(strBuf);
            xml.Formatting = Formatting.Indented;
            xml.Indentation = 4;
            Serialize(xml, obj);
            xml.Flush();
            var returns = strBuf.ToString();
            xml.Close();
            return returns;
        }

        /// <remarks>Serialize the object to the output stream.</remarks>
        /// <param name="output">An <c>XmlTextWriter</c> stream to write data to.</param>
        /// <param name="obj">An <c>Object</c> to serialize.</param>
        public void SerializeObject(XmlTextWriter output, object obj)
        {
            if (obj == null)
                return;

            if (obj is byte[])
            {
                var ba = (byte[]) obj;
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
            else if (obj is DateTime)
            {
                output.WriteElementString(DATETIME, ((DateTime) obj).ToString(ISO_DATETIME));
            }
            else if (obj is double)
            {
                output.WriteElementString(DOUBLE, obj.ToString());
            }
            else if (obj is bool)
            {
                output.WriteElementString(BOOLEAN, (bool) obj ? "1" : "0");
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