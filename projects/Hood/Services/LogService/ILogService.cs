using Geocoding;
using Hood.Interfaces;
using Hood.Models;
using System;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface ILogService
    {
        Task AddLogAsync(string message, LogSource source, string detail = "", LogType type = LogType.Info, string UserId = null, string entityId = null, string entityType = null, string url = null);
        Task LogErrorAsync(string message, Exception ex, LogType type, LogSource source, string UserId = null, string entityId = null, string entityType = null, string url = null);
    }
}
