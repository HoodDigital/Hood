using Hood.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
                    r =>
                    {
                        string path = r.File.PhysicalPath;
                        if (path.EndsWith(".css") || path.EndsWith(".js") || path.EndsWith(".gif") || path.EndsWith(".jpg") || path.EndsWith(".png") || path.EndsWith(".svg"))
                        {
                            TimeSpan maxAge = new TimeSpan(7, 0, 0, 0);
                            r.Context.Response.Headers.Append("Cache-Control", "max-age=" + maxAge.TotalSeconds.ToString("0"));
                        }
                    }
            });

            // Activate url helpers
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            UrlHelpers.Configure(httpContextAccessor);

            if (config.IsDatabaseConfigured())
            {
                app.UseAuthentication();
            }

            app.UseSession();

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
                        name: "ContentCheck",
                        template: "{lvl1:cms}/{lvl2:cms?}/{lvl3:cms?}/{lvl4:cms?}/{lvl5:cms?}",
                        defaults: new { controller = "Home", action = "Show" });

                    routes.MapRoute(
                        name: "areaRoute",
                        template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                    routes.MapRoute(
                        name: "default-fallback",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });
            }

            return app;
        }
    }
}
