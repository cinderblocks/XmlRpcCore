using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
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

        // Node counting to mitigate DoS from extremely large documents
        private long _nodeCount;

        /// <summary>Maximum characters allowed in the XML document. Default 10000000 (10MB).</summary>
        public static long MaxCharactersInDocument { get; set; } = 10000000;

        /// <summary>Maximum characters produced by entity expansion. Default 1000000.</summary>
        public static long MaxCharactersFromEntities { get; set; } = 1000000;

        /// <summary>Maximum number of XML nodes to process. Default 100000.</summary>
        public static int MaxNodeCount { get; set; } = 100000;

        /// <summary>Maximum element depth to prevent stack-like attacks. Default 2048.</summary>
        public static int MaxElementDepth { get; set; } = 2048;

        // Current depth counter
        private int _depth;

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

        /// <summary>Deserialize XML from a <see cref="TextReader"/> and bind to a POCO of type T if necessary.</summary>
        public T Deserialize<T>(TextReader xmlData)
        {
            var obj = Deserialize(xmlData);

            // If the concrete deserializer returned an XmlRpcResponse, map its Value
            if (obj is XmlRpcResponse resp)
            {
                return (T)MapToType(resp.Value, typeof(T));
            }

            // If the concrete deserializer returned an XmlRpcRequest, map first param if present
            if (obj is XmlRpcRequest req)
            {
                if (req.Params != null && req.Params.Count > 0)
                    return (T)MapToType(req.Params[0], typeof(T));

                return default(T);
            }

            return (T)MapToType(obj, typeof(T));
        }

        /// <summary>Deserialize XML from a <see cref="string"/> and bind to a POCO of type T if necessary.</summary>
        public T Deserialize<T>(string xmlData)
        {
            using (var sr = new StringReader(xmlData))
            {
                return Deserialize<T>(sr);
            }
        }

        /// <summary>Protected method to parse a node in an XML-RPC XML stream.</summary>
        /// <remarks>
        ///     Method deals with elements common to all XML-RPC data, subclasses of
        ///     this object deal with request/response spefic elements.
        /// </remarks>
        /// <param name="reader"><c>XmlTextReader</c> of the in progress parsing data stream.</param>
        protected void DeserializeNode(XmlReader reader)
        {
            // simple node count guard to avoid extremely large documents
            _nodeCount++;
            if (_nodeCount > MaxNodeCount)
                throw new XmlException($"XML document contains too many nodes (>{MaxNodeCount}). Potential DoS attack or malformed document.");

            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    // increment depth on element entry
                    _depth++;
                    if (_depth > MaxElementDepth)
                        throw new XmlException($"XML element depth exceeded maximum ({MaxElementDepth}). Potential DoS attack or malformed document.");

                    if (Logger.Delegate != null)
                        Logger.WriteEntry($"START {reader.Name}", LogLevel.Information);
                    switch (reader.Name)
                    {
                        case VALUE:
                            _value = null;
                            _text = null;
                            break;
                        case STRUCT:
                            if (reader.IsEmptyElement)
                            {
                                // empty struct, nothing to push; decrement depth and break
                                _container = new Dictionary<string, object>();
                                _depth--;
                                break;
                            }
                            PushContext();
                            // use generic dictionary for structs
                            _container = new Dictionary<string, object>();
                            break;
                        case ARRAY:
                            if (reader.IsEmptyElement)
                            {
                                // empty array
                                _container = new List<object>();
                                _depth--;
                                break;
                            }
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

                    // decrement depth on element exit, ensure not negative
                    if (_depth > 0)
                        _depth--;

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
            _nodeCount = 0;
            _depth = 0;
        }

#region POCO binder helpers
        private object MapToType(object value, Type targetType)
        {
            if (value == null)
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            // If already assignable
            if (targetType.IsInstanceOfType(value))
                return value;

            // Handle nullable
            var underlying = Nullable.GetUnderlyingType(targetType);
            if (underlying != null)
            {
                return MapToType(value, underlying);
            }

            // Primitives, enums, strings, DateTime
            if (targetType.IsPrimitive || targetType == typeof(string) || targetType == typeof(decimal) || targetType == typeof(DateTime) || targetType.IsEnum)
            {
                try
                {
                    if (targetType == typeof(DateTime) && value is string s)
                    {
                        return DateTime.ParseExact(s, ISO_DATETIME, CultureInfo.InvariantCulture);
                    }

                    return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                }
                catch
                {
                    throw new InvalidCastException($"Cannot convert value to {targetType}");
                }
            }

            // Arrays / generic lists
            if (targetType.IsArray && value is IList listVal)
            {
                var elemType = targetType.GetElementType();
                var array = Array.CreateInstance(elemType, listVal.Count);
                for (int i = 0; i < listVal.Count; i++)
                    array.SetValue(MapToType(listVal[i], elemType), i);
                return array;
            }

            if (IsGenericList(targetType) && value is IList listVal2)
            {
                var elemType = targetType.GetGenericArguments()[0];
                var genericList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elemType));
                foreach (var item in listVal2)
                    genericList.Add(MapToType(item, elemType));
                return genericList;
            }

            // Dictionaries -> POCO
            if (value is IDictionary<string, object> gd || value is IDictionary)
            {
                var map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                if (value is IDictionary<string, object> genericDict)
                {
                    foreach (var kv in genericDict)
                        map[kv.Key] = kv.Value;
                }
                else if (value is IDictionary nonGenericDict)
                {
                    foreach (DictionaryEntry de in nonGenericDict)
                    {
                        var key = de.Key?.ToString();
                        if (key != null) map[key] = de.Value;
                    }
                }

                // If targetType is IDictionary<string,object> just return mapped dictionary
                if (typeof(IDictionary<string, object>).IsAssignableFrom(targetType))
                {
                    return map;
                }

                // If targetType has a constructor that matches dictionary keys, prefer constructor binding
                var ctors = targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                foreach (var ctor in ctors)
                {
                    var parameters = ctor.GetParameters();
                    if (parameters.Length == 0) continue;

                    var args = new object[parameters.Length];
                    var match = true;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var p = parameters[i];
                        // check XmlRpcName attribute first
                        var attr = (XmlRpcNameAttribute)p.GetCustomAttribute(typeof(XmlRpcNameAttribute));
                        var name = attr?.Name ?? p.Name;

                        if (!map.TryGetValue(name, out var v))
                        {
                            match = false;
                            break;
                        }

                        try
                        {
                            args[i] = MapToType(v, p.ParameterType);
                        }
                        catch
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        return ctor.Invoke(args);
                    }
                }

                // Create POCO and set properties
                var poco = Activator.CreateInstance(targetType);
                var props = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (var prop in props)
                {
                    // prefer writable properties or those with a non-public setter
                    var setMethod = prop.GetSetMethod(true);
                    if (setMethod == null && !prop.CanWrite)
                        continue;

                    // determine name via XmlRpcNameAttribute if present, otherwise use property name
                    var propAttr = (XmlRpcNameAttribute)prop.GetCustomAttribute(typeof(XmlRpcNameAttribute));
                    var propName = propAttr?.Name ?? prop.Name;

                    if (map.TryGetValue(propName, out var propVal))
                    {
                        var converted = MapToType(propVal, prop.PropertyType);
                        if (setMethod != null)
                        {
                            // invoke setter even if non-public
                            setMethod.Invoke(poco, new[] { converted });
                        }
                        else
                        {
                            // Try to set an auto-property backing field (compiler generated)
                            var backing = targetType.GetField("<" + prop.Name + ">k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (backing != null)
                            {
                                backing.SetValue(poco, converted);
                            }
                        }
                    }
                }

                // Also set public and non-public fields directly
                var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var fieldAttr = (XmlRpcNameAttribute)field.GetCustomAttribute(typeof(XmlRpcNameAttribute));
                    var fieldName = fieldAttr?.Name ?? field.Name;
                    if (map.TryGetValue(fieldName, out var fVal))
                    {
                        var converted = MapToType(fVal, field.FieldType);
                        field.SetValue(poco, converted);
                    }
                }

                return poco;
            }

            throw new InvalidCastException($"Unable to map value of type {value.GetType()} to target type {targetType}");
        }

        private static bool IsGenericList(Type t)
        {
            if (!t.IsGenericType) return false;
            var def = t.GetGenericTypeDefinition();
            return def == typeof(List<>) || def == typeof(IList<>) || def == typeof(IEnumerable<>);
        }
#endregion

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