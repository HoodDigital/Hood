using System;
using System.Threading.Tasks;
using Hood.Core;
using Hood.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Hood.Attributes
{
    /// <summary>
    /// Only allows users who are not active, and have been flagged for needing connection.
    /// </summary>
    public class DisconnectedUsersOnlyAttribute : TypeFilterAttribute
    {
        public DisconnectedUsersOnlyAttribute() : base(typeof(DisconnectedUsersOnlyFilter))
        { }

    }

    public class DisconnectedUsersOnlyFilter : Attribute, IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var linkGenerator = Engine.Services.Resolve<LinkGenerator>();
            if (!context.HttpContext.User.Identity.IsAuthenticated || !context.HttpContext.User.RequiresConnection())
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", new { });
            }
            return Task.CompletedTask;
        }
    }
}
