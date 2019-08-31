using Hood.Core;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Linq;

namespace Hood.Extensions
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
        public static IApplicationBuilder UseHood(this IApplicationBuilder app, IHostingEnvironment env, IConfiguration config)
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

            return app;
        }

        public static IApplicationBuilder UseHoodDefaults(this IApplicationBuilder app, IHostingEnvironment env, IConfiguration config)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-GB");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/error/500");
            }

            app.Use(async (ctx, next) =>
            {
                await next();

                if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
                {
                    string originalPath = ctx.Request.Path.Value;
                    ctx.Items["originalPath"] = originalPath;
                    ctx.Request.Path = "/error/404";
                    await next();
                }
            });

            if (config.ForceHttps())
            {
                app.UseHttpsRedirection();
                app.UseHsts();
            }

            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse =
                    ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
                    }
            });

            app.UseCookiePolicy();

            // Activate url helpers
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            UrlHelpers.Configure(httpContextAccessor);

            if (config.IsDatabaseConfigured())
            {
                app.UseAuthentication();

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
                    throw new Exception();
                var context = Engine.Services.Resolve<HoodDbContext>();
                var profile = context.UserProfiles.FirstOrDefault();
            }
            catch (Exception)
            {
                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "SiteNotInstalled",
                        template: "{*url}",
                        defaults: new { controller = "Install", action = "Install" }
                    );
                });
                return app;
            }

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
                     name: "Addresses",
                     template: "account/addresses/{action=Index}/{id?}",
                     defaults: new { controller = "Address" }
                );
                routes.MapRoute(
                     name: "Api",
                     template: "account/api/{action=Index}/{id?}",
                     defaults: new { controller = "Api" }
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
            return app;
        }
    }

}
