using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Auth0.AspNetCore.Authentication;
using Hood.Caching;
using Hood.Constants.Identity;
using Hood.Contexts;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Identity;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Hood.Startup
{
    /// <summary>
    /// Represents extensions of IServiceCollection
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureHood(
            this IServiceCollection services,
            IConfiguration config,
            IWebHostEnvironment env
        )
        {
            try
            {
                // Register core stuff.
                services.ConfigureHoodBasics(config);
                services.ConfigureHoodSite(config, env);
                services.ConfigureHoodEngine(config);
            }
            catch (StartupException) { }
            return services;
        }

        public static IServiceCollection ConfigureHoodApi(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            try
            {
                services.ConfigureHoodCore(config);

                services.AddCors(options =>
                {
                    string[] domains = config.GetSection("Cors:Domains").Get<string[]>();
                    options.AddDefaultPolicy(builder =>
                    {
                        builder.WithOrigins(domains);
                    });
                });

                services
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = $"https://{config["Identity:Auth0:Domain"]}/";
                        options.Audience = config["Identity:Auth0:Audience"];

                        options.Events = new JwtBearerEvents
                        {
                            OnChallenge = context =>
                            {
                                context.Response.OnStarting(async () =>
                                {
                                    await context.Response.WriteAsync(
                                        JsonConvert.SerializeObject(
                                            new ApiResponse("You are not authorized!")
                                        )
                                    );
                                });
                                return Task.CompletedTask;
                            },
                        };
                    });

                services.AddAuthorization(options =>
                {
                    options.AddPolicy(
                        "Admin",
                        policy =>
                            policy.RequireAssertion(context =>
                                context.User.HasClaim(c =>
                                    (c.Type == "permissions" && c.Value == "read:admin-messages")
                                    && c.Issuer == $"https://{config["Identity:Auth0:Domain"]}/"
                                )
                            )
                    );
                });

                services.AddControllers().AddNewtonsoftJson();
            }
            catch (StartupException) { }
            return services;
        }

        public static IServiceCollection ConfigureHoodCore(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            try
            {
                // Register core stuff.
                services.ConfigureHoodBasics(config);
                services.ConfigureHoodEngine(config);
            }
            catch (StartupException) { }
            return services;
        }

        private static IServiceCollection ConfigureHoodBasics(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            services.Configure<HoodConfiguration>(config.GetSection("Hood"));
            services.Configure<Auth0Configuration>(config.GetSection("Identity:Auth0"));

            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<ITemplateProvider, TemplateProvider>();
            services.AddSingleton<IAddressService, AddressService>();

            services.ConfigureHoodDatabase(config);
            services.ConfigureHoodDatabaseDependentServices();

            services.ConfigureProperty(config);
            services.ConfigureContent(config);

            services.ConfigureCache();
            services.ConfigureCacheProfiles();

            services.ConfigureDataProtection(config);

            return services;
        }

        // Persist the Data Protection key ring to a stable location when Hood:DataProtectionKeyPath is set,
        // so antiforgery tokens / auth cookies survive app restarts, container rebuilds and multi-instance
        // hosting. Without it the keys default to an ephemeral per-process/container location and reset on
        // every rebuild (causing "The antiforgery token could not be decrypted"). The path is left unset by
        // default — single-host installs work fine on the default key ring; containers/farms set the path.
        private static IServiceCollection ConfigureDataProtection(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            IDataProtectionBuilder dataProtection = services
                .AddDataProtection()
                .SetApplicationName("Hood");

            string keyPath = config["Hood:DataProtectionKeyPath"];
            if (keyPath.IsSet())
            {
                Directory.CreateDirectory(keyPath);
                dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keyPath));
            }

            return services;
        }

        private static IServiceCollection ConfigureHoodSite(
            this IServiceCollection services,
            IConfiguration config,
            IWebHostEnvironment env
        )
        {
            // Register site stuff.
            services.AddSingleton<IThemesService, ThemesService>();
            services.AddScoped<IRazorViewRenderer, RazorViewRenderer>();
            services.AddScoped<IPageBuilder, PageBuilder>();
            services.AddScoped<IRecaptchaService, RecaptchaService>();

            if (!config.IsDatabaseConnected())
            {
                throw new StartupException(
                    "Database connection string is not configured.",
                    StartupError.NoConnectionString
                );
            }

            // The entire Identity:Auth0 section is optional — omit it and Hood falls back to the
            // standard ASP.NET Identity/password backend below.
            if (
                config.IsConfigured("Identity:Auth0:Domain")
                && config.IsConfigured("Identity:Auth0:ClientId")
            )
            {
                services.ConfigureAuth0(config, new Auth0LoginService(config));
            }
            else
            {
                services.ConfigurePasswordAuthentication(config);
            }

            if (env.EnvironmentName == "Development" || env.EnvironmentName == "Hood")
            {
                services.AddDatabaseDeveloperPageExceptionFilter();
            }

            services.ConfigureViewEngine(config, env);

            services.ConfigureAntiForgery(config);

            services.ConfigureCookieConsent(config);

            services.ConfigureSession(config);

            services.ConfigureHoodSlugRouteConstraints();

            IMvcBuilder mvcBuilder = services
                .AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver()
                    {
                        NamingStrategy = new CamelCaseNamingStrategy(),
                    };
                })
                .AddApplicationPart(typeof(Engine).Assembly)
                .AddApplicationPart(typeof(IServiceCollectionExtensions).Assembly);

            // Views ship precompiled (RCL); runtime compilation is a dev-loop tool.
            // Hood:AllowRuntimeViewCompilation restores live server-side view editing for
            // consumers who depend on it.
            if (
                env.EnvironmentName == "Development"
                || config.GetValue<bool>("Hood:AllowRuntimeViewCompilation")
            )
            {
                mvcBuilder.AddRazorRuntimeCompilation();
            }

            // Only the active UI flavour's compiled views participate in view resolution;
            // switching flavour requires an app restart.
            mvcBuilder.ConfigureApplicationPartManager(partManager =>
                UserInterfaceProvider.FilterInactiveUI(partManager, config, env)
            );

            services.AddRazorPages();
            return services;
        }

        public static IServiceCollection ConfigureHoodDatabaseDependentServices(
            this IServiceCollection services
        )
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

        public static IServiceCollection ConfigureHoodEngine(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            //add accessor to HttpContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //create, initialize and configure the engine
            IHoodServiceProvider engine = Engine.CreateHoodServiceProvider();
            engine.Initialize(services);
            engine.ConfigureServices(services, configuration);

            return services;
        }

        #region Caching

        public static IServiceCollection ConfigureCache(this IServiceCollection services)
        {
            // Caching
            //if (config["ConnectionStrings:RedisCache"].IsSet())
            //{
            //    services.AddDistributedMemoryCache();
            //    services.AddSingleton<IConnectionMultiplexer>(x => ConnectionMultiplexer.Connect(config.GetValue<string>("ConnectionStrings:RedisCache")));
            //    services.AddSingleton<IHoodCache, HoodRedisCache>();
            //}
            //else
            //{
            //    services.AddSingleton<IHoodCache, HoodCache>();
            //}
            services.AddMemoryCache();
            services.AddSingleton<IHoodCache, HoodCache>();
            return services;
        }

        public static IServiceCollection ConfigureCacheProfiles(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(options =>
            {
                options.CacheProfiles.Add(
                    "Year",
                    new CacheProfile
                    {
                        Location = ResponseCacheLocation.Client,
                        Duration = 31536000,
                    }
                );
                options.CacheProfiles.Add(
                    "Month",
                    new CacheProfile { Location = ResponseCacheLocation.Client, Duration = 2629000 }
                );
                options.CacheProfiles.Add(
                    "Week",
                    new CacheProfile { Location = ResponseCacheLocation.Client, Duration = 604800 }
                );
                options.CacheProfiles.Add(
                    "Day",
                    new CacheProfile { Location = ResponseCacheLocation.Client, Duration = 86400 }
                );
                options.CacheProfiles.Add(
                    "Hour",
                    new CacheProfile { Location = ResponseCacheLocation.Client, Duration = 3600 }
                );
                options.CacheProfiles.Add(
                    "HalfHour",
                    new CacheProfile { Location = ResponseCacheLocation.Client, Duration = 1800 }
                );
                options.CacheProfiles.Add(
                    "TenMinutes",
                    new CacheProfile { Location = ResponseCacheLocation.Client, Duration = 600 }
                );
            });

            return services;
        }

        #endregion

        #region Contexts

        public static IServiceCollection ConfigureHoodDatabase(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            services.AddDbContext<HoodDbContext>(options =>
                options.UseSqlServer(config["ConnectionStrings:DefaultConnection"])
            );
            return services;
        }

        public static IServiceCollection ConfigureProperty(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            // PropertyListingView reads Include several collections (Media/FloorPlans/Metadata); split
            // those into separate queries to avoid the cartesian-explosion single-query (EF Query[20504]).
            services.AddDbContext<PropertyContext>(options =>
                options.UseSqlServer(
                    config["ConnectionStrings:DefaultConnection"],
                    sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                )
            );
            services.AddSingleton<IFTPService, FTPService>();
            services.AddSingleton<IPropertyImporter, BlmFileImporter>();
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            return services;
        }

        public static IServiceCollection ConfigureContent(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            // ContentView reads Include several collections (Media/Metadata/Categories); split those into
            // separate queries to avoid the cartesian-explosion single-query (EF Query[20504]).
            services.AddDbContext<ContentContext>(options =>
                options.UseSqlServer(
                    config["ConnectionStrings:DefaultConnection"],
                    sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                )
            );
            services.AddSingleton<ContentCategoryCache>();
            services.AddSingleton<ContentByTypeCache>();
            services.AddScoped<IContentRepository, ContentRepository>();
            return services;
        }

        #endregion

        #region Anti Forgery

        public static IServiceCollection ConfigureAntiForgery(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            string cookieName = config["Identity:Cookies:Name"].IsSet()
                ? config["Identity:Cookies:Name"]
                : Authentication.CookieDefaultName;
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = $"{cookieName}_af";
                options.Cookie.Domain = config["Identity:Cookies:Domain"].IsSet()
                    ? config["Identity:Cookies:Domain"]
                    : null;
            });
            return services;
        }

        #endregion

        #region Cookie Consent

        public static IServiceCollection ConfigureCookieConsent(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            string cookieName = config["Identity:Cookies:Name"].IsSet()
                ? config["Identity:Cookies:Name"]
                : Authentication.CookieDefaultName;
            bool consentRequired = config.GetValue("Identity:Cookies:ConsentRequired", true);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = _ => consentRequired;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.ConsentCookie.Name = $"{cookieName}_consent";
                options.ConsentCookie.Domain = config["Identity:Cookies:Domain"].IsSet()
                    ? config["Identity:Cookies:Domain"]
                    : null;
            });
            return services;
        }

        #endregion

        #region Password Authentication

        public static IServiceCollection ConfigurePasswordAuthentication(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            services.AddDbContext<IdentityContext>(options =>
                options.UseSqlServer(config["ConnectionStrings:DefaultConnection"])
            );
            services.AddScoped<IPasswordAccountRepository, AccountRepository>();
            services.AddScoped<IHoodAccountRepository, AccountRepository>();

            services
                .AddIdentity<ApplicationUser, IdentityRole>(o =>
                {
                    // configure identity options
                    o.User.RequireUniqueEmail = true;

                    o.SignIn.RequireConfirmedEmail = false;
                    o.SignIn.RequireConfirmedPhoneNumber = false;

                    // All of Identity:Password is optional — every value below is Hood's default,
                    // used as-is unless a consumer overrides the specific key.
                    o.Password.RequireDigit = config.GetValue(
                        "Identity:Password:RequireDigit",
                        true
                    );
                    o.Password.RequireLowercase = config.GetValue(
                        "Identity:Password:RequireLowercase",
                        false
                    );
                    o.Password.RequireUppercase = config.GetValue(
                        "Identity:Password:RequireUppercase",
                        false
                    );
                    o.Password.RequireNonAlphanumeric = config.GetValue(
                        "Identity:Password:RequireNonAlphanumeric",
                        true
                    );
                    o.Password.RequiredLength = config.GetValue(
                        "Identity:Password:RequiredLength",
                        6
                    );
                })
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                SetAuthenticationCookieDefaults(config, options);

                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.Domain = config["Identity:Cookies:Domain"].IsSet()
                    ? config["Identity:Cookies:Domain"]
                    : null;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(
                    config.GetValue("Session:Timeout", 60)
                );
                options.SlidingExpiration = true;

                options.Events = new CookieAuthenticationEvents()
                {
                    OnValidatePrincipal = async e =>
                    {
                        // get the user profile and store important bits on the claim.
                        var repo = Engine.Services.Resolve<IPasswordAccountRepository>();
                        var user = await repo.GetUserByIdAsync(e.Principal.GetUserId());
                        if (user?.UserProfile == null)
                        {
                            e.RejectPrincipal();
                            await e.HttpContext.SignOutAsync();
                            return;
                        }
                        e.Principal.SetUserClaims(user.UserProfile);
                        if (user.EmailConfirmed)
                        {
                            e.Principal.AddOrUpdateClaimValue(ClaimTypes.EmailConfirmed, "true");
                        }
                        if (user.Active || !Engine.Settings.Account.RequireEmailConfirmation)
                        {
                            e.Principal.AddOrUpdateClaimValue(ClaimTypes.Active, "true");
                        }
                    },
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    Policies.Active,
                    policy => policy.RequireClaim(ClaimTypes.Active)
                );
                options.AddPolicy(
                    Policies.AccountNotConnected,
                    policy => policy.RequireClaim(ClaimTypes.AccountNotConnected)
                );
                options.AddPolicy(
                    Policies.AccountLinkRequired,
                    policy => policy.RequireClaim(ClaimTypes.AccountLinkRequired)
                );
            });

            services.ConfigurePasswordImpersonation();

            return services;
        }

        public static IServiceCollection ConfigurePasswordImpersonation(
            this IServiceCollection services
        )
        {
            services.Configure<SecurityStampValidatorOptions>(options => // different class name
            {
                options.ValidationInterval = TimeSpan.FromMinutes(1); // new property name
                options.OnRefreshingPrincipal = context => // new property name
                {
                    System.Security.Claims.Claim originalUserIdClaim =
                        context.CurrentPrincipal.FindFirst(ClaimTypes.OriginalUserId);
                    System.Security.Claims.Claim isImpersonatingClaim =
                        context.CurrentPrincipal.FindFirst(ClaimTypes.IsImpersonating);
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

        public static IServiceCollection ConfigureAuth0(
            this IServiceCollection services,
            IConfiguration config,
            IAuth0LoginService auth0Options
        )
        {
            services.AddDbContext<Auth0IdentityContext>(options =>
                options.UseSqlServer(config["ConnectionStrings:DefaultConnection"])
            );
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

            services
                .AddOptions<CookieAuthenticationOptions>(
                    CookieAuthenticationDefaults.AuthenticationScheme
                )
                .Configure(options =>
                {
                    SetAuthenticationCookieDefaults(config, options);
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    Policies.Active,
                    policy => policy.RequireClaim(ClaimTypes.Active)
                );
                options.AddPolicy(
                    Policies.AccountNotConnected,
                    policy => policy.RequireClaim(ClaimTypes.AccountNotConnected)
                );
            });
            return services;
        }

        private static void SetAuthenticationCookieDefaults(
            IConfiguration config,
            CookieAuthenticationOptions options
        )
        {
            string cookieName = config["Identity:Cookies:Name"].IsSet()
                ? config["Identity:Cookies:Name"]
                : Authentication.CookieDefaultName;

            options.Cookie.Name = $"{cookieName}_auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.Domain = config["Identity:Cookies:Domain"].IsSet()
                ? config["Identity:Cookies:Domain"]
                : null;

            options.AccessDeniedPath = config["Identity:AccessDeniedPath"].IsSet()
                ? config["Identity:AccessDeniedPath"]
                : "/account/access-denied";
            options.LoginPath = config["Identity:LoginPath"].IsSet()
                ? config["Identity:LoginPath"]
                : "/account/login";
            options.LogoutPath = config["Identity:LogoutPath"].IsSet()
                ? config["Identity:LogoutPath"]
                : "/account/logout";
            options.ReturnUrlParameter = Authentication.ReturnUrlParameter;
        }

        private static IServiceCollection ConfigureSameSiteNoneCookies(
            this IServiceCollection services
        )
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.CookieOptions);
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

        public static IServiceCollection ConfigureSession(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            string cookieName = config["Identity:Cookies:Name"].IsSet()
                ? config["Identity:Cookies:Name"]
                : Authentication.CookieDefaultName;
            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.Name = $"{cookieName}_td";
                options.Cookie.HttpOnly = true;
                options.Cookie.Domain = config["Identity:Cookies:Domain"].IsSet()
                    ? config["Identity:Cookies:Domain"]
                    : null;
            });

            int sessionTimeout;
            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.Name = $"{cookieName}_session";
                options.Cookie.HttpOnly = true;
                options.Cookie.Domain = config["Identity:Cookies:Domain"].IsSet()
                    ? config["Identity:Cookies:Domain"]
                    : null;

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

        public static IServiceCollection ConfigureHoodSlugRouteConstraints(
            this IServiceCollection services
        )
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

        public static IServiceCollection ConfigureViewEngine(
            this IServiceCollection services,
            IConfiguration config,
            IWebHostEnvironment env
        )
        {
            services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
            {
                options.FileProviders.Add(
                    new EmbeddedFileProvider(typeof(Engine).Assembly, "ComponentLib")
                );
                options.FileProviders.Add(
                    new EmbeddedFileProvider(
                        typeof(IServiceCollectionExtensions).Assembly,
                        "ComponentLib"
                    )
                );

                // In-repo dev loop (Hood.Development): watch the sibling UI package sources so
                // package views live-edit straight from their canonical homes — no copy step.
                // Only wired when the source folders exist next to the content root.
                string repoRoot = Path.GetFullPath(Path.Combine(env.ContentRootPath, ".."));
                string activeUI = UserInterfaceProvider.GetActiveUIAssembly(config, env);
                foreach (string package in new[] { "Hood.UI.Core", "Hood.UI.Admin", activeUI })
                {
                    if (package == null)
                    {
                        continue;
                    }
                    string packageDir = Path.Combine(repoRoot, package);
                    if (Directory.Exists(packageDir))
                    {
                        options.FileProviders.Add(new PhysicalFileProvider(packageDir));
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
