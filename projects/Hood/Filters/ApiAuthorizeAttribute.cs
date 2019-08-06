using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

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
            private readonly ILogService _logService;
            private readonly AccessLevel _access;

            public ApiAuthorizeAttributeImpl(
                HoodDbContext db,
                ILogService logService,
                AccessLevel access)
            {
                _db = db;
                _logService = logService;
                _access = access;
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                try
                {
                    // Specific forum based subscription/role required stuff
                    string key = null;
                    if (context.HttpContext.Request.Query.ContainsKey("key") && context.HttpContext.Request.Method == "GET" && _access == AccessLevel.Public)
                    {
                        key = context.HttpContext.Request.Query["key"].ToString();
                    }
                    else if (context.HttpContext.Request.Headers.ContainsKey("Authorization") && context.HttpContext.Request.Method == "POST" && _access >= AccessLevel.Restricted)
                    {
                        key = context.HttpContext.Request.Headers["Authorization"].ToString().Split(' ')[1];
                    }

                    if (!key.IsSet())
                        throw new Exception("No API credentials were not supplied, please supply a valid authorization token.");

                    // get the Id value from the claims in the token (ClaimTypes.NameIdentifier)
                    var id = new JwtSecurityTokenHandler().ReadJwtToken(key).Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                    // load the api key object from the db.
                    ApiKey apiKey = _db.ApiKeys.SingleOrDefault(f => f.Id == id);

                    if (apiKey == null)
                        throw new Exception("Could not load API key object from provided credentials.");

                    // Validate the JWT Signature using HS256 and the private key from the ApiKey object.
                    if (!apiKey.ValidateToken(key, _access))
                        throw new Exception("Signature validation failed.");

                    // Check the ApiKey is still active.
                    if (!apiKey.Active)
                        throw new Exception("The API key is not active.");

                    // Check the ApiKey is has the right access level.
                    if (apiKey.AccessLevel < _access)
                        throw new Exception("The API key is does not have the required access level.");

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

                    _logService.AddLogAsync<ApiAuthorizeAttribute>("Api was accessed using API Key: " + apiKey.Name, userId: apiKey.UserId, url: context.HttpContext.GetSiteUrl(true, true));
                }
                catch (Exception ex)
                {
                    _logService.AddExceptionAsync<ApiAuthorizeAttribute>("Error accessing API using an API Key.", ex, url: context.HttpContext.GetSiteUrl(true, true));
                    context.Result = new UnauthorizedResult();
                    return;
                }

            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }
        }
    }
}
