using Hood.Core;
using Hood.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

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
                if (_config["Hood.SiteUrl"] == null)
                {
                    Engine.Settings["Hood.SiteUrl"] = context.HttpContext.GetSiteUrl();
                }
                else
                {
                    Engine.Settings["Hood.SiteUrl"] = _config["Hood.SiteUrl"];
                }
            }
            else
            {
                if (Engine.Url != context.HttpContext.GetSiteUrl())
                {
                    Engine.Settings["Hood.SiteUrl"] = context.HttpContext.GetSiteUrl();
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
