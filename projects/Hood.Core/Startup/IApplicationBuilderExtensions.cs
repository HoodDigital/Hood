using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Identity;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Hood.Startup
{

    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseHood(this IApplicationBuilder app, IWebHostEnvironment env, IConfiguration config)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-GB");

            if (env.EnvironmentName == "Development" || env.EnvironmentName == "Hood")
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/error/500");
                app.UseStatusCodePagesWithReExecute("/error/{0}");
            }

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".md"] = "text/markdown";
            provider.Mappings[".webmanifest"] = "text/json";
            app.UseStaticFiles(new StaticFileOptions()
            {
                HttpsCompression = Microsoft.AspNetCore.Http.Features.HttpsCompressionMode.Compress,
                ContentTypeProvider = provider,
                OnPrepareResponse =
                    ctx =>
                    {
                        ctx.Context.Response.Headers["Cache-Control"] = "max-age=600";
                    }
            });

            app.UseRouting();
            app.UseCors();

            // Activate url helpers
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            UrlHelpers.Configure(httpContextAccessor);

            if (config.IsDatabaseConnected())
            {
                app.UseAuthentication();
                app.UseAuthorization();

                var cookieName = config["Identity:Cookies:Name"].IsSet() ? config["Identity:Cookies:Name"] : Constants.CookieDefaultName;

                var builder = new CookieBuilder() { Name = $".{cookieName}.Session" };
                builder.Expiration = TimeSpan.FromMinutes(config.GetValue("Session:Timeout", 60));

                app.UseSession(new SessionOptions()
                {
                    IdleTimeout = builder.Expiration.Value,
                    Cookie = builder
                });
            }

            app.UseHoodComponents(env, config);
            app.UseHoodDefaultRoutes(config);

            return app;
        }
        public static IApplicationBuilder UseHoodComponents(this IApplicationBuilder app, IWebHostEnvironment env, IConfiguration config)
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
        public static IApplicationBuilder UseHoodDefaultRoutes(this IApplicationBuilder app, IConfiguration config)
        {
            try
            {
                if (config.IsDatabaseConnected())
                {
                    try
                    {
                        var context = Engine.Services.Resolve<HoodDbContext>();
                        var profile = context.UserProfiles.FirstOrDefault();
                    }
                    catch (Microsoft.Data.SqlClient.SqlException ex)
                    {
                        throw new StartupException("Database views are not installed.", ex, StartupError.DatabaseViewsNotInstalled);
                    }
                }
            }
            catch (StartupException)
            { }

            app.UseEndpoints(endpoints =>
            {

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
