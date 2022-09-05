using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class LogService : ILogService
    {
        public LogService()
        {}
        
        public Task AddLogAsync<TSource>(string message, object logObject = null, LogType type = LogType.Info)
        {
            var _logger = Engine.Services.Resolve<ILogger<TSource>>();
            switch (type)
            {
                case LogType.Error:
                    _logger.LogMessage(HoodLogErrorTypes.SystemMessage, message, LogLevel.Error);
                    break;
                case LogType.Warning:
                    _logger.LogMessage(HoodLogErrorTypes.SystemMessage, message, LogLevel.Warning);
                    break;
                default:
                    _logger.LogMessage(HoodLogErrorTypes.SystemMessage, message, LogLevel.Information);
                    break;
            }
            return Task.CompletedTask;
        }

        public Task AddExceptionAsync<TSource>(string message, object logObject, Exception ex, LogType type = LogType.Error)
        {
            var _logger = Engine.Services.Resolve<ILogger<TSource>>();
            switch (type)
            {
                case LogType.Error:
                    _logger.LogException(HoodLogErrorTypes.SystemMessage, ex,  message, LogLevel.Error);
                    break;
                case LogType.Warning:
                    _logger.LogException(HoodLogErrorTypes.SystemMessage, ex,  message, LogLevel.Warning);
                    break;
                default:
                    _logger.LogException(HoodLogErrorTypes.SystemMessage, ex,  message, LogLevel.Information);
                    break;
            }
            return Task.CompletedTask;
        }

        public Task AddExceptionAsync<TSource>(string message, Exception ex, LogType type = LogType.Error)
        {
            var _logger = Engine.Services.Resolve<ILogger<TSource>>();
            switch (type)
            {
                case LogType.Error:
                    _logger.LogException(HoodLogErrorTypes.SystemMessage, ex, message, LogLevel.Error);
                    break;
                case LogType.Warning:
                    _logger.LogException(HoodLogErrorTypes.SystemMessage, ex, message, LogLevel.Warning);
                    break;
                default:
                    _logger.LogException(HoodLogErrorTypes.SystemMessage, ex, message, LogLevel.Information);
                    break;
            }
            return Task.CompletedTask;
        }
    }

}
