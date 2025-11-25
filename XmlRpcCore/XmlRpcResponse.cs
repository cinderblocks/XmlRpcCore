// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

using System.Collections.Generic;

namespace XmlRpcCore
{
    public class XmlRpcResponse
    {
        public object Value { get; set; }
        public bool IsFault { get; set; }
        public int FaultCode { get; set; }
        public string FaultString { get; set; }
    }
}
