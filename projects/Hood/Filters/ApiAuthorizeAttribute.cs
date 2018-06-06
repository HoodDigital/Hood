using Hood.Caching;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Filters
{
    /// <summary>
    /// This will first check that subscriptions, stripe etc are all enabled and installed correctly, then it checks what is required for forum access and fires the user to the right pages to upgrade if required.
    /// </summary>
    /// <param name="AccessRequired">Determines what type of access is required.</param>
    public class ApiAuthorizeAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// This will first check that subscriptions, stripe etc are all enabled and installed correctly, then it checks what is required for forum access and fires the user to the right pages to upgrade if required.
        /// </summary>
        /// <param name="AccessRequired">Determines what type of access is required.</param>
        public ApiAuthorizeAttribute(AccessLevel AccessRequired = AccessLevel.Restricted) : base(typeof(ApiAuthorizeAttributeImpl))
        {
            Arguments = new object[] { AccessRequired };
        }

        private class ApiAuthorizeAttributeImpl : IActionFilter
        {
            private readonly HoodDbContext _db;
            private readonly ILogger _logger;
            private readonly IBillingService _billing;
            private readonly ISettingsRepository _settings;

            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IAccountRepository _auth;
            private readonly RoleManager<IdentityRole> _roleManager;

            private readonly AccessLevel _access;

            public ApiAuthorizeAttributeImpl(
                HoodDbContext db,
                IAccountRepository auth,
                ILoggerFactory loggerFactory,
                IBillingService billing,
                IHttpContextAccessor contextAccessor,
                IHoodCache cache,
                ISettingsRepository settings,
                RoleManager<IdentityRole> roleManager,
                UserManager<ApplicationUser> userManager,
                AccessLevel access)
            {
                _db = db;
                _auth = new AccountRepository(db, settings, billing, contextAccessor, cache, userManager, roleManager);
                _logger = loggerFactory.CreateLogger<SubscriptionRequiredAttribute>();
                _billing = billing;
                _settings = settings;
                _userManager = userManager;
                _roleManager = roleManager;
                _access = access;
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                // Specific forum based subscription/role required stuff
                string key = null;
                if (context.HttpContext.Request.Query.ContainsKey("key"))
                {
                    key = context.HttpContext.Request.Query["key"].ToString();
                }
                else if (context.HttpContext.Request.Form.ContainsKey("key"))
                {
                    key = context.HttpContext.Request.Form["key"].ToString();
                }

                if (!key.IsSet())
                {
                    LogError("The API creadentials were not supplied.", context.HttpContext.GetSiteUrl(true, true));
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Get the key/id from the encoded string
                var credentials = new ApiKeyPair(key);

                ApiKey apiKey = _db.ApiKeys.SingleOrDefault(f => f.Key == credentials.Key && f.Id == credentials.Id);
                if (apiKey == null)
                {
                    LogError("Could not load API key object from provided credentials.", context.HttpContext.GetSiteUrl(true, true));
                    context.Result = new UnauthorizedResult();
                    return;
                }

                if (!apiKey.Active)
                {
                    LogError("The API key is not active.", context.HttpContext.GetSiteUrl(true, true), apiKey.UserId, apiKey.Id);
                    context.Result = new UnauthorizedResult();
                    return;
                }

                if (apiKey.AccessLevel < _access)
                {
                    LogError("The API key is does not have the required access level.", context.HttpContext.GetSiteUrl(true, true), apiKey.UserId, apiKey.Id);
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // We did it! Now log the access, and the event.

                var apiEvent = new ApiEvent()
                {
                    Action = context.RouteData.Values["action"].ToString(),
                    ApiKeyId = apiKey.Id,
                    IpAddress = context.HttpContext.Connection.RemoteIpAddress.ToString(),
                    RequiredAccessLevel = _access,
                    RouteData = context.RouteData.Values,
                    Time = DateTime.Now,
                    Url = context.HttpContext.Request.Path
                };
                _db.ApiEvents.Add(apiEvent);
                _db.SaveChanges();

                var log = new Log()
                {
                    Type = LogType.Info,
                    Source = LogSource.Api,
                    Detail = "Api was accessed using API Key: " + apiKey.Name,
                    Time = DateTime.Now,
                    Title = "Api was accessed using API Key: " + apiKey.Name,
                    UserId = apiKey.UserId,
                    EntityId = apiEvent.Id.ToString(),
                    EntityType = nameof(ApiEvent),
                    SourceUrl = context.HttpContext.GetSiteUrl(true, true)
                };
                _db.Logs.Add(log);
                _db.SaveChanges();

            }

            private void LogError(string reason, string url, string userId = null, string keyId = null)
            {
                var log = new Log()
                {
                    Type = LogType.Error,
                    Source = LogSource.Api,
                    Detail = "Api attempted access failed with response: " + reason,
                    Time = DateTime.Now,
                    Title = "Api attempted access failed using API Key: " + reason,
                    UserId = userId,
                    EntityId = keyId,
                    EntityType = keyId.IsSet() ? nameof(ApiKey) : null,
                    SourceUrl = url
                };
                _db.Logs.Add(log);
                _db.SaveChanges();
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }
        }
    }
}
