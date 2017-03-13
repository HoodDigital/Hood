using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Hood.Filters
{
    /// <summary>
    /// This checks for the stripe feature, if it is not installed correctly or enabled this will short circuit the controller/action.
    /// </summary>
    public class LockoutModeFilter : IActionFilter
    {
        private readonly ILogger _logger;
        private readonly ISettingsRepository _settings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public LockoutModeFilter(IConfiguration config, 
            ILoggerFactory loggerFactory, 
            IBillingService billing, 
            ISettingsRepository settings,
            UserManager<ApplicationUser> userManager)
        {
            _logger = loggerFactory.CreateLogger<StripeRequiredAttribute>();
            _settings = settings;
            _config = config;
            _userManager = userManager;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            IActionResult result = new RedirectToActionResult("LockoutModeEntrance", "Home", new { returnUrl = context.HttpContext.Request.Path.ToUriComponent() });
            if (_settings.GetBasicSettings().LockoutMode)
            {
                // if this is the login page, or the betalock page allow the user through.
                string action = (string)context.RouteData.Values["action"];
                string controller = (string)context.RouteData.Values["controller"];

                if (action.Equals("LockoutModeEntrance", StringComparison.InvariantCultureIgnoreCase) && 
                    controller.Equals("Home", StringComparison.InvariantCultureIgnoreCase))
                    return;

                if (action.Equals("Index", StringComparison.InvariantCultureIgnoreCase) && 
                    controller.Equals("Home", StringComparison.InvariantCultureIgnoreCase))
                    return;

                if (action.Equals("Login", StringComparison.InvariantCultureIgnoreCase) && 
                    controller.Equals("Account", StringComparison.InvariantCultureIgnoreCase))
                    return;

                // If they are in an override role, let them through.
                if (context.HttpContext.User.Identity.IsAuthenticated)
                {
                    AccountInfo _account = context.HttpContext.GetAccountInfo();

                    string[] _roles = { "SuperUser", "Admin" };
                    if (_userManager.GetRolesAsync(_account.User).Result.Any(r => _roles.Contains(r)))
                        return;
                }

                if (context.HttpContext.IsLockedOut(_settings.LockoutAccessCodes))
                {
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
