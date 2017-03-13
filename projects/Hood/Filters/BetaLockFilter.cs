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
    public class BetaLockFilter : IActionFilter
    {
        private readonly ILogger _logger;
        private readonly ISettingsRepository _settings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public BetaLockFilter(IConfiguration config, 
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
            IActionResult result = new RedirectToActionResult("BetaLock", "Home", new { returnUrl = context.HttpContext.Request.Path.ToUriComponent() });
            if (_settings.GetBasicSettings().BetaLock)
            {
                // If they are in an override role, let them through.
                if (context.HttpContext.User.Identity.IsAuthenticated)
                {
                    AccountInfo _account = context.HttpContext.GetAccountInfo();

                    string[] _roles = { "SuperUser", "Admin" };
                    if (_userManager.GetRolesAsync(_account.User).Result.Any(r => _roles.Contains(r)))
                        return;
                }

                byte[] betaCodeBytes = null;
                if (!context.HttpContext.Session.TryGetValue("BetaCode", out betaCodeBytes))
                {
                    context.Result = result;
                    return;
                }

                var betaCode = System.Text.Encoding.Default.GetString(betaCodeBytes);
                var allowedCodes = _settings.GetBasicSettings().BetaCodes.Split(Environment.NewLine.ToCharArray()).ToList();
                allowedCodes.RemoveAll(str => string.IsNullOrEmpty(str));

                string overrideCode = _config["Beta:OverrideKey"];
                if (overrideCode.IsSet())
                    allowedCodes.Add(overrideCode);

                if (!allowedCodes.Contains(betaCode))
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
