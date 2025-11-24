using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace XmlRpcCore
{
    /// <summary>Parser context, we maintain contexts in a stack to avoiding recursion. </summary>
    internal struct Context
    {
        public string Name;
        public object Container;
    }

    /// <summary>Basic XML-RPC data deserializer.</summary>
    /// <remarks>
    ///     Uses <c>XmlTextReader</c> to parse the XML data. This level of the class
    ///     only handles the tokens common to both Requests and Responses. This class is not useful in and of itself
    ///     but is designed to be subclassed.
    /// </remarks>
    public class XmlRpcDeserializer : XmlRpcXmlTokens
    {
        private static readonly DateTimeFormatInfo _dateFormat = new DateTimeFormatInfo();

        private object _container;
        private Stack _containerStack;

        /// <summary>Protected reference to last text.</summary>
        protected string _text;

        /// <summary>Protected reference to last deserialized value.</summary>
        protected object _value;

        /// <summary>Protected reference to last name field.</summary>
        protected string _name;


        /// <summary>Basic constructor.</summary>
        public XmlRpcDeserializer()
        {
            Reset();
            _dateFormat.FullDateTimePattern = ISO_DATETIME;
        }

        /// <summary>Static method that parses XML data into a response using the Singleton.</summary>
        /// <param name="xmlData"><c>StreamReader</c> containing an XML-RPC response.</param>
        /// <returns><c>Object</c> object resulting from the deserialization.</returns>
        public virtual object Deserialize(TextReader xmlData)
        {
            return null;
        }

        /// <summary>Protected method to parse a node in an XML-RPC XML stream.</summary>
        /// <remarks>
        ///     Method deals with elements common to all XML-RPC data, subclasses of
        ///     this object deal with request/response spefic elements.
        /// </remarks>
        /// <param name="reader"><c>XmlTextReader</c> of the in progress parsing data stream.</param>
        protected void DeserializeNode(XmlReader reader)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (Logger.Delegate != null)
                        Logger.WriteEntry($"START {reader.Name}", LogLevel.Information);
                    switch (reader.Name)
                    {
                        case VALUE:
                            _value = null;
                            _text = null;
                            break;
                        case STRUCT:
                            if (reader.IsEmptyElement) { break; }
                            PushContext();
                            // use generic dictionary for structs
                            _container = new Dictionary<string, object>();
                            break;
                        case ARRAY:
                            if (reader.IsEmptyElement) { break; }
                            PushContext();
                            // use generic list for arrays
                            _container = new List<object>();
                            break;
                    }

                    break;
                case XmlNodeType.EndElement:
                    if (Logger.Delegate != null)
                        Logger.WriteEntry($"END {reader.Name}", LogLevel.Information);
                    switch (reader.Name)
                    {
                        case BASE64:
                            _value = Convert.FromBase64String(_text);
                            break;
                        case BOOLEAN:
                            short bval;
                            _value = (short.TryParse(_text, out bval) && bval == 1);
                            break;
                        case STRING:
                            _value = _text;
                            break;
                        case DOUBLE:
                            double dval;
                            _value = double.TryParse(_text, out dval) ? dval : 0.0;
                            break;
                        case INT:
                        case ALT_INT:
                            int ival;
                            _value = int.TryParse(_text, out ival) ? ival : 0;
                            break;
                        case DATETIME:
#if __MONO__
				_value = DateParse(_text);
#else
                            // Use the ISO_DATETIME format and invariant culture so serialization and deserialization
                            // round-trip reliably across cultures.
                            _value = DateTime.ParseExact(_text, ISO_DATETIME, CultureInfo.InvariantCulture, DateTimeStyles.None);
#endif
                            break;
                        case NAME:
                            _name = _text;
                            break;
                        case VALUE:
                            if (_value == null)
                                _value = _text; // some kits don't use <string> tag, they just do <value>

                            // If inside an array, add the value to it.
                            if (_container is IList list)
                            {
                                list.Add(_value);
                            }
                            else if (_container is IList<object> genericList)
                            {
                                genericList.Add(_value);
                            }

                            break;
                        case MEMBER:
                            // If inside a struct, add the name/value pair.
                            if (_container is IDictionary dictionary)
                            {
                                dictionary.Add(_name, _value);
                            }
                            else if (_container is IDictionary<string, object> genericDict)
                            {
                                genericDict.Add(_name, _value);
                            }
                            break;
                        case ARRAY:
                        case STRUCT:
                            _value = _container;
                            PopContext();
                            break;
                    }

                    break;
                case XmlNodeType.Text:
                    if (Logger.Delegate != null)
                        Logger.WriteEntry($"Text {reader.Value}", LogLevel.Information);
                    _text = reader.Value;
                    break;
            }
        }

        /// <summary>
        ///     Static method that parses XML in a <c>String</c> into a
        ///     request using the Singleton.
        /// </summary>
        /// <param name="xmlData"><c>String</c> containing an XML-RPC request.</param>
        /// <returns><c>XmlRpcRequest</c> object resulting from the parse.</returns>
        public object Deserialize(string xmlData)
        {
            var sr = new StringReader(xmlData);
            return Deserialize(sr);
        }

        /// <summary>Pop a Context of the stack, an Array or Struct has closed.</summary>
        private void PopContext()
        {
            var c = (Context) _containerStack.Pop();
            _container = c.Container;
            _name = c.Name;
        }

        /// <summary>Push a Context on the stack, an Array or Struct has opened.</summary>
        private void PushContext()
        {
            Context context;

            context.Container = _container;
            context.Name = _name;

            _containerStack.Push(context);
        }

        /// <summary>Reset the internal state of the deserializer.</summary>
        protected void Reset()
        {
            _text = null;
            _value = null;
            _name = null;
            _container = null;
            _containerStack = new Stack();
        }

#if __MONO__
    private DateTime DateParse(String str)
      {
	int year = Int32.Parse(str.Substring(0,4));
	int month = Int32.Parse(str.Substring(4,2));
	int day = Int32.Parse(str.Substring(6,2));
	int hour = Int32.Parse(str.Substring(9,2));
	int min = Int32.Parse(str.Substring(12,2));
	int sec = Int32.Parse(str.Substring(15,2));
	return new DateTime(year,month,day,hour,min,sec);
      }
#endif
    }
}