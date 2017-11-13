using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Hood.Filters
{
    /// <summary>
    /// This will first check that subscriptions, stripe etc are all enabled and installed correctly, it will then run a user check, and save the subscription info into the context pipeline in Items["AccountInfo"]
    /// </summary>
    public class AccountFilter : IActionFilter
    {
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountRepository _auth;

        public AccountFilter(ILoggerFactory loggerFactory, 
                                  IAccountRepository auth,
                                  UserManager<ApplicationUser> userManager)
        {
            _logger = loggerFactory.CreateLogger<AccountFilter>();
            _auth = auth;
            _userManager = userManager;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            AccountInfo info = _auth.LoadAccountInfo(_userManager.GetUserId(context.HttpContext.User));
            context.HttpContext.Items["AccountInfo"] = info;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
