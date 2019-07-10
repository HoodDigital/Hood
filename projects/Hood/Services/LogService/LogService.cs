using System.Threading.Tasks;
using Hood.Models;
using System;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Collections;
using Hood.Extensions;

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
                if (logObject is string)
                    await AddLogAsync(message, logObject.ToString(), type, userId, url, typeof(TSource).ToString());
                else
                    await AddLogAsync(message, logObject.ToJson(), type, userId, url, typeof(TSource).ToString());
            }
            catch (Exception loggingException)
            {
                await AddExceptionAsync<TSource>("Error logging exception.", loggingException, type, userId, url);
            }
        }

        public async Task AddExceptionAsync<TSource>(string message, Exception ex, LogType type = LogType.Error, string userId = null, string url = null)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new ErrorLogDetail
                {
                    Exception = ex.ToDictionary(),
                    InnerException = ex.InnerException?.ToDictionary()
                });
                await AddLogAsync<TSource>(message, json, LogType.Error, userId, url);
            }
            catch (Exception loggingException)
            {
                await AddExceptionAsync<TSource>("Error logging exception.", loggingException, type, userId, url);
            }
        }

        public async Task AddExceptionAsync<TSource>(string message, object logObject, Exception ex, LogType type = LogType.Error, string userId = null, string url = null)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new ErrorLogDetail {
                    ObjectJson = logObject.ToJson(),
                    Exception = ex.ToDictionary(),
                    InnerException = ex.InnerException?.ToDictionary()
                });
                await AddLogAsync<TSource>(message, json, LogType.Error, userId, url);
            }
            catch (Exception loggingException)
            {
                await AddExceptionAsync<TSource>("Error logging exception.", loggingException, type, userId, url);
            }
        }

    }

}
