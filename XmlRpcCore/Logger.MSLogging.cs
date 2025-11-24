#if HAS_MS_LOGGING
// Define HAS_MS_LOGGING in your project if you reference Microsoft.Extensions.Logging
using Microsoft.Extensions.Logging;

namespace XmlRpcCore
{
    public static partial class Logger
    {
        /// <summary>
        /// Use a Microsoft.Extensions.Logging.ILogger directly (strongly typed overload).
        /// This method is compiled only when Microsoft.Extensions.Logging is referenced and
        /// the HAS_MS_LOGGING symbol is defined for the project.
        /// </summary>
        public static void UseMicrosoftLogger(ILogger logger)
        {
            if (logger == null)
            {
                Delegate = null;
                return;
            }

            // Wrap ILogger with Delegate
            Delegate = (message, level) =>
            {
                switch (level)
                {
                    case LogLevel.Information:
                        logger.LogInformation(message);
                        break;
                    case LogLevel.Warning:
                        logger.LogWarning(message);
                        break;
                    case LogLevel.Error:
                        logger.LogError(message);
                        break;
                }
            };
        }
    }
}
#endif
