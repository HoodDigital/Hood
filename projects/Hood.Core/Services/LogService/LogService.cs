using System;
using System.Threading.Tasks;
using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Microsoft.Extensions.Logging;

namespace Hood.Services
{
    public class LogService : ILogService
    {
        public Task AddLogAsync<TSource>(
            string message,
            object logObject = null,
            LogType type = LogType.Info
        )
        {
            var _logger = Engine.Services.Resolve<ILogger<TSource>>();
            switch (type)
            {
                case LogType.Error:
                    _logger.LogMessage(HoodLogErrorTypes.SystemMessage, message);
                    break;
                case LogType.Warning:
                    _logger.LogMessage(HoodLogErrorTypes.SystemMessage, message, LogLevel.Warning);
                    break;
                default:
                    _logger.LogMessage(
                        HoodLogErrorTypes.SystemMessage,
                        message,
                        LogLevel.Information
                    );
                    break;
            }
            return Task.CompletedTask;
        }

        public Task AddExceptionAsync<TSource>(
            string message,
            object logObject,
            Exception ex,
            LogType type = LogType.Error
        )
        {
            var _logger = Engine.Services.Resolve<ILogger<TSource>>();
            switch (type)
            {
                case LogType.Error:
                    _logger.LogException(HoodLogErrorTypes.SystemMessage, ex, message);
                    break;
                case LogType.Warning:
                    _logger.LogException(
                        HoodLogErrorTypes.SystemMessage,
                        ex,
                        message,
                        LogLevel.Warning
                    );
                    break;
                default:
                    _logger.LogException(
                        HoodLogErrorTypes.SystemMessage,
                        ex,
                        message,
                        LogLevel.Information
                    );
                    break;
            }
            return Task.CompletedTask;
        }

        public Task AddExceptionAsync<TSource>(
            string message,
            Exception ex,
            LogType type = LogType.Error
        )
        {
            var _logger = Engine.Services.Resolve<ILogger<TSource>>();
            switch (type)
            {
                case LogType.Error:
                    _logger.LogException(HoodLogErrorTypes.SystemMessage, ex, message);
                    break;
                case LogType.Warning:
                    _logger.LogException(
                        HoodLogErrorTypes.SystemMessage,
                        ex,
                        message,
                        LogLevel.Warning
                    );
                    break;
                default:
                    _logger.LogException(
                        HoodLogErrorTypes.SystemMessage,
                        ex,
                        message,
                        LogLevel.Information
                    );
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
