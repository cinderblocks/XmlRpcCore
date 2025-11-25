// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

using System;

namespace XmlRpcCore
{
    public class XmlRpcTransportException : XmlRpcException
    {
        public XmlRpcTransportException(string message, Exception innerException = null) : base(-32000 /* transport error */, message)
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
    }
}
