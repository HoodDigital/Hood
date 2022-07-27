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
            services.Configure<Auth0Configuration>(config.GetSection("Identity:Auth0"));

            services.ConfigureHoodServices();

            try
            {
                if (!config.IsDatabaseConnected())
                {
                    throw new StartupException("Database connection string is not configured.", StartupError.NoConnectionString);
                }
                services.ConfigureHoodDatabase<TContext>(config);
                services.ConfigureHoodDatabaseDependentServices();
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

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.ConfigureCache(config);

            services.ConfigureViewEngine(config);
            services.ConfigureAntiForgery(config);

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

            services.ConfigureEngine(config);

            // TODO: #37 Redo logging service so it logs to app insights, repurpose db logs for internal messaging.             
            services.AddSingleton<ILogService, LogService>();

            return services;
        }

        public static IServiceCollection ConfigureHoodServices(this IServiceCollection services)
        {

            // Register singletons.
            services.AddSingleton<IFTPService, FTPService>();
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
            services.AddSingleton<IPropertyImporter, BlmFileImporter>();
            services.AddSingleton<IDirectoryManager, DirectoryManager>();
            services.AddSingleton<IMediaManager, MediaManager>();
            services.AddSingleton<ContentCategoryCache>();
            services.AddSingleton<ContentByTypeCache>();

            // Register scoped.
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IContentRepository, ContentRepository>();

            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<ISmsSender, SmsSender>();

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
        public static IServiceProvider ConfigureEngine(this IServiceCollection services, IConfiguration configuration)
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
            services.AddIdentity<ApplicationUser, ApplicationRole>(o =>
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
                        var repo = Engine.Services.Resolve<IAccountRepository>();
                        var user = await repo.GetUserByIdAsync(e.Principal.GetUserId());
                        e.Principal.SetUserClaims(user);
                        if (user.EmailConfirmed)
                        {
                            e.Principal.AddOrUpdateClaimValue(HoodClaimTypes.EmailConfirmed, "true");
                        }
                        if (user.Active || !Engine.Settings.Account.RequireEmailConfirmation)
                        {
                            e.Principal.AddOrUpdateClaimValue(HoodClaimTypes.Active, "true");
                        }
                    }
                };
            });

            services.AddScoped<IAccountRepository, AccountRepository>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.Active, policy => policy.RequireClaim(Identity.HoodClaimTypes.Active));
                options.AddPolicy(Policies.AccountNotConnected, policy => policy.RequireClaim(Identity.HoodClaimTypes.AccountNotConnected));
                options.AddPolicy(Policies.AccountLinkRequired, policy => policy.RequireClaim(Identity.HoodClaimTypes.AccountLinkRequired));
            });

            return services;
        }

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

            services.AddAuth0IdentityStores();

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

        public static void AddAuth0IdentityStores(this IServiceCollection services)
        {
            // Mock Aspnet Identity stores.
            services.TryAddScoped<IUserValidator<ApplicationUser>, UserValidator<ApplicationUser>>();
            services.TryAddScoped<IPasswordValidator<ApplicationUser>, PasswordValidator<ApplicationUser>>();
            services.TryAddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            services.TryAddScoped<IdentityErrorDescriber>();
            //services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<ApplicationUser>>();

            var identityBuilder = new IdentityBuilder(typeof(ApplicationUser), services);

            identityBuilder.AddRoles<ApplicationRole>();
            identityBuilder.AddRoleStore<RoleStore<ApplicationRole, HoodDbContext, string>>();
            identityBuilder.AddUserStore<UserStore<ApplicationUser, ApplicationRole, HoodDbContext, string>>();
            identityBuilder.AddUserManager<UserManager<ApplicationUser>>();
            identityBuilder.AddRoleManager<RoleManager<ApplicationRole>>();

            services.AddScoped<IAccountRepository, Auth0AccountRepository>();
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
        public static IServiceCollection ConfigureSession(this IServiceCollection services, IConfiguration config)
        {

            string cookieName = config["Identity:Cookies:Name"].IsSet() ? config["Identity:Cookies:Name"] : Constants.CookieDefaultName;
            services.Configure<CookieTempDataProviderOptions>(options => {
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
        private static IServiceCollection AddStores(this IServiceCollection services)
        {
            Type userType = typeof(ApplicationUser);
            Type roleType = typeof(ApplicationRole);
            Type contextType = typeof(HoodDbContext);
            Type keyType = typeof(string);

            Type userStoreType;
            Type roleStoreType;
            var identityContext = typeof(HoodDbContext);

            userStoreType = typeof(UserStore<ApplicationUser, ApplicationRole, HoodDbContext, string>);
            roleStoreType = typeof(RoleStore<ApplicationRole, HoodDbContext, string>);


            services.TryAddScoped(typeof(IRoleValidator<ApplicationRole>), typeof(RoleValidator<ApplicationRole>));
            services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
            services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), roleStoreType);
            services.TryAddScoped<RoleManager<ApplicationRole>>();
            services.AddScoped(typeof(IUserClaimsPrincipalFactory<>).MakeGenericType(userType), typeof(UserClaimsPrincipalFactory<,>).MakeGenericType(userType, roleType));

            services.AddScoped(typeof(UserManager<>).MakeGenericType(userType), typeof(UserManager<ApplicationUser>));

            return services;
        }
    }
}
