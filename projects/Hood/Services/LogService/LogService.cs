using System.Collections.Generic;
using System.Linq;
using Hood.Interfaces;
using Geocoding;
using Geocoding.Google;
using Hood.Extensions;
using System.Threading.Tasks;
using Hood.Models;
using System;
using Newtonsoft.Json;

namespace Hood.Services
{
    public class LogService : ILogService
    {
        private readonly HoodDbContext _db;
        private readonly ISettingsRepository _settings;

        public LogService(ISettingsRepository site, HoodDbContext db)
        {
            _settings = site;
            _db = db;
        }

        public async Task AddLogAsync(string message, string detail, LogType type, LogSource source, string UserId, string entityId, string url)
        {
            var log = new Log()
            {
                Type = type,
                Source = source,
                Detail = detail,
                Time = DateTime.Now,
                Title = message,
                UserId = UserId,
                EntityId = entityId,
                SourceUrl = url
            };
            _db.Logs.Add(log);
            await _db.SaveChangesAsync();
        }

        public async Task AddLogAsync(string message, Exception ex, LogType type, LogSource source, string UserId, string entityId, string url)
        {
            var detail = string.Concat("Stack Trace:", Environment.NewLine, ex.StackTrace);
            if (ex.InnerException != null)
                detail = string.Concat(
                    "Inner Exception: ", ex.InnerException.Message, Environment.NewLine,
                    "Stack Trace:", Environment.NewLine, ex.StackTrace,
                    "Exception JSON:", Environment.NewLine, Environment.NewLine,
                    JsonConvert.SerializeObject(ex));
            await AddLogAsync(message, detail, LogType.Error, source, UserId, entityId, url);
        }

    }
}
