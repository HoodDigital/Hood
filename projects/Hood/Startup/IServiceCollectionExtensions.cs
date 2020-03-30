using Hood.Caching;
using Hood.Core;
using Hood.Extensions;
using Hood.Filters;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Startup
{
    /// <summary>
    /// Represents extensions of IServiceCollection
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureHood<TContext>(this IServiceCollection services, IConfiguration config)
          where TContext : HoodDbContext
        {

            if (config.IsDatabaseConnected())
            {
                services.ConfigureHoodServices();
                services.ConfigureHoodDatabase<TContext>(config);
                services.ConfigureAuthentication(config);
            }

            services.ConfigureViewEngine(config);
            services.ConfigureHoodAntiForgery(config);
            services.ConfigureCookies(config);
            services.ConfigureSession(config);
            services.ConfigureCacheProfiles();
            services.ConfigureFilters();
            services.ConfigureImpersonation();
            services.ConfigureRoutes();

            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver())
                .AddApplicationPart(typeof(Engine).Assembly)
                .AddApplicationPart(typeof(IServiceCollectionExtensions).Assembly);

            services.AddRazorPages();

            return services;
        }

        public static IServiceCollection ConfigureHoodServices(this IServiceCollection services)
        {
            // Register singletons.
            services.AddSingleton<SubscriptionsEventListener>();
            services.AddSingleton<IFTPService, FTPService>();
            services.AddSingleton<IPropertyImporter, BlmFileImporter>();
            services.AddSingleton<IMediaRefreshService, MediaRefreshService>();
            services.AddSingleton<IThemesService, ThemesService>();
            services.AddSingleton<IAddressService, AddressService>();
            services.AddSingleton<IDirectoryManager, DirectoryManager>();
            services.AddSingleton<IMediaManager, MediaManager>();
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<IHoodCache, HoodCache>();
            services.AddSingleton<ContentCategoryCache>();
            services.AddSingleton<ContentByTypeCache>();
            services.AddSingleton<ForumCategoryCache>();

            // Register transients.
            services.AddTransient<IStripeService, StripeService>();

            // Register scoped.
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IContentRepository, ContentRepository>();

            services.AddScoped<IRazorViewRenderer, RazorViewRenderer>();
            services.AddScoped<IStripeWebHookService, StripeWebHookService>();
            services.AddScoped<IPageBuilder, PageBuilder>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<ISmsSender, SmsSender>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IRecaptchaService, RecaptchaService>();
            services.AddScoped<IPropertyExporter, PropertyExporter>();
            services.AddScoped<IContentExporter, ContentExporter>();

            return services;
        }

        /// <summary>
        /// Add required Hood services to the application and configure service provider.
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        /// <returns>Configured service provider</returns>
        public static IServiceProvider AddHood(this IServiceCollection services, IConfiguration configuration)
        {
            //add accessor to HttpContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //create, initialize and configure the engine
            var engine = Engine.CreateHoodServiceProvider();
            engine.Initialize(services);
            var serviceProvider = engine.ConfigureServices(services, configuration);

            return serviceProvider;
        }

        public static IServiceCollection ConfigureHoodDatabase<TContext>(this IServiceCollection services, IConfiguration config)
            where TContext : HoodDbContext
        {
            services.AddDbContext<TContext>(options => options.UseSqlServer(config["ConnectionStrings:DefaultConnection"]));
            services.AddDbContext<HoodDbContext>(options => options.UseSqlServer(config["ConnectionStrings:DefaultConnection"]));
            return services;
        }
        public static IServiceCollection ConfigureHoodAntiForgery(this IServiceCollection services, IConfiguration config)
        {
            var cookieName = config["Cookies:Name"].IsSet() ? config["Cookies:Name"] : "Hood";

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = $".{cookieName}.Antiforgery";
            });
            return services;
        }
        public static IServiceCollection ConfigureCookies(this IServiceCollection services, IConfiguration config)
        {
            var cookieName = config["Cookies:Name"].IsSet() ? config["Cookies:Name"] : "Hood";
            bool consentRequired = config.GetValue("Cookies:ConsentRequired", true);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => consentRequired;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.ConsentCookie.Name = $".{cookieName}.Consent";
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Cookie.Name = $".{cookieName}.Authentication";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(config.GetValue("Session:Timeout", 60));
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.ReturnUrlParameter = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });
            return services;
        }
        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration config)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(o =>
            {
                // configure identity options
                o.User.RequireUniqueEmail = true;

                o.SignIn.RequireConfirmedEmail = false;
                o.SignIn.RequireConfirmedPhoneNumber = false;

                o.Password.RequireDigit = config["Identity:Password:RequireDigit"].IsSet() ? bool.Parse(config["Identity:Password:RequireDigit"]) : true;
                o.Password.RequireLowercase = config["Identity:Password:RequireLowercase"].IsSet() ? bool.Parse(config["Identity:Password:RequireLowercase"]) : false;
                o.Password.RequireUppercase = config["Identity:Password:RequireUppercase"].IsSet() ? bool.Parse(config["Identity:Password:RequireUppercase"]) : false;
                o.Password.RequireNonAlphanumeric = config["Identity:Password:RequireNonAlphanumeric"].IsSet() ? bool.Parse(config["Identity:Password:RequireNonAlphanumeric"]) : true;
                o.Password.RequiredLength = config["Identity:Password:RequiredLength"].IsSet() ? int.Parse(config["Identity:Password:RequiredLength"]) : 6;
            })
            .AddEntityFrameworkStores<HoodDbContext>()
            .AddDefaultTokenProviders();

            var cookieName = config["Cookies:Name"].IsSet() ? config["Cookies:Name"] : "Hood";

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Cookie.Name = $".{cookieName}.Authentication";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(config.GetValue("Session:Timeout", 60));
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.ReturnUrlParameter = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });
            return services;
        }
        public static IServiceCollection ConfigureSession(this IServiceCollection services, IConfiguration config)
        {
            var cookieName = config["Cookies:Name"].IsSet() ? config["Cookies:Name"] : "Hood";

            int sessionTimeout = 60;
            services.AddSession(options =>
            {
                options.Cookie.Name = $".{cookieName}.Session";
                options.Cookie.HttpOnly = true;
                if (int.TryParse(config["Session:Timeout"], out sessionTimeout))
                    options.Cookie.Expiration = TimeSpan.FromMinutes(sessionTimeout);
                else
                    options.Cookie.Expiration = TimeSpan.FromMinutes(60);
            });
            return services;
        }
        public static IServiceCollection ConfigureCacheProfiles(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(options =>
            {
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

            return services;
        }
        public static IServiceCollection ConfigureFilters(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(typeof(UrlFilter));
                options.Filters.Add(typeof(LockoutModeFilter));
            });

            return services;
        }
        public static IServiceCollection ConfigureImpersonation(this IServiceCollection services)
        {
            services.Configure<SecurityStampValidatorOptions>(options => // different class name
            {
                options.ValidationInterval = TimeSpan.FromMinutes(1);  // new property name
                options.OnRefreshingPrincipal = context =>             // new property name
                {
                    var originalUserIdClaim = context.CurrentPrincipal.FindFirst("OriginalUserId");
                    var isImpersonatingClaim = context.CurrentPrincipal.FindFirst("IsImpersonating");
                    if (originalUserIdClaim != null && isImpersonatingClaim.Value == "true")
                    {
                        context.NewPrincipal.Identities.First().AddClaim(originalUserIdClaim);
                        context.NewPrincipal.Identities.First().AddClaim(isImpersonatingClaim);
                    }
                    return Task.FromResult(0);
                };
            });

            return services;
        }
        public static IServiceCollection ConfigureRoutes(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap.Add("propertySlug", typeof(PropertyRouteConstraint));
                options.ConstraintMap.Add("pageSlug", typeof(PagesRouteConstraint));
                options.ConstraintMap.Add("contentTypeSlug", typeof(ContentTypeRouteConstraint));
                options.LowercaseUrls = true;
            });
            return services;
        }
        public static IServiceCollection ConfigureViewEngine(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
            {
                options.FileProviders.Add(new EmbeddedFileProvider(typeof(Engine).Assembly, "ComponentLib"));
                options.FileProviders.Add(new EmbeddedFileProvider(typeof(IServiceCollectionExtensions).Assembly, "ComponentLib"));
                options.FileProviders.Add(UserInterfaceProvider.GetAdminProvider());
                options.FileProviders.Add(UserInterfaceProvider.GetAccountProvider());
                var defaultUI = UserInterfaceProvider.GetProvider(config);
                if (defaultUI != null)
                    options.FileProviders.Add(defaultUI);
            });
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });
            return services;
        }

    }
}
