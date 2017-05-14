using Hood.Extensions;
using Hood.Filters;
using Hood.Infrastructure;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System;
using System.Globalization;
using Hood.IO;
using Hood.Caching;

namespace Hood
{
    public static class HoodStartup
    {
        /// <summary>
        /// Configure Services for the application to run HoodCMS. 
        /// </summary>
        /// <param name="config">The site's conficuration object. In a default ASP.NET application template this is called Configuration.</param>
        /// <param name="services">The IServiceCollection. Standard parameter of Startup.ConfigureServices().</param>
        public static void AddHood<TContext>(this IServiceCollection services, IConfigurationRoot config)
            where TContext : HoodDbContext
        {
            config.ConfigSetup("Installed:DB", "ConnectionStrings:DefaultConnection");

            config.ConfigSetup("Installed:ApplicationInsights", "ApplicationInsights:Key");

            // Set the installed flag
            config.ConfigSetup("Installed", "Installed:DB");

            // Check optional app settings
            config.ConfigSetup("Installed:Facebook", "Authentication:Facebook:AppId", "Authentication:Facebook:Secret");
            config.ConfigSetup("Installed:Google", "Authentication:Google:AppId", "Authentication:Google:Secret");

            services.AddMvc();

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(EmbeddedFiles.GetProvider());
                if (config.CheckSetup("Installed:DB"))
                {
                    options.ViewLocationExpanders.Add(new ViewLocationExpander());
                }
            });

            // Add framework services.
            if (config.CheckSetup("Installed:DB"))
            {
                services.AddDbContext<TContext>(options => options.UseSqlServer(config["ConnectionStrings:DefaultConnection"], b => { b.UseRowNumberForPaging(); }));
                services.AddDbContext<HoodDbContext>(options => options.UseSqlServer(config["ConnectionStrings:DefaultConnection"], b => { b.UseRowNumberForPaging(); }));


                services.AddIdentity<ApplicationUser, IdentityRole>(o =>
                {
                    // configure identity options
                    o.Password.RequireDigit = config["Identity:Password:RequireDigit"].IsSet() ? bool.Parse(config["Identity:Password:RequireDigit"]) : true;
                    o.Password.RequireLowercase = config["Identity:Password:RequireLowercase"].IsSet() ? bool.Parse(config["Identity:Password:RequireLowercase"]) : false;
                    o.Password.RequireUppercase = config["Identity:Password:RequireUppercase"].IsSet() ? bool.Parse(config["Identity:Password:RequireUppercase"]) : false;
                    o.Password.RequireNonAlphanumeric = config["Identity:Password:RequireNonAlphanumeric"].IsSet() ? bool.Parse(config["Identity:Password:RequireNonAlphanumeric"]) : true;
                    o.Password.RequiredLength = config["Identity:Password:RequiredLength"].IsSet() ? int.Parse(config["Identity:Password:RequiredLength"]) : 6;
                })
                .AddEntityFrameworkStores<HoodDbContext>()
                .AddDefaultTokenProviders();

                services.AddSingleton<EventsService>();
                services.AddSingleton<SubscriptionsEventListener>();
                services.AddSingleton<ContentCategoryCache>();
                services.AddSingleton<ContentByTypeCache>();
                services.AddSingleton<IRazorViewRenderer, RazorViewRenderer>();
                services.AddSingleton<IHoodCache, HoodCache>();
                services.AddSingleton<IFTPService, FTPService>();
                services.AddSingleton<IPropertyImporter, PropertyImporter>();
                services.AddSingleton<IMediaRefreshService, MediaRefreshService>();
                services.AddSingleton<IPropertyExporter, PropertyExporter>();
                services.AddSingleton<IContentExporter, ContentExporter>();
                services.AddSingleton<IThemesService, ThemesService>();
                services.AddSingleton<IAddressService, AddressService>();
                services.AddScoped<ISettingsRepository, SettingsRepository>();
                services.AddScoped<IAccountRepository, AccountRepository>();
                services.AddScoped<IPropertyRepository, PropertyRepository>();
                services.AddScoped<IMediaManager<SiteMedia>, MediaManager<SiteMedia>>();
                services.AddScoped<IContentRepository, ContentRepository>();
                services.AddScoped<IStripeWebHookService, StripeWebHookService>();
                services.AddTransient<ISmsSender, SmsSender>();
                services.AddTransient<IPayPalService, PayPalService>();
                services.AddTransient<IShoppingCart, ShoppingCart>();
                services.AddTransient<IStripeService, StripeService>();
                services.AddTransient<ISubscriptionPlanService, SubscriptionPlanService>();
                services.AddTransient<ISubscriptionService, SubscriptionService>();
                services.AddTransient<ICardService, CardService>();
                services.AddTransient<ICustomerService, CustomerService>();
                services.AddTransient<IInvoiceService, InvoiceService>();
                services.AddTransient<IBillingService, BillingService>();
                services.AddTransient<IEmailSender, EmailSender>();
                services.AddTransient<IFormSenderService, FormSenderService>();

                services.Configure<RouteOptions>(options =>
                {
                    options.ConstraintMap.Add("cms", typeof(CmsUrlConstraint));
                    options.LowercaseUrls = true;
                });

                services.Configure<MvcJsonOptions>(opt =>
                {
                    var resolver = opt.SerializerSettings.ContractResolver;
                    if (resolver != null)
                    {
                        var res = resolver as DefaultContractResolver;
                        res.NamingStrategy = null;  // <<!-- this removes the camelcasing
                    }
                });

                services.Configure<MvcOptions>(options =>
                {
                    // Global filters
                    options.Filters.Add(typeof(AccountFilter));
                    options.Filters.Add(typeof(LockoutModeFilter));

                    options.CacheProfiles.Add("Year",
                        new CacheProfile
                        {
                            Location = ResponseCacheLocation.Client,
                            Duration = 31536000
                        });
                    options.CacheProfiles.Add("Month",
                        new CacheProfile
                        {
                            Location = ResponseCacheLocation.Client,
                            Duration = 2629000
                        });
                    options.CacheProfiles.Add("Week",
                        new CacheProfile
                        {
                            Location = ResponseCacheLocation.Client,
                            Duration = 604800
                        });
                    options.CacheProfiles.Add("Day",
                        new CacheProfile
                        {
                            Location = ResponseCacheLocation.Client,
                            Duration = 86400
                        });
                    options.CacheProfiles.Add("Hour",
                        new CacheProfile
                        {
                            Location = ResponseCacheLocation.Client,
                            Duration = 3600
                        });
                    options.CacheProfiles.Add("HalfHour",
                         new CacheProfile
                         {
                             Location = ResponseCacheLocation.Client,
                             Duration = 1800
                         });
                    options.CacheProfiles.Add("TenMinutes",
                         new CacheProfile
                         {
                             Location = ResponseCacheLocation.Client,
                             Duration = 600
                         });

                });

            }
            else
            {
                services.AddScoped<ISettingsRepository, SettingsRepositoryStub>();
            }
            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddSingleton<IConfiguration>(config);

            services.AddApplicationInsightsTelemetry(config);
        }

        /// <summary>
        /// Configure the application to run HoodCMS. 
        /// </summary>
        /// <param name="app">The IApplicationBuilder. Standard parameter of Startup.ConfigureServices().</param>
        /// <param name="env">The IHostingEnvironment. Standard parameter of Startup.ConfigureServices().</param>
        /// <param name="loggerFactory">The ILoggerFactory. Standard parameter of Startup.ConfigureServices().</param>
        /// <param name="config">The IConfigurationRoot. Standard parameter of Startup.ConfigureServices().</param>
        /// <param name="customRoutes">Routes to add to the sites route map. These will be added after the standard HoodCMS routes, but before the standard catch all route {area:exists}/{controller=Home}/{action=Index}/{id?}.</param>
        public static void UseHood<TContext>(this IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConfigurationRoot config, Action<IRouteBuilder> configureRoutes = null)
            where TContext : HoodDbContext
        {
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

            if (config.CheckSetup("Installed:DB"))
            {

                // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    serviceScope.ServiceProvider.GetService<TContext>().Database.Migrate();
                    var userManager = app.ApplicationServices.GetService<UserManager<ApplicationUser>>();
                    var roleManager = app.ApplicationServices.GetService<RoleManager<IdentityRole>>();
                    var options = new DbContextOptionsBuilder<HoodDbContext>();
                    options.UseSqlServer(config["ConnectionStrings:DefaultConnection"]);
                    var db = new HoodDbContext(options.Options);
                    db.EnsureSetup(userManager, roleManager);

                    // Initialise events
                    serviceScope.ServiceProvider.GetService<SubscriptionsEventListener>().Configure();
                }
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

            if (config.CheckSetup("Installed:DB"))
            {
                app.UseIdentity();
            }

            app.UseSession();

            if (!config.CheckSetup("Installed"))
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
                    routes.MapRoute(
                        name: "ContentCheck",
                        template: "{lvl1:cms}/{lvl2:cms?}/{lvl3:cms?}/{lvl4:cms?}/{lvl5:cms?}",
                        defaults: new { controller = "Home", action = "Show" });

                    routes.MapRoute(
                        name: "areaRoute",
                        template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                    configureRoutes?.Invoke(routes);

                    routes.MapRoute(
                        name: "default-fallback",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });
            }
        }

    }
}
