// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

using System;

namespace XmlRpcCore
{
    public interface ITypeConverter
    {
        bool TryConvert(object value, Type targetType, out object result);
    }

    public class DefaultTypeConverter : ITypeConverter
    {
        public bool TryConvert(object value, Type targetType, out object result)
        {
            if (value == null)
            {
                result = null;
                return !targetType.IsValueType;
            }

            try
            {
                if (targetType.IsInstanceOfType(value))
                {
                    result = value;
                    return true;
                }

                if (targetType.IsEnum)
                {
                    if (value is string s)
                    {
                        result = Enum.Parse(targetType, s);
                        return true;
                    }
                    result = Enum.ToObject(targetType, value);
                    return true;
                }

                result = Convert.ChangeType(value, targetType);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
