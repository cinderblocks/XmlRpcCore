using System;

namespace XmlRpcCore
{
    /// <summary>Attribute to specify the XML-RPC name for a property, field or constructor parameter.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class XmlRpcNameAttribute : Attribute
    {
        public XmlRpcNameAttribute(string name) => Name = name;
        public string Name { get; }
    }
}
