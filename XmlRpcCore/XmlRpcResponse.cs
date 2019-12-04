using System.Collections;

namespace XmlRpcCore
{
    /// <summary>Class designed to represent an XML-RPC response.</summary>
    public class XmlRpcResponse
    {
        private object _value;

        /// <summary><c>bool</c> indicating if this response represents a fault.</summary>
        public bool IsFault;

        /// <summary>Basic constructor</summary>
        public XmlRpcResponse()
        {
            Value = null;
            IsFault = false;
        }

        /// <summary>Constructor for a fault.</summary>
        /// <param name="code"><c>int</c> the numeric faultCode value.</param>
        /// <param name="message"><c>String</c> the faultString value.</param>
        public XmlRpcResponse(int code, string message) : this()
        {
            SetFault(code, message);
        }

        /// <summary>The data value of the response, may be fault data.</summary>
        public object Value
        {
            get => _value;
            set
            {
                IsFault = false;
                _value = value;
            }
        }

        /// <summary>The faultCode if this is a fault.</summary>
        public int FaultCode
        {
            get
            {
                if (!IsFault)
                    return 0;
                return (int) ((Hashtable) _value)[XmlRpcXmlTokens.FAULT_CODE];
            }
        }

        /// <summary>The faultString if this is a fault.</summary>
        public string FaultString
        {
            get
            {
                if (!IsFault)
                    return "";
                return (string) ((Hashtable) _value)[XmlRpcXmlTokens.FAULT_STRING];
            }
        }

        /// <summary>Set this response to be a fault.</summary>
        /// <param name="code"><c>int</c> the numeric faultCode value.</param>
        /// <param name="message"><c>String</c> the faultString value.</param>
        public void SetFault(int code, string message)
        {
            var fault = new Hashtable();
            fault.Add("faultCode", code);
            fault.Add("faultString", message);
            Value = fault;
            IsFault = true;
        }

        /// <summary>Form a useful string representation of the object, in this case the XML response.</summary>
        /// <returns><c>String</c> The XML serialized XML-RPC response.</returns>
        public override string ToString()
        {
            return XmlRpcResponseSerializer.Singleton.Serialize(this);
        }
    }
}