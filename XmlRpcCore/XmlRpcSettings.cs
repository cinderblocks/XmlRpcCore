// Copyright (c) 2003 Nicholas Christopher; 2016-2025 Sjofn LLC.
// Licensed under the BSD-3-Clause License. See LICENSE in the repository root for details.

using System;
using System.Reflection;
using System.Xml;

namespace XmlRpcCore
{
    public class XmlRpcOptions
    {
        public int MaxDepth { get; set; } = 128;
        public long MaxCharactersInDocument { get; set; } = 10 * 1024 * 1024; // 10 MB
        public long MaxCharactersFromEntities { get; set; } = 1024 * 1024; // 1 MB
        public bool AllowDtd { get; set; } = false;
        public bool AllowXmlResolver { get; set; } = false;
    }

    public static class XmlRpcSettingsManager
    {
        private static XmlRpcOptions _options = new XmlRpcOptions();

        public static XmlRpcOptions Options
        {
            get => _options;
            set => _options = value ?? new XmlRpcOptions();
        }
    }

    internal static class XmlRpcSettings
    {
        // Default limits (used only when building defaults)
        public const int DefaultMaxDepth = 128;
        public const long DefaultMaxCharactersInDocument = 10 * 1024 * 1024; // 10 MB
        public const long DefaultMaxCharactersFromEntities = 1024 * 1024; // 1 MB

        public static XmlReaderSettings CreateReaderSettings()
        {
            var opts = XmlRpcSettingsManager.Options ?? new XmlRpcOptions();

            var settings = new XmlReaderSettings
            {
                DtdProcessing = opts.AllowDtd ? DtdProcessing.Parse : DtdProcessing.Prohibit,
                XmlResolver = opts.AllowXmlResolver ? new XmlUrlResolver() : null
            };

            TrySetLongProperty(settings, "MaxCharactersInDocument", opts.MaxCharactersInDocument);
            TrySetLongProperty(settings, "MaxCharactersFromEntities", opts.MaxCharactersFromEntities);

            return settings;
        }

        private static void TrySetLongProperty(object target, string propName, long value)
        {
            try
            {
                var pi = target.GetType().GetRuntimeProperty(propName);
                if (pi != null && pi.CanWrite && pi.PropertyType == typeof(long))
                {
                    pi.SetValue(target, value);
                }
            }
            catch
            {
                // ignore if not supported
            }
        }
    }
}
