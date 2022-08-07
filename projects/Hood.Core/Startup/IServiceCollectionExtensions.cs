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
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Hood.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Hood.Enums;
using Hood.Contexts;
using Microsoft.AspNetCore.Hosting;

namespace Hood.Startup
{
    /// <summary>
    /// Represents extensions of IServiceCollection
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureHood(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {

            services.Configure<HoodConfiguration>(config.GetSection("Hood"));
            services.Configure<Auth0Configuration>(config.GetSection("Identity:Auth0"));

            services.ConfigureHoodCoreServices();

            try
            {
                if (!config.IsDatabaseConnected())
                {
                    throw new StartupException("Database connection string is not configured.", StartupError.NoConnectionString);
                }
                services.ConfigureHoodDatabase(config);       

                services.ConfigureHoodDatabaseDependentServices();     

                services.ConfigureProperty(config);
                services.ConfigureContent(config);

                if (config.IsConfigured("Identity:Auth0:Domain") && config.IsConfigured("Identity:Auth0:ClientId"))
                {
                    services.ConfigureAuth0(config, new HoodAuth0Options(config));
                }
                else
                {
                    services.ConfigureAuthentication(config);
                }

            }
            catch (StartupException) { }

            if (env.EnvironmentName == "Development" || env.EnvironmentName == "Hood")
            {
                services.AddDatabaseDeveloperPageExceptionFilter();
            }


            services.ConfigureCache(config);
            services.ConfigureCacheProfiles();

            services.ConfigureViewEngine(config);
            services.ConfigureAntiForgery(config);

            services.AddDistributedMemoryCache();

            services.ConfigureCookies(config);
            services.ConfigureSession(config);

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

            services.ConfigureEngine(config);


            return services;
        }

        public static IServiceCollection ConfigureHoodCoreServices(this IServiceCollection services)
        {
            // Register singletons.
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<IThemesService, ThemesService>();
            services.AddSingleton<IAddressService, AddressService>();

            // Register scoped.
            services.AddScoped<IRazorViewRenderer, RazorViewRenderer>();
            services.AddScoped<IPageBuilder, PageBuilder>();
            services.AddScoped<IRecaptchaService, RecaptchaService>();

            return services;
        }
        public static IServiceCollection ConfigureHoodDatabaseDependentServices(this IServiceCollection services)
        {

            // Register singletons.
            services.AddSingleton<IDirectoryManager, DirectoryManager>();
            services.AddSingleton<IMediaManager, MediaManager>();

            // Register scoped.
            services.AddScoped<ISettingsRepository, SettingsRepository>();

            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<ISmsSender, SmsSender>();

            return services;
        }

        #region Caching

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

        #endregion

        public static IServiceCollection ConfigureEngine(this IServiceCollection services, IConfiguration configuration)
        {
            //add accessor to HttpContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //create, initialize and configure the engine
            IHoodServiceProvider engine = Engine.CreateHoodServiceProvider();
            engine.Initialize(services);
            IServiceProvider serviceProvider = engine.ConfigureServices(services, configuration);

            return services;
        }

        #region Contexts

        public static IServiceCollection ConfigureHoodDatabase(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<HoodDbContext>(options => options.UseSqlServer(config["ConnectionStrings:DefaultConnection"]));
            return services;
        }
        public static IServiceCollection ConfigureProperty(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<PropertyContext>(options => options.UseSqlServer(config["ConnectionStrings:DefaultConnection"]));
            services.AddSingleton<IFTPService, FTPService>();
            services.AddSingleton<IPropertyImporter, BlmFileImporter>();
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            return services;
        }
        public static IServiceCollection ConfigureContent(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<ContentContext>(options => options.UseSqlServer(config["ConnectionStrings:DefaultConnection"]));
            services.AddSingleton<ContentCategoryCache>();
            services.AddSingleton<ContentByTypeCache>();
            services.AddScoped<IContentRepository, ContentRepository>();
            return services;
        }

        #endregion

        #region Password Authentication
        public static IServiceCollection ConfigureAntiForgery(this IServiceCollection services, IConfiguration config)
        {
            string cookieName = config["Identity:Cookies:Name"].IsSet() ? config["Identity:Cookies:Name"] : Constants.CookieDefaultName;
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = $"{cookieName}_af";
                options.Cookie.Domain = config["Identity:Cookies:Domain"].IsSet() ? config["Identity:Cookies:Domain"] : null;
            });
            return services;
        }
        public static IServiceCollection ConfigureCookies(this IServiceCollection services, IConfiguration config)
        {
            string cookieName = config["Identity:Cookies:Name"].IsSet() ? config["Identity:Cookies:Name"] : Constants.CookieDefaultName;
            bool consentRequired = config.GetValue("Identity:Cookies:ConsentRequired", true);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => consentRequired;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.ConsentCookie.Name = $"{cookieName}_consent";
                options.ConsentCookie.Domain = config["Identity:Cookies:Domain"].IsSet() ? config["Identity:Cookies:Domain"] : null;
            });
            return services;
        }
        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration config)
        {
            return services;
        }

        #endregion

        #region Auth0 Authentication

        public static IServiceCollection ConfigureAuth0(this IServiceCollection services, IConfiguration config, IHoodAuth0Options auth0Options)
        {

            
            services.ConfigureSameSiteNoneCookies();

            services.AddAuth0WebAppAuthentication(options =>
            {
                options.Backchannel = auth0Options.Backchannel;
                options.CallbackPath = auth0Options.CallbackPath;
                options.ClientId = auth0Options.ClientId;
                options.ClientSecret = auth0Options.ClientSecret;
                options.Domain = auth0Options.Domain;
                options.LoginParameters = auth0Options.LoginParameters;
                options.MaxAge = auth0Options.MaxAge;
                options.OpenIdConnectEvents = auth0Options.AsOpenIdConnectEvents();
                options.Organization = auth0Options.Organization;
                options.ResponseType = auth0Options.ResponseType;
                options.Scope = auth0Options.Scope;
            });

            services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
                .Configure(options =>
                {
                    SetAuthenticationCookieDefaults(config, options);
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.Active, policy => policy.RequireClaim(Identity.HoodClaimTypes.Active));
                options.AddPolicy(Policies.AccountNotConnected, policy => policy.RequireClaim(Identity.HoodClaimTypes.AccountNotConnected));
            });
            return services;

        }
        private static void SetAuthenticationCookieDefaults(IConfiguration config, CookieAuthenticationOptions options)
        {
            string cookieName = config["Identity:Cookies:Name"].IsSet() ? config["Identity:Cookies:Name"] : Constants.CookieDefaultName;

            options.Cookie.Name = $"{cookieName}_auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.Domain = config["Identity:Cookies:Domain"].IsSet() ? config["Identity:Cookies:Domain"] : null;

            options.AccessDeniedPath = config["Identity:AccessDeniedPath"].IsSet() ? config["Identity:AccessDeniedPath"] : "/account/access-denied";
            options.LoginPath = config["Identity:LoginPath"].IsSet() ? config["Identity:LoginPath"] : "/account/login";
            options.LogoutPath = config["Identity:LogoutPath"].IsSet() ? config["Identity:LogoutPath"] : "/account/logout";
            options.ReturnUrlParameter = Constants.ReturnUrlParameter;
        }
        public static IServiceCollection ConfigureSameSiteNoneCookies(this IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext => CheckSameSite(cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext => CheckSameSite(cookieContext.CookieOptions);
            });
            return services;
        }
        private static void CheckSameSite(CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None && options.Secure == false)
            {
                options.SameSite = SameSiteMode.Unspecified;
            }
        }

        #endregion

        #region Session

        public static IServiceCollection ConfigureSession(this IServiceCollection services, IConfiguration config)
        {
            string cookieName = config["Identity:Cookies:Name"].IsSet() ? config["Identity:Cookies:Name"] : Constants.CookieDefaultName;
            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.Name = $"{cookieName}_td";
                options.Cookie.HttpOnly = true;
                options.Cookie.Domain = config["Identity:Cookies:Domain"].IsSet() ? config["Identity:Cookies:Domain"] : null;
            });

            int sessionTimeout = 60;
            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.Name = $"{cookieName}_session";
                options.Cookie.HttpOnly = true;
                options.Cookie.Domain = config["Identity:Cookies:Domain"].IsSet() ? config["Identity:Cookies:Domain"] : null;

                if (int.TryParse(config["Session:Timeout"], out sessionTimeout))
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(sessionTimeout);
                }
                else
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(60);
                }
            });
            return services;
        }

        #endregion

        public static IServiceCollection ConfigureImpersonation(this IServiceCollection services)
        {
            services.Configure<SecurityStampValidatorOptions>(options => // different class name
            {
                options.ValidationInterval = TimeSpan.FromMinutes(1);  // new property name
                options.OnRefreshingPrincipal = context =>             // new property name
                {
                    System.Security.Claims.Claim originalUserIdClaim = context.CurrentPrincipal.FindFirst(HoodClaimTypes.OriginalUserId);
                    System.Security.Claims.Claim isImpersonatingClaim = context.CurrentPrincipal.FindFirst(HoodClaimTypes.IsImpersonating);
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
