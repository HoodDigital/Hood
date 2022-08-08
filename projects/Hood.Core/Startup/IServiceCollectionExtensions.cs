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
using Hood.Constants.Identity;

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
                    services.ConfigureAuth0(config, new Auth0LoginService(config));
                }
                else
                {
                    services.ConfigurePasswordAuthentication(config);
                }

            }
            catch (StartupException) { }

            if (env.EnvironmentName == "Development" || env.EnvironmentName == "Hood")
            {
                services.AddDatabaseDeveloperPageExceptionFilter();
            }

            services.ConfigureCache(config);
            services.ConfigureCacheProfiles();
            services.AddDistributedMemoryCache();

            services.ConfigureViewEngine(config);

            services.ConfigureAntiForgery(config);

            services.ConfigureCookieConsent(config);

            services.ConfigureSession(config);

            services.ConfigureHoodSlugRouteConstraints();

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

        public static IServiceCollection ConfigureHoodEngine(this IServiceCollection services, IConfiguration configuration)
        {
            //add accessor to HttpContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //create, initialize and configure the engine
            IHoodServiceProvider engine = Engine.CreateHoodServiceProvider();
            engine.Initialize(services);
            IServiceProvider serviceProvider = engine.ConfigureServices(services, configuration);

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

        #region Anti Forgery

        public static IServiceCollection ConfigureAntiForgery(this IServiceCollection services, IConfiguration config)
        {
            string cookieName = config["Identity:Cookies:Name"].IsSet() ? config["Identity:Cookies:Name"] : Authentication.CookieDefaultName;
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = $"{cookieName}_af";
                options.Cookie.Domain = config["Identity:Cookies:Domain"].IsSet() ? config["Identity:Cookies:Domain"] : null;
            });
            return services;
        }

        #endregion

        #region Cookie Consent

        public static IServiceCollection ConfigureCookieConsent(this IServiceCollection services, IConfiguration config)
        {
            string cookieName = config["Identity:Cookies:Name"].IsSet() ? config["Identity:Cookies:Name"] : Authentication.CookieDefaultName;
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

        #endregion

        #region Password Authentication

        public static IServiceCollection ConfigurePasswordAuthentication(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<IdentityContext>(options => options.UseSqlServer(config["ConnectionStrings:DefaultConnection"]));
            services.AddScoped<IPasswordAccountRepository, AccountRepository>();
            services.AddScoped<IHoodAccountRepository, AccountRepository>();

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
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                SetAuthenticationCookieDefaults(config, options);

                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.Domain = config["Identity:Cookies:Domain"].IsSet() ? config["Identity:Cookies:Domain"] : null;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(config.GetValue("Session:Timeout", 60));
                options.SlidingExpiration = true;

                options.Events = new CookieAuthenticationEvents()
                {
                    OnValidatePrincipal = async e =>
                    {
                        // get the user profile and store important bits on the claim.
                        var repo = Engine.Services.Resolve<IPasswordAccountRepository>();
                        var user = await repo.GetUserByIdAsync(e.Principal.GetUserId());
                        e.Principal.SetUserClaims(user.UserProfile);
                        if (user.EmailConfirmed)
                        {
                            e.Principal.AddOrUpdateClaimValue(Hood.Constants.Identity.ClaimTypes.EmailConfirmed, "true");
                        }
                        if (user.Active || !Engine.Settings.Account.RequireEmailConfirmation)
                        {
                            e.Principal.AddOrUpdateClaimValue(Hood.Constants.Identity.ClaimTypes.Active, "true");
                        }
                    }
                };
            });


            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.Active, policy => policy.RequireClaim(Hood.Constants.Identity.ClaimTypes.Active));
                options.AddPolicy(Policies.AccountNotConnected, policy => policy.RequireClaim(Hood.Constants.Identity.ClaimTypes.AccountNotConnected));
                options.AddPolicy(Policies.AccountLinkRequired, policy => policy.RequireClaim(Hood.Constants.Identity.ClaimTypes.AccountLinkRequired));
            });

            services.ConfigurePasswordImpersonation();

            return services;
        }

        public static IServiceCollection ConfigurePasswordImpersonation(this IServiceCollection services)
        {
            services.Configure<SecurityStampValidatorOptions>(options => // different class name
            {
                options.ValidationInterval = TimeSpan.FromMinutes(1);  // new property name
                options.OnRefreshingPrincipal = context =>             // new property name
                {
                    System.Security.Claims.Claim originalUserIdClaim = context.CurrentPrincipal.FindFirst(Hood.Constants.Identity.ClaimTypes.OriginalUserId);
                    System.Security.Claims.Claim isImpersonatingClaim = context.CurrentPrincipal.FindFirst(Hood.Constants.Identity.ClaimTypes.IsImpersonating);
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

        #endregion

        #region Auth0 Authentication

        public static IServiceCollection ConfigureAuth0(this IServiceCollection services, IConfiguration config, IAuth0LoginService auth0Options)
        {
            services.AddDbContext<Auth0IdentityContext>(options => options.UseSqlServer(config["ConnectionStrings:DefaultConnection"]));
            services.AddSingleton<IAuth0Service, Auth0Service>();
            services.AddScoped<IAuth0AccountRepository, Auth0AccountRepository>();
            services.AddScoped<IHoodAccountRepository, Auth0AccountRepository>();

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
                options.AddPolicy(Policies.Active, policy => policy.RequireClaim(Hood.Constants.Identity.ClaimTypes.Active));
                options.AddPolicy(Policies.AccountNotConnected, policy => policy.RequireClaim(Hood.Constants.Identity.ClaimTypes.AccountNotConnected));
            });
            return services;

        }
        private static void SetAuthenticationCookieDefaults(IConfiguration config, CookieAuthenticationOptions options)
        {
            string cookieName = config["Identity:Cookies:Name"].IsSet() ? config["Identity:Cookies:Name"] : Authentication.CookieDefaultName;

            options.Cookie.Name = $"{cookieName}_auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.Domain = config["Identity:Cookies:Domain"].IsSet() ? config["Identity:Cookies:Domain"] : null;

            options.AccessDeniedPath = config["Identity:AccessDeniedPath"].IsSet() ? config["Identity:AccessDeniedPath"] : "/account/access-denied";
            options.LoginPath = config["Identity:LoginPath"].IsSet() ? config["Identity:LoginPath"] : "/account/login";
            options.LogoutPath = config["Identity:LogoutPath"].IsSet() ? config["Identity:LogoutPath"] : "/account/logout";
            options.ReturnUrlParameter = Authentication.ReturnUrlParameter;
        }
        private static IServiceCollection ConfigureSameSiteNoneCookies(this IServiceCollection services)
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
            string cookieName = config["Identity:Cookies:Name"].IsSet() ? config["Identity:Cookies:Name"] : Authentication.CookieDefaultName;
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

        #region RouteConstraints

        public static IServiceCollection ConfigureHoodSlugRouteConstraints(this IServiceCollection services)
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

        #endregion

        #region View Engine (File Providers & Theme)

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

        #endregion
    }
}
