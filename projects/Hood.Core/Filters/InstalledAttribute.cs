using System;
using System.Threading.Tasks;
using Hood.Core;
using Hood.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Hood.Filters
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InstalledAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!Engine.Services.Installed)
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new
                        {
                            controller = "Install",
                            action = "Install"
                        }
                    )
                );
                return;
            }

            if (Engine.Configuration.InitializeOnStartup || Engine.Auth0Configuration.SetupRemoteOnIntitialize)
            {
                if (Engine.Auth0Enabled && Engine.Auth0Configuration.SetupRemoteOnIntitialize)
                {
                    var authService = new Auth0Service();
                    await authService.InitialiseApp();
                }
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new
                        {
                            controller = "Install",
                            action = "Initialized"
                        }
                    )
                );
                return;
            }

            await next();
        }
    }
}
