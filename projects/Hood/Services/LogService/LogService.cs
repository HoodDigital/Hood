using Hood.Extensions;
using Hood.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class LogService : ILogService
    {
        protected HoodDbContext _hoodDbContext
        {
            get
            {
                DbContextOptionsBuilder<HoodDbContext> options = new DbContextOptionsBuilder<HoodDbContext>();
                options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
                return new HoodDbContext(options.Options);
            }
        }
        private readonly IConfiguration _config;

        public LogService(IConfiguration config)
        {
            _config = config;
        }

        private async Task AddLogAsync(string message, string detail = "", LogType type = LogType.Info, string userId = null, string url = null, string source = "")
        {
            using (HoodDbContext context = _hoodDbContext)
            {
                Log log = new Log()
                {
                    Type = type,
                    Source = source,
                    Detail = detail,
                    Time = DateTime.Now,
                    Title = message,
                    UserId = userId,
                    SourceUrl = url
                };
                context.Logs.Add(log);
                await context.SaveChangesAsync();
            }
        }

        public async Task AddLogAsync<TSource>(string message, object logObject = null, LogType type = LogType.Info, string userId = null, string url = null)
        {
            try
            {
                if (logObject is string)
                {
                    await AddLogAsync(message, logObject.ToString(), type, userId, url, typeof(TSource).ToString());
                }
                else
                {
                    await AddLogAsync(message, logObject.ToJson(), type, userId, url, typeof(TSource).ToString());
                }
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
                string json = JsonConvert.SerializeObject(new ErrorLogDetail
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
                string json = JsonConvert.SerializeObject(new ErrorLogDetail
                {
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
