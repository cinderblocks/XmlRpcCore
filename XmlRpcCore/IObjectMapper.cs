// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

namespace XmlRpcCore
{
    public interface IObjectMapper
    {
        T MapTo<T>(object xmlRpcValue);
        object MapFrom(object obj);
    }
}
