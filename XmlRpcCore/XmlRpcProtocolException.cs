// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

using System;

namespace XmlRpcCore
{
    public class XmlRpcProtocolException : XmlRpcException
    {
        public XmlRpcProtocolException(string message, Exception innerException = null) : base(-32700 /* parse error */, message)
        {
            if (innerException != null)
            {
                try
                {
                    Data["InnerExceptionType"] = innerException.GetType().FullName;
                    Data["InnerExceptionMessage"] = innerException.Message;
                }
                catch
                {
                }
            }
        }

        public XmlRpcProtocolException(string message, string xmlSnippet, Exception innerException = null) : this(message, innerException)
        {
            if (!string.IsNullOrEmpty(xmlSnippet))
            {
                try
                {
                    Data["XmlSnippet"] = xmlSnippet;
                }
                catch { }
            }
        }
    }
}
