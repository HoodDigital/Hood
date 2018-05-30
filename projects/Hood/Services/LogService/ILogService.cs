using Geocoding;
using Hood.Interfaces;
using Hood.Models;
using System;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface ILogService
    {
        Task AddLogAsync(string message, string detail, LogType type, LogSource source, string UserId, string entityId, string url);
        Task AddLogAsync(string message, Exception ex, LogType type, LogSource source, string UserId, string entityId, string url);
    }
}
