using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Globalization;

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
                    new XmlWriterSettings {Encoding = Encoding.UTF8, Indent = true}))
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
        public static void SerializeObject(XmlWriter output, object obj)
        {
            switch (obj)
            {
                case null:
                    return;
                case byte[] ba:
                    output.WriteStartElement(BASE64);
                    output.WriteBase64(ba, 0, ba.Length);
                    output.WriteEndElement();
                    break;
                case string _:
                    output.WriteElementString(STRING, obj.ToString());
                    break;
                case int _:
                    output.WriteElementString(INT, obj.ToString());
                    break;
                case DateTime time:
                    output.WriteElementString(DATETIME, time.ToString(ISO_DATETIME, CultureInfo.InvariantCulture));
                    break;
                case double _:
                    output.WriteElementString(DOUBLE, obj.ToString());
                    break;
                case bool b:
                    output.WriteElementString(BOOLEAN, b ? "1" : "0");
                    break;
                case IList<object> genericList:
                {
                    output.WriteStartElement(ARRAY);
                    output.WriteStartElement(DATA);
                    if (genericList.Count > 0)
                        foreach (var member in genericList)
                        {
                            output.WriteStartElement(VALUE);
                            SerializeObject(output, member);
                            output.WriteEndElement();
                        }

                    output.WriteEndElement();
                    output.WriteEndElement();
                    break;
                }
                case IList list:
                {
                    output.WriteStartElement(ARRAY);
                    output.WriteStartElement(DATA);
                    if (list.Count > 0)
                        foreach (var member in list)
                        {
                            output.WriteStartElement(VALUE);
                            SerializeObject(output, member);
                            output.WriteEndElement();
                        }

                    output.WriteEndElement();
                    output.WriteEndElement();
                    break;
                }
                case IDictionary<string, object> genericDict:
                {
                    output.WriteStartElement(STRUCT);
                    foreach (var kv in genericDict)
                    {
                        output.WriteStartElement(MEMBER);
                        output.WriteElementString(NAME, kv.Key);
                        output.WriteStartElement(VALUE);
                        SerializeObject(output, kv.Value);
                        output.WriteEndElement();
                        output.WriteEndElement();
                    }

                    output.WriteEndElement();
                    break;
                }
                case IDictionary dictionary:
                {
                    output.WriteStartElement(STRUCT);
                    foreach (string key in dictionary.Keys)
                    {
                        output.WriteStartElement(MEMBER);
                        output.WriteElementString(NAME, key);
                        output.WriteStartElement(VALUE);
                        SerializeObject(output, dictionary[key]);
                        output.WriteEndElement();
                        output.WriteEndElement();
                    }

                    output.WriteEndElement();
                    break;
                }
            }
        }
    }
}