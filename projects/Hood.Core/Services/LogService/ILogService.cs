using System;
using System.Threading.Tasks;
using Hood.Models;

namespace Hood.Services
{
    public interface ILogService
    {
        Task AddLogAsync<TSource>(
            string message,
            object logObject = null,
            LogType type = LogType.Info
        );
        Task AddExceptionAsync<TSource>(string message, Exception ex, LogType type = LogType.Error);
        Task AddExceptionAsync<TSource>(
            string message,
            object logObject,
            Exception ex,
            LogType type = LogType.Error
        );
    }
}
