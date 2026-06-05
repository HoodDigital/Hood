using System;
using System.Threading.Tasks;
using Hood.Models;

namespace Hood.Services
{
    public interface ILogService
    {
        // ReSharper disable once UnusedParameter.Global — logging payload contract; the default sink doesn't persist it yet.
        Task AddLogAsync<TSource>(
            string message,
            object logObject = null,
            LogType type = LogType.Info
        );
        Task AddExceptionAsync<TSource>(string message, Exception ex, LogType type = LogType.Error);

        // ReSharper disable once UnusedParameter.Global — logging payload contract; the default sink doesn't persist it yet.
        Task AddExceptionAsync<TSource>(
            string message,
            object logObject,
            Exception ex,
            LogType type = LogType.Error
        );
    }
}
