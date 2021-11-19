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
using StackExchange.Redis;
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

            services.Configure<HoodConfiguration>(config.GetSection("Hood"));

            if (!config.IsDatabaseConnected())
            {
                throw new StartupException("No database connected.", StartupError.DatabaseConnectionFailed);
            }

            services.ConfigureHoodDatabase<TContext>(config);
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.ConfigureHoodServices();

            services.ConfigureCache(config);
            services.ConfigureAuthentication(config);

            services.ConfigureViewEngine(config);
            services.ConfigureHoodAntiForgery(config);

            services.AddDistributedMemoryCache();

            services.ConfigureCookies(config);
            services.ConfigureSession(config);
            services.ConfigureCacheProfiles();
            services.ConfigureFilters();
            services.ConfigureImpersonation();
            services.ConfigureRoutes();

            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver()
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    };
                })
                .AddApplicationPart(typeof(Engine).Assembly)
                .AddApplicationPart(typeof(IServiceCollectionExtensions).Assembly);

            services.AddRazorPages();

            services.ConfigureHoodEngine(config);

            return services;
        }

        public static IServiceCollection ConfigureHoodServices(this IServiceCollection services)
        {
            // Register singletons.
            services.AddSingleton<IFTPService, FTPService>();
            services.AddSingleton<IPropertyImporter, BlmFileImporter>();
            services.AddSingleton<IMediaRefreshService, MediaRefreshService>();
            services.AddSingleton<IThemesService, ThemesService>();
            services.AddSingleton<IAddressService, AddressService>();
            services.AddSingleton<IDirectoryManager, DirectoryManager>();
            services.AddSingleton<IMediaManager, MediaManager>();
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<ContentCategoryCache>();
            services.AddSingleton<ContentByTypeCache>();

            // Register scoped.
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IContentRepository, ContentRepository>();

            services.AddScoped<IRazorViewRenderer, RazorViewRenderer>();
            services.AddScoped<IPageBuilder, PageBuilder>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<ISmsSender, SmsSender>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IRecaptchaService, RecaptchaService>();

            return services;
        }

        public static IServiceCollection ConfigureCache(this IServiceCollection services, IConfiguration config)
        {
            // Caching
            //if (config["ConnectionStrings:RedisCache"].IsSet())
            //{
            //    services.AddSingleton<IConnectionMultiplexer>(x => ConnectionMultiplexer.Connect(config.GetValue<string>("ConnectionStrings:RedisCache")));
            //    services.AddSingleton<IHoodCache, HoodRedisCache>();
            //}
            //else
            //{
            //    services.AddSingleton<IHoodCache, HoodCache>();
            //}
            services.AddSingleton<IHoodCache, HoodCache>();

            return services;
        }

        /// <summary>
        /// Add required Hood services to the application and configure service provider.
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        /// <returns>Configured service provider</returns>
        public static IServiceProvider ConfigureHoodEngine(this IServiceCollection services, IConfiguration configuration)
        {
            //add accessor to HttpContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //create, initialize and configure the engine
            IHoodServiceProvider engine = Engine.CreateHoodServiceProvider();
            engine.Initialize(services);
            IServiceProvider serviceProvider = engine.ConfigureServices(services, configuration);

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
            string cookieName = config["Cookies:Name"].IsSet() ? config["Cookies:Name"] : "Hood";

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = $".{cookieName}.Antiforgery";
            });
            return services;
        }
        public static IServiceCollection ConfigureCookies(this IServiceCollection services, IConfiguration config)
        {
            string cookieName = config["Cookies:Name"].IsSet() ? config["Cookies:Name"] : "Hood";
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
                options.Cookie.Name = $".{cookieName}.Authentication";
                options.Cookie.HttpOnly = true;    
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.AccessDeniedPath = config["Identity:AccessDeniedPath"].IsSet() ? config["Identity:AccessDeniedPath"] : "/account/access-denied";

                options.ExpireTimeSpan = TimeSpan.FromMinutes(config.GetValue("Session:Timeout", 60));
                options.LoginPath = config["Identity:LoginPath"].IsSet() ? config["Identity:LoginPath"] : "/account/login";
                options.LogoutPath = config["Identity:LogoutPath"].IsSet() ? config["Identity:LogoutPath"] : "/account/logout";
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

                o.Password.RequireDigit = !config["Identity:Password:RequireDigit"].IsSet() || bool.Parse(config["Identity:Password:RequireDigit"]);
                o.Password.RequireLowercase = config["Identity:Password:RequireLowercase"].IsSet() && bool.Parse(config["Identity:Password:RequireLowercase"]);
                o.Password.RequireUppercase = config["Identity:Password:RequireUppercase"].IsSet() && bool.Parse(config["Identity:Password:RequireUppercase"]);
                o.Password.RequireNonAlphanumeric = !config["Identity:Password:RequireNonAlphanumeric"].IsSet() || bool.Parse(config["Identity:Password:RequireNonAlphanumeric"]);
                o.Password.RequiredLength = config["Identity:Password:RequiredLength"].IsSet() ? int.Parse(config["Identity:Password:RequiredLength"]) : 6;
            })
            .AddEntityFrameworkStores<HoodDbContext>()
            .AddDefaultTokenProviders()
            .AddMagicLoginTokenProvider();

            string cookieName = config["Cookies:Name"].IsSet() ? config["Cookies:Name"] : "Hood";

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = $".{cookieName}.Authentication";
                options.Cookie.HttpOnly = true;    
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.AccessDeniedPath = config["Identity:AccessDeniedPath"].IsSet() ? config["Identity:AccessDeniedPath"] : "/account/access-denied";

                options.ExpireTimeSpan = TimeSpan.FromMinutes(config.GetValue("Session:Timeout", 60));
                options.LoginPath = config["Identity:LoginPath"].IsSet() ? config["Identity:LoginPath"] : "/account/login";
                options.LogoutPath = config["Identity:LogoutPath"].IsSet() ? config["Identity:LogoutPath"] : "/account/logout";
                options.ReturnUrlParameter = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;   
            });
            return services;
        }
        public static IServiceCollection ConfigureSession(this IServiceCollection services, IConfiguration config)
        {
            string cookieName = config["Cookies:Name"].IsSet() ? config["Cookies:Name"] : "Hood";

            int sessionTimeout = 60;
            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.Name = $".{cookieName}.Session";
                options.Cookie.HttpOnly = true;
                if (int.TryParse(config["Session:Timeout"], out sessionTimeout))
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(sessionTimeout);
                    options.Cookie.Expiration = TimeSpan.FromMinutes(sessionTimeout);
                }
                else
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(60);
                    options.Cookie.Expiration = TimeSpan.FromMinutes(60);
                }
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
                    System.Security.Claims.Claim originalUserIdClaim = context.CurrentPrincipal.FindFirst("OriginalUserId");
                    System.Security.Claims.Claim isImpersonatingClaim = context.CurrentPrincipal.FindFirst("IsImpersonating");
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
                if (Engine.Services.Installed)
                {
                    EmbeddedFileProvider defaultUI = UserInterfaceProvider.GetProvider(config);
                    if (defaultUI != null)
                    {
                        options.FileProviders.Add(defaultUI);
                    }
                }
            });
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });
            return services;
        }

    }
}
