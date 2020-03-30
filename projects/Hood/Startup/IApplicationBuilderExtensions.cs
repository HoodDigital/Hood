using Hood.Core;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace Hood.Startup
{

    public static class IApplicationBuilderExtensions
    {
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
        public static IApplicationBuilder UseHood(this IApplicationBuilder app, IWebHostEnvironment env, IConfiguration config)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (env == null)
                throw new ArgumentNullException(nameof(env));
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            // Register all Hood Components
            var typeFinder = Engine.Services.Resolve<ITypeFinder>();
            var dependencies = typeFinder.FindClassesOfType<IHoodComponent>();

            var instances = dependencies
                                .Select(dependencyRegistrar => (IHoodComponent)Activator.CreateInstance(dependencyRegistrar))
                                .OrderBy(dependencyRegistrar => dependencyRegistrar.ServiceConfigurationOrder);

            foreach (var dependency in instances)
                dependency.Configure(app, env, config);

            if (config.IsDatabaseConfigured())
            {
                ScheduledTaskManager.Instance.Initialize();
                ScheduledTaskManager.Instance.Start();
            }

            return app;
        }

        public static IApplicationBuilder UseHoodDefaults(this IApplicationBuilder app, IWebHostEnvironment env, IConfiguration config)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-GB");

            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/error/500");
                app.UseStatusCodePagesWithReExecute("/error/{0}");
            }

            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse =
                    ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
                    }
            });

            app.UseRouting();
            app.UseCors();
            
            // Activate url helpers
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            UrlHelpers.Configure(httpContextAccessor);

            if (config.IsDatabaseConfigured())
            {
                app.UseAuthentication();
                app.UseAuthorization();

                var cookieName = config["Cookies:Name"].IsSet() ? config["Cookies:Name"] : "Hood";

                var builder = new CookieBuilder() { Name = $".{cookieName}.Session" };
                builder.Expiration = TimeSpan.FromMinutes(config.GetValue("Session:Timeout", 60));

                app.UseSession(new SessionOptions()
                {
                    IdleTimeout = builder.Expiration.Value,
                    Cookie = builder
                });
            }

            return app;
        }
        public static IApplicationBuilder UseHoodDefaultRoutes(this IApplicationBuilder app, IConfiguration config)
        {
            try
            {
                if (!config.IsDatabaseConfigured())
                    throw new StartupException();
                var context = Engine.Services.Resolve<HoodDbContext>();
                try
                {
                    var profile = context.UserProfiles.FirstOrDefault();
                    Engine.Services.ViewsInstalled = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Message.Contains("Invalid object name 'HoodUserProfiles'"))
                    {
                        Engine.Services.ViewsInstalled = false;
                    }
                    throw new StartupException();
                }
            }
            catch (StartupException)
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "SiteNotInstalled",
                        pattern: "{*url}",
                        defaults: new { controller = "Install", action = "Install" }
                    );
                });
                return app;
            }

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllerRoute(
                     name: "Manage",
                     pattern: "account/manage/{action=Index}/{id?}",
                     defaults: new { controller = "Manage" }
                );
                endpoints.MapControllerRoute(
                     name: "Billing",
                     pattern: "account/billing/{action=Index}/{id?}",
                     defaults: new { controller = "Billing" }
                );
                endpoints.MapControllerRoute(
                     name: "Addresses",
                     pattern: "account/addresses/{action=Index}/{id?}",
                     defaults: new { controller = "Address" }
                );
                endpoints.MapControllerRoute(
                     name: "Api",
                     pattern: "account/api/{action=Index}/{id?}",
                     defaults: new { controller = "Api" }
                );
                endpoints.MapControllerRoute(
                     name: "Subscriptions",
                     pattern: "account/subscriptions/{action=Index}/{id?}",
                     defaults: new { controller = "Subscriptions" }
                );
                endpoints.MapControllerRoute(
                    name: "Areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );

                endpoints.MapControllerRoute(
                    name: "Default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"
                );

            });
            return app;
        }
    }

}
