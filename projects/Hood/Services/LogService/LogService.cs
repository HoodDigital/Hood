using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _contextAccessor;

        public LogService(IConfiguration config, IHttpContextAccessor contextAccessor)
        {
            _config = config;
            _contextAccessor = contextAccessor;
        }

        private async Task AddLogAsync(string message, string detail = "", LogType type = LogType.Info, string source = "")
        {
            if (!Engine.Services.Installed)
                return;

            string userId = null;
            if (Engine.Account != null)
            {
                userId = Engine.Account.Id;
            }

            string url = null;
            if (_contextAccessor.HttpContext != null)
            {
                url = _contextAccessor.HttpContext.GetSiteUrl(true, true);
            }
            
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

        public async Task AddLogAsync<TSource>(string message, object logObject = null, LogType type = LogType.Info)
        {
            try
            {
                if (logObject is string)
                {
                    await AddLogAsync(message, logObject.ToString(), type, typeof(TSource).ToString());
                }
                else
                {
                    await AddLogAsync(message, logObject.ToJson(), type, typeof(TSource).ToString());
                }
            }
            catch (Exception loggingException)
            {
                await AddExceptionAsync<TSource>("Error logging exception.", loggingException, type);
            }
        }

        public async Task AddExceptionAsync<TSource>(string message, Exception ex, LogType type = LogType.Error)
        {
            try
            {
                string json = JsonConvert.SerializeObject(new ErrorLogDetail
                {
                    Exception = ex.ToDictionary(),
                    InnerException = ex.InnerException?.ToDictionary()
                });
                await AddLogAsync<TSource>(message, json, LogType.Error);
            }
            catch (Exception loggingException)
            {
                await AddExceptionAsync<TSource>("Error logging exception.", loggingException, type);
            }
        }

        public async Task AddExceptionAsync<TSource>(string message, object logObject, Exception ex, LogType type = LogType.Error)
        {
            try
            {
                string json = JsonConvert.SerializeObject(new ErrorLogDetail
                {
                    ObjectJson = logObject.ToJson(),
                    Exception = ex.ToDictionary(),
                    InnerException = ex.InnerException?.ToDictionary()
                });
                await AddLogAsync<TSource>(message, json, LogType.Error);
            }
            catch (Exception loggingException)
            {
                await AddExceptionAsync<TSource>("Error logging exception.", loggingException, type);
            }
        }

    }

}
