using System;
using Hood.Core;
using Hood.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Hood.Attributes
{
    /// <summary>
    /// This checks for the stripe feature, if it is not installed correctly or enabled this will short circuit the controller/action.
    /// </summary>
    public class DisableForAuth0Attribute : ActionFilterAttribute
    {
        private readonly IConfiguration _config;

        public DisableForAuth0Attribute()
        {
            _config = Engine.Services.Resolve<IConfiguration>();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (Engine.Auth0Enabled)
            {
                if (context.HttpContext.User.IsAdminOrBetter())
                {
                    throw new ApplicationException("This endpoint is disbaled when using Auth0.");
                }
                context.Result = new NotFoundResult();
            }
        }
    }
}
