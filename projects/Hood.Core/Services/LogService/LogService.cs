using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

            if (Engine.Configuration.LogLevel == LogLevel.None)
            {
                return;
            }

            switch (type)
            {
                case LogType.Error:
                    if (Engine.Configuration.LogLevel >= LogLevel.None)
                    {
                        return;
                    }
                    break;
                case LogType.Warning:
                    if (Engine.Configuration.LogLevel >= LogLevel.Error)
                    {
                        return;
                    }
                    break;
                case LogType.Info:
                    if (Engine.Configuration.LogLevel >= LogLevel.Warning)
                    {
                        return;
                    }
                    break;
                default:
                    if (Engine.Configuration.LogLevel >= LogLevel.Information)
                    {
                        return;
                    }
                    break;
            }

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
                var _logger = Engine.Services.Resolve<ILogger<TSource>>();
                switch (type)
                {
                    case LogType.Error:
                        _logger.LogError(message);
                        break;
                    case LogType.Warning:
                        _logger.LogWarning(message);
                        break;
                    default:
                        _logger.LogInformation(message);
                        break;
                }

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
                var _logger = Engine.Services.Resolve<ILogger<TSource>>();
                switch (type)
                {
                    case LogType.Error:
                        _logger.LogError(ex, message);
                        break;
                    case LogType.Warning:
                        _logger.LogWarning(ex, message);
                        break;
                    default:
                        _logger.LogInformation(ex, message);
                        break;
                }
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
                var _logger = Engine.Services.Resolve<ILogger<TSource>>();
                switch (type)
                {
                    case LogType.Error:
                        _logger.LogError(ex, message);
                        break;
                    case LogType.Warning:
                        _logger.LogWarning(ex, message);
                        break;
                    default:
                        _logger.LogInformation(ex, message);
                        break;
                }
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
