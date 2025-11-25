// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace XmlRpcCore
{
    internal static class JsonElementExtensions
    {
        public static object ToPlainObject(this JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (var prop in element.EnumerateObject())
                    {
                        dict[prop.Name] = prop.Value.ToPlainObject();
                    }
                    return dict;
                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        list.Add(item.ToPlainObject());
                    }
                    return list;
                case JsonValueKind.String:
                    if (element.TryGetDateTime(out var dt))
                        return dt;
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt64(out var l))
                    {
                        if (l >= int.MinValue && l <= int.MaxValue)
                            return (int)l;
                        return l;
                    }
                    if (element.TryGetDouble(out var d))
                        return d;
                    if (element.TryGetDecimal(out var dec))
                        return dec;
                    return element.GetRawText();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                default:
                    return element.GetRawText();
            }
        }
    }
}
