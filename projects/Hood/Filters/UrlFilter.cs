using Hood.Core;
using Hood.Extensions;
using Hood.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;

namespace Hood.Filters
{
    /// <summary>
    /// This checks for the stripe feature, if it is not installed correctly or enabled this will short circuit the controller/action.
    /// </summary>
    public class UrlFilter : IActionFilter
    {
        private readonly IConfiguration _config;

        public UrlFilter()
        {
            _config = Engine.Services.Resolve<IConfiguration>();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (Engine.Url == null)
            {
                if (_config["Hood:SiteUrl"] == null)
                {
                    Engine.Settings["Hood:SiteUrl"] = context.HttpContext.GetSiteUrl();
                }
                else
                {
                    Engine.Settings["Hood:SiteUrl"] = _config["Hood:SiteUrl"];
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
