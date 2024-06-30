namespace XmlRpcCore
{
    /// <summary>Class collecting <c>String</c> tokens that are part of XML-RPC files.</summary>
    public abstract class XmlRpcXmlTokens
    {
        /// <summary>C# formatting string to describe an ISO 8601 date.</summary>
        public const string ISO_DATETIME = "yyyyMMdd\\THH\\:mm\\:ss";

        /// <summary>Base64 field indicator.</summary>
        /// <remarks>Corresponds to the &lt;base64&gt; tag.</remarks>
        public const string BASE64 = "base64";

        /// <summary>String field indicator.</summary>
        /// <remarks>Corresponds to the &lt;string&gt; tag.</remarks>
        public const string STRING = "string";

        /// <summary>Integer field integer.</summary>
        /// <remarks>Corresponds to the &lt;i4&gt; tag.</remarks>
        public const string INT = "i4";

        /// <summary>Alternate integer field indicator.</summary>
        /// <remarks>Corresponds to the &lt;int&gt; tag.</remarks>
        public const string ALT_INT = "int";

        /// <summary>Date field indicator.</summary>
        /// <remarks>Corresponds to the &lt;dateTime.iso8601&gt; tag.</remarks>
        public const string DATETIME = "dateTime.iso8601";

        /// <summary>Boolean field indicator.</summary>
        /// <remarks>Corresponds to the &lt;boolean&gt; tag.</remarks>
        public const string BOOLEAN = "boolean";

        /// <summary>Value token.</summary>
        /// <remarks>Corresponds to the &lt;value&gt; tag.</remarks>
        public const string VALUE = "value";

        /// <summary>Name token.</summary>
        /// <remarks>Corresponds to the &lt;name&gt; tag.</remarks>
        public const string NAME = "name";

        /// <summary>Array field indicator..</summary>
        /// <remarks>Corresponds to the &lt;array&gt; tag.</remarks>
        public const string ARRAY = "array";

        /// <summary>Data token.</summary>
        /// <remarks>Corresponds to the &lt;data&gt; tag.</remarks>
        public const string DATA = "data";

        /// <summary>Member token.</summary>
        /// <remarks>Corresponds to the &lt;member&gt; tag.</remarks>
        public const string MEMBER = "member";

        /// <summary>Struct field indicator.</summary>
        /// <remarks>Corresponds to the &lt;struct&gt; tag.</remarks>
        public const string STRUCT = "struct";

        /// <summary>Double field indicator.</summary>
        /// <remarks>Corresponds to the &lt;double&gt; tag.</remarks>
        public const string DOUBLE = "double";

        /// <summary>Param token.</summary>
        /// <remarks>Corresponds to the &lt;param&gt; tag.</remarks>
        public const string PARAM = "param";

        /// <summary>Params token.</summary>
        /// <remarks>Corresponds to the &lt;params&gt; tag.</remarks>
        public const string PARAMS = "params";

        /// <summary>MethodCall token.</summary>
        /// <remarks>Corresponds to the &lt;methodCall&gt; tag.</remarks>
        public const string METHOD_CALL = "methodCall";

        /// <summary>MethodName token.</summary>
        /// <remarks>Corresponds to the &lt;methodName&gt; tag.</remarks>
        public const string METHOD_NAME = "methodName";

        /// <summary>MethodResponse token</summary>
        /// <remarks>Corresponds to the &lt;methodResponse&gt; tag.</remarks>
        public const string METHOD_RESPONSE = "methodResponse";

        /// <summary>Fault response token.</summary>
        /// <remarks>Corresponds to the &lt;fault&gt; tag.</remarks>
        public const string FAULT = "fault";

        /// <summary>FaultCode token.</summary>
        /// <remarks>Corresponds to the &lt;faultCode&gt; tag.</remarks>
        public const string FAULT_CODE = "faultCode";

        /// <summary>FaultString token.</summary>
        /// <remarks>Corresponds to the &lt;faultString&gt; tag.</remarks>
        public const string FAULT_STRING = "faultString";
    }
}