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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Hood.Services
{
    public class LogService : ILogService
    {
        protected readonly HoodDbContext _db;
        protected readonly IConfiguration _config;

        public LogService(IConfiguration config)
        {
            _config = config;

            var options = new DbContextOptionsBuilder<HoodDbContext>();
            options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
            _db = new HoodDbContext(options.Options);
        }

        private async Task AddLogAsync(string message, string detail = "", LogType type = LogType.Info, string userId = null, string url = null, string source = "")
        {
            var log = new Log()
            {
                Type = type,
                Source = source,
                Detail = detail,
                Time = DateTime.Now,
                Title = message,
                UserId = userId,
                SourceUrl = url
            };
            _db.Logs.Add(log);
            await _db.SaveChangesAsync();
        }

        public async Task AddLogAsync<TSource>(string message, object logObject = null, LogType type = LogType.Info, string userId = null, string url = null)
        {
            try
            {
                if (logObject != null)
                {
                    if (logObject is string)
                    {
                        await AddLogAsync(message, logObject as string, type, userId, url, logObject.GetType().ToString());
                    }
                    var detail = string.Concat(
                        $"{typeof(TSource).ToString()}:", Environment.NewLine,
                        JsonConvert.SerializeObject(logObject)
                    );
                    await AddLogAsync(message, detail, type, userId, url);
                }
                else
                    await AddLogAsync(message, "No detail provided for this log", type, userId, url);
            }
            catch (Exception ex)
            {
                await AddLogAsync(message, $"Could not serialize {logObject.GetType().ToString()} data: {ex.Message}", type, userId, url);
            }
        }

        public async Task AddExceptionAsync<TSource>(string message, Exception ex, LogType type = LogType.Error, string userId = null, string url = null)
        {
            try
            {
                var detail = string.Concat(
                "Exception Message: ", ex.Message, Environment.NewLine,
                "Stack Trace:", Environment.NewLine, ex.StackTrace,
                "Exception JSON:", Environment.NewLine, Environment.NewLine,
                JsonConvert.SerializeObject(ex)
            );
                if (ex.InnerException != null)
                    detail = string.Concat(
                        "Inner Exception: ", ex.InnerException.Message, Environment.NewLine,
                        "Stack Trace:", Environment.NewLine, ex.StackTrace,
                        "Exception JSON:", Environment.NewLine, Environment.NewLine,
                        JsonConvert.SerializeObject(ex));
                await AddLogAsync<TSource>(message, detail, LogType.Error, userId, url);
            }
            catch (Exception serializationExecption)
            {
                await AddLogAsync<TSource>(message, $"Could not serialize exception data: {serializationExecption.Message}", type, userId, url);
            }
        }

        public async Task AddExceptionAsync<TSource>(string message, object logObject, Exception ex, LogType type = LogType.Error, string userId = null, string url = null)
        {
            try
            {
                var detail = string.Concat(
                    $"{typeof(TSource).ToString()}:", Environment.NewLine,
                    JsonConvert.SerializeObject(logObject),
                    "Exception Message: ", ex.Message, Environment.NewLine,
                    "Stack Trace:", Environment.NewLine, ex.StackTrace,
                    "Exception JSON:", Environment.NewLine, Environment.NewLine,
                    JsonConvert.SerializeObject(ex), Environment.NewLine, Environment.NewLine
                );
                if (ex.InnerException != null)
                    detail += string.Concat(
                        "Inner Exception: ", ex.InnerException.Message, Environment.NewLine,
                        "Stack Trace:", Environment.NewLine, ex.StackTrace,
                        "Exception JSON:", Environment.NewLine, Environment.NewLine,
                        JsonConvert.SerializeObject(ex));
                await AddLogAsync<TSource>(message, detail, LogType.Error, userId, url);
            }
            catch (Exception serializationExecption)
            {
                await AddLogAsync<TSource>(message, $"Could not serialize exception or {logObject.GetType().ToString()} data: {serializationExecption.Message}", type, userId, url);
            }
        }

    }
}
