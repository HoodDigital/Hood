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
    public class LockoutModeFilter : IActionFilter
    {
        private readonly ILogService _logService;
        private readonly IConfiguration _config;

        public LockoutModeFilter()
        {
            _logService = Engine.Services.Resolve<ILogService>();
            _config = Engine.Services.Resolve<IConfiguration>();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!_config.IsDatabaseConfigured())
                return;

            IActionResult result = new RedirectToActionResult("LockoutModeEntrance", "Home", new { returnUrl = context.HttpContext.Request.Path.ToUriComponent() });
            var basicSettings = Engine.Settings.Basic;
            if (basicSettings.LockoutMode)
            {
                // if this is the login page, or the betalock page allow the user through.
                string action = (string)context.RouteData.Values["action"];
                string controller = (string)context.RouteData.Values["controller"];

                if (action.Equals("LockoutModeEntrance", StringComparison.InvariantCultureIgnoreCase) &&
                    controller.Equals("Hood", StringComparison.InvariantCultureIgnoreCase))
                    return;

                if (action.Equals("WebHooks", StringComparison.InvariantCultureIgnoreCase) &&
                    controller.Equals("Subscriptions", StringComparison.InvariantCultureIgnoreCase))
                    return;

                if (action.Equals("Index", StringComparison.InvariantCultureIgnoreCase) &&
                    controller.Equals("Home", StringComparison.InvariantCultureIgnoreCase))
                    return;

                if (!basicSettings.LockLoginPage)
                {
                    if (action.Equals("Login", StringComparison.InvariantCultureIgnoreCase) &&
                        controller.Equals("Account", StringComparison.InvariantCultureIgnoreCase))
                        return;
                }

                // If they are in an override role, let them through.
                if (context.HttpContext.User.IsAdminOrBetter())
                {
                    _logService.AddLogAsync<LockoutModeFilter>($"User, {context.HttpContext.User.Identity.Name}, accessed the site through the code lockout, as they are an administrator.");
                    return;
                }

                if (context.HttpContext.IsLockedOut(Engine.Settings.LockoutAccessCodes))
                {
                    _logService.AddLogAsync<LockoutModeFilter>($"User, {context.HttpContext.User}, accessed the site through the code lockout, as they are an administrator.");
                    context.Result = result;
                    return;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
