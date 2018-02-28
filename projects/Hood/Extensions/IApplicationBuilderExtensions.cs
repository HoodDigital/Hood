using Hood.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;

namespace Hood.Extensions
{

    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseHttpsEnforcement(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            return app.UseMiddleware<EnforceHttpsMiddleware>();
        }

        /// <summary>
        /// Configure Hood, using the default setup, can be configured to add custom routes and priority routes if required.
        /// </summary>
        /// <param name="app">The application builder object.</param>
        /// <param name="env">The app's hosting enviromnent object.</param>
        /// <param name="loggerFactory">The app's logger factory object.</param>
        /// <param name="config">Your app configuration root object.</param>
        /// <param name="customRoutes">Define custom routes, these will replace the Default Routes (so you will need to include a catch-all or default route), but will be added after the page finder routes, and the basic HoodCMS routes.</param>
        /// <param name="priorityRoutes">Define priority routes, these will be added before the page finder routes, and any basic HoodCMS routes.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseHood(this IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConfiguration config)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (env == null)
                throw new ArgumentNullException(nameof(env));
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-GB");

            if (env.IsEnvironment("Development") || env.IsEnvironment("PreProduction") || env.IsEnvironment("Staging"))
            {
                loggerFactory.AddDebug(LogLevel.Debug);
                loggerFactory.AddConsole(LogLevel.Debug);
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseStatusCodePagesWithReExecute("/error/{0}");
            }

            if (config.ForceHttps())
            {
                app.UseHttpsEnforcement();
            }

            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse =
                    ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
                    }
            });

            // Activate url helpers
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            UrlHelpers.Configure(httpContextAccessor);

            if (config.IsDatabaseConfigured())
            {
                app.UseAuthentication();
            }

            int timeout = 60;
            var builder = new CookieBuilder()
            {
                Name = config["Session:CookieName"].IsSet() ? config["Session:CookieName"] : ".Hood.Session"
            };
            if (int.TryParse(config["Session:Timeout"], out timeout))
                builder.Expiration = TimeSpan.FromMinutes(timeout);
            else
                builder.Expiration = TimeSpan.FromMinutes(60);

            app.UseSession(new SessionOptions()
            {
                IdleTimeout = builder.Expiration.Value,
                Cookie = builder
            });

            return app;
        }

        public static IApplicationBuilder UseDefaultRoutesForHood(this IApplicationBuilder app, IConfiguration config)
        {
            if (!config.IsDatabaseConfigured())
            {
                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "SiteNotInstalled",
                        template: "{*url}",
                        defaults: new { controller = "Install", action = "Install" }
                    );
                });
            }
            else
            {
                app.UseMvc(routes =>
                {
                    // Check for a url string that matches pages, content routes or custom user set urls. Maximum of five '/' allowed in the route.
                    routes.MapRoute(
                        name: "Content",
                        template: "{lvl1:cms}/{lvl2:cms?}/{lvl3:cms?}/{lvl4:cms?}/{lvl5:cms?}",
                        defaults: new { controller = "Home", action = "Show" }
                    );
                    routes.MapRoute(
                         name: "Manage",
                         template: "account/manage/{action=Index}/{id?}",
                         defaults: new { controller = "Manage" }
                    );
                    routes.MapRoute(
                         name: "Billing",
                         template: "account/billing/{action=Index}/{id?}",
                         defaults: new { controller = "Billing" }
                    );
                    routes.MapRoute(
                         name: "Subscriptions",
                         template: "account/subscriptions/{action=Index}/{id?}",
                         defaults: new { controller = "Subscriptions" }
                    );
                    routes.MapRoute(
                        name: "Areas",
                        template: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                    );
                    routes.MapRoute(
                        name: "Default",
                        template: "{controller=Home}/{action=Index}/{id?}"
                    );
                });
            }
            return app;
        }
    }
}
