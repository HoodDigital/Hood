using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Hood.Extensions;
using Microsoft.AspNetCore.Http;
using Hood.Services;
using Hood.Models;
using Microsoft.AspNetCore.Identity;

namespace Hood.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment() || env.EnvironmentName == "PreProduction")
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Configuration.ConfigSetup("Installed:DB", "ConnectionStrings:DefaultConnection");

            Configuration.ConfigSetup("Installed:ApplicationInsights", "ApplicationInsights:Key");

            // Set the installed flag
            Configuration.ConfigSetup("Installed", "Installed:DB");

            // Check optional app settings
            Configuration.ConfigSetup("Installed:Facebook", "Authentication:Facebook:AppId", "Authentication:Facebook:Secret");
            Configuration.ConfigSetup("Installed:Google", "Authentication:Google:AppId", "Authentication:Google:Secret");

            services.AddMvc();

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(EmbeddedFiles.GetProvider());
                if (Configuration.CheckSetup("Installed:DB"))
                {
                    options.ViewLocationExpanders.Add(new ViewLocationExpander());
                }
            });

            // Add framework services.
            if (Configuration.CheckSetup("Installed:DB"))
            {
                services.AddDbContext<TContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"], b => { b.UseRowNumberForPaging(); }));
                services.AddDbContext<HoodDbContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"], b => { b.UseRowNumberForPaging(); }));


                services.AddIdentity<ApplicationUser, IdentityRole>(o =>
                {
                    // configure identity options
                    o.Password.RequireDigit = Configuration["Identity:Password:RequireDigit"].IsSet() ? bool.Parse(Configuration["Identity:Password:RequireDigit"]) : true;
                    o.Password.RequireLowercase = Configuration["Identity:Password:RequireLowercase"].IsSet() ? bool.Parse(Configuration["Identity:Password:RequireLowercase"]) : false;
                    o.Password.RequireUppercase = Configuration["Identity:Password:RequireUppercase"].IsSet() ? bool.Parse(Configuration["Identity:Password:RequireUppercase"]) : false;
                    o.Password.RequireNonAlphanumeric = Configuration["Identity:Password:RequireNonAlphanumeric"].IsSet() ? bool.Parse(Configuration["Identity:Password:RequireNonAlphanumeric"]) : true;
                    o.Password.RequiredLength = Configuration["Identity:Password:RequiredLength"].IsSet() ? int.Parse(Configuration["Identity:Password:RequiredLength"]) : 6;
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
                services.AddTransient<FormSenderService>();
                services.AddTransient<WelcomeEmailSender>();

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

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddApplicationInsightsTelemetry(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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

            if (Configuration.CheckSetup("Installed:DB"))
            {

                // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
                    var userManager = app.ApplicationServices.GetService<UserManager<ApplicationUser>>();
                    var roleManager = app.ApplicationServices.GetService<RoleManager<IdentityRole>>();
                    var options = new DbContextOptionsBuilder<HoodDbContext>();
                    options.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"]);
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

            if (Configuration.CheckSetup("Installed:DB"))
            {
                app.UseIdentity();
            }

            app.UseSession();

            if (!Configuration.CheckSetup("Installed"))
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
        }
    }
}
