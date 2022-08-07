using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Hood.Extensions
{
    public static class HoodLogErrorTypes
    {
        public static EventId SystemMessage =  new EventId(0, "System Message");
        public static EventId GenericError =  new EventId(1000, "Generic Error");
        public static EventId AccountError =  new EventId(2000, "Account Error");
        public static EventId Auth0Error =  new EventId(3000, "Auth0 Error");
    }

    public static class LoggerExtensions
    {
        public static void LogException(this ILogger logger, EventId type, Exception ex, string message = null, LogLevel level = LogLevel.Error, params object[] args)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    logger.LogTrace(type, ex, message, args);
                    break;
                case LogLevel.Debug:
                    logger.LogDebug(type, ex, message, args);
                    break;
                case LogLevel.Information:
                    logger.LogInformation(type, ex, message, args);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(type, ex, message, args);
                    break;
                case LogLevel.Error:
                    logger.LogError(type, ex, message, args);
                    break;
                case LogLevel.Critical:
                    logger.LogCritical(type, ex, message, args);
                    break;
            }
        }
        public static void LogMessage(this ILogger logger, EventId type, string message, LogLevel level = LogLevel.Error, params object[] args)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    logger.LogTrace(type, message, args);
                    break;
                case LogLevel.Debug:
                    logger.LogDebug(type, message, args);
                    break;
                case LogLevel.Information:
                    logger.LogInformation(type, message, args);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(type, message, args);
                    break;
                case LogLevel.Error:
                    logger.LogError(type, message, args);
                    break;
                case LogLevel.Critical:
                    logger.LogCritical(type, message, args);
                    break;
            }
        }
    }
}