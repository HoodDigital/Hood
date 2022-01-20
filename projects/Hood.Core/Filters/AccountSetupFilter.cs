using Hood.Core;
using Hood.Extensions;
using Hood.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Hood.Filters
{
    /// <summary>
    /// This checks for the stripe feature, if it is not installed correctly or enabled this will short circuit the controller/action.
    /// </summary>
    public class AccountSetupFilter : IActionFilter
    {
        public AccountSetupFilter()
        { }
        
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!Engine.Auth0Enabled)
            {
                return;
            }
            if (context.HttpContext.User.IsNotSetup())
            {
                context.Result = new RedirectToActionResult("CompleteSetup", "Account", new { });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
