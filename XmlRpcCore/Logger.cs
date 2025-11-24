using System;
using System.Linq;
using System.Reflection;

namespace XmlRpcCore
{
    /// <summary>Define levels of logging.</summary>
    /// <remarks>
    ///     This duplicates
    ///     similar enumerations in System.Diagnostics.EventLogEntryType. The
    ///     duplication was merited because .NET Compact Framework lacked the EventLogEntryType enum.
    /// </remarks>
    public enum LogLevel
    {
        /// <summary>Information level, log entry for informational reasons only.</summary>
        Information,

        /// <summary>Warning level, indicates a possible problem.</summary>
        Warning,

        /// <summary>Error level, implies a significant problem.</summary>
        Error
    }

    /// <summary>
    ///     Logging singleton with swappable output delegate.
    /// </summary>
    /// <remarks>
    ///     This singleton provides a centralized log. The actual WriteEntry calls are passed
    ///     off to a delegate however. Having a delegate do the actual logging allows you to
    ///     implement different logging mechanism and have them take effect throughout the system.
    /// </remarks>
    public static partial class Logger
    {
        /// <summary>Delegate definition for logging.</summary>
        /// <param name="message">The message <c>String</c> to log.</param>
        /// <param name="level">The <c>LogLevel</c> of your message.</param>
        public delegate void LoggerDelegate(string message, LogLevel level);

        ///<summary>The LoggerDelegate that will receive WriteEntry requests.</summary>
        public static LoggerDelegate Delegate;

        /// <summary>
        ///     Method logging events are sent to.
        /// </summary>
        /// <param name="message">The message <c>String</c> to log.</param>
        /// <param name="level">The <c>LogLevel</c> of your message.</param>
        public static void WriteEntry(string message, LogLevel level)
        {
            Delegate?.Invoke(message, level);
        }

        /// <summary>
        ///     Replace the logging delegate with an Action&lt;string,LogLevel&gt;.
        /// </summary>
        public static void Use(Action<string, LogLevel> action)
        {
            Delegate = action == null ? (LoggerDelegate) null : new LoggerDelegate(action);
        }

        /// <summary>
        ///     Attempt to use a Microsoft.Extensions.Logging.ILogger instance for logging.
        ///     This uses reflection to avoid a hard dependency on the logging package.
        /// </summary>
        /// <param name="msLogger">An instance implementing Microsoft.Extensions.Logging.ILogger (may be null to clear).</param>
        public static void UseMicrosoftLogger(object msLogger)
        {
            if (msLogger == null)
            {
                Delegate = null;
                return;
            }

            // Try to find LoggerExtensions type that contains LogInformation/LogWarning/LogError static extension methods
            var extType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); } catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.FullName == "Microsoft.Extensions.Logging.LoggerExtensions");

            MethodInfo infoMethod = null, warnMethod = null, errorMethod = null;
            if (extType != null)
            {
                // Prefer overloads that accept (ILogger, string, object[])
                infoMethod = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == "LogInformation" && m.GetParameters().Length >= 2);
                warnMethod = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == "LogWarning" && m.GetParameters().Length >= 2);
                errorMethod = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == "LogError" && m.GetParameters().Length >= 2);
            }

            // Set Delegate to call the appropriate method if available, otherwise no-op
            Delegate = (message, level) =>
            {
                try
                {
                    if (extType != null)
                    {
                        MethodInfo mi = level == LogLevel.Information ? infoMethod : level == LogLevel.Warning ? warnMethod : errorMethod;
                        if (mi != null)
                        {
                            // invoke static extension: LoggerExtensions.LogX(msLogger, message, new object[0])
                            var parameters = mi.GetParameters();
                            if (parameters.Length == 2)
                            {
                                // signature (ILogger, string)
                                mi.Invoke(null, new[] { msLogger, message });
                            }
                            else
                            {
                                // signature (ILogger, string, object[])
                                mi.Invoke(null, new object[] { msLogger, message, new object[0] });
                            }

                            return;
                        }
                    }

                    // fallback: try to find instance LogInformation/LogWarning/LogError on msLogger
                    var instType = msLogger.GetType();
                    var instName = level == LogLevel.Information ? "LogInformation" : level == LogLevel.Warning ? "LogWarning" : "LogError";
                    var instMi = instType.GetMethod(instName, new Type[] { typeof(string) });
                    if (instMi != null)
                    {
                        instMi.Invoke(msLogger, new object[] { message });
                        return;
                    }

                    // No known method found, ignore
                }
                catch
                {
                    // Swallow any exceptions coming from external logger invocation to avoid affecting library flow
                }
            };
        }
    }
}