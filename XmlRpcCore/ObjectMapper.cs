// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XmlRpcCore
{
    public class ObjectMapper : IObjectMapper
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public T MapTo<T>(object xmlRpcValue)
        {
            if (xmlRpcValue == null) return default(T);
            if (xmlRpcValue is T t) return t;

            if (xmlRpcValue is IDictionary<string, object> dict)
            {
                var json = JsonSerializer.Serialize(dict, _options);
                return JsonSerializer.Deserialize<T>(json, _options);
            }

            return (T)Convert.ChangeType(xmlRpcValue, typeof(T));
        }

        public object MapFrom(object obj)
        {
            if (obj == null) return null;
            var json = JsonSerializer.Serialize(obj, _options);
            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (var prop in root.EnumerateObject())
                    {
                        dict[prop.Name] = prop.Value.ToPlainObject();
                    }
                    return dict;
                }

                return root.ToPlainObject();
            }
        }
    }
}
