using System;

namespace XmlRpcCore
{
    /// <summary>An XML-RPC Exception.</summary>
    /// <remarks>
    ///     Maps a C# exception to an XML-RPC fault. Normal exceptions
    ///     include a message so this adds the code needed by XML-RPC.
    /// </remarks>
    public class XmlRpcException : Exception
    {
        /// <summary>Instantiate an <c>XmlRpcException</c> with a code and message.</summary>
        /// <param name="code"><c>Int</c> faultCode associated with this exception.</param>
        /// <param name="message"><c>String</c> faultMessage associated with this exception.</param>
        public XmlRpcException(int code, string message) : base(message)
        {
            FaultCode = code;
        }

        /// <summary>The value of the faults message, i.e. the faultString.</summary>
        public string FaultString => Message;

        /// <summary>The value of the faults code, i.e. the faultCode.</summary>
        public int FaultCode { get; }

        /// <summary>Format the message to include the code.</summary>
        public override string ToString()
        {
            return "Code: " + FaultCode + " Message: " + base.ToString();
        }
    }
}