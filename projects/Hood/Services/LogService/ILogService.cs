using Geocoding;
using Hood.Interfaces;
using Hood.Models;
using System;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface ILogService
    {
        Task AddLogAsync<TSource>(string message, object logObject = null, LogType type = LogType.Info, string userId = null, string url = null);
        Task AddExceptionAsync<TSource>(string message, Exception ex, LogType type = LogType.Error, string userId = null, string url = null);
        Task AddExceptionAsync<TSource>(string message, object logObject, Exception ex, LogType type = LogType.Error, string userId = null, string url = null);
    }
}
