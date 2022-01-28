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
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using Hood.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

            if (!config.IsDatabaseConnected())
            {
                throw new StartupException("No database connected.", StartupError.DatabaseConnectionFailed);
            }

            services.ConfigureHoodDatabase<TContext>(config);
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.ConfigureHoodServices();

            services.ConfigureCache(config);

            if (config.IsConfigured("Identity:Auth0:Domain") && config.IsConfigured("Identity:Auth0:ClientId"))
            {
                services.ConfigureAuth0(config);
            }
            else
            {
                services.ConfigureAuthentication(config);
            }

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
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IContentRepository, ContentRepository>();

            services.AddScoped<IRazorViewRenderer, RazorViewRenderer>();
            services.AddScoped<IPageBuilder, PageBuilder>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<ISmsSender, SmsSender>();
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
            string cookieName = config["Cookies:Name"].IsSet() ? config["Cookies:Name"] : Constants.CookieDefaultName;
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = $"{cookieName}_af";
            });
            return services;
        }
        public static IServiceCollection ConfigureCookies(this IServiceCollection services, IConfiguration config)
        {
            string cookieName = config["Cookies:Name"].IsSet() ? config["Cookies:Name"] : Constants.CookieDefaultName;
            bool consentRequired = config.GetValue("Cookies:ConsentRequired", true);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => consentRequired;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.ConsentCookie.Name = $"{cookieName}_consent";
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

            services.ConfigureApplicationCookie(options =>
            {
                SetAuthenticationCookieDefaults(config, options);

                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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
                            e.Principal.AddOrUpdateClaim(HoodClaimTypes.EmailConfirmed, "true");
                        }
                        if (user.Active || !Engine.Settings.Account.RequireEmailConfirmation)
                        {
                            e.Principal.AddOrUpdateClaim(HoodClaimTypes.Active, "true");
                        }
                    }
                };
            });

            services.AddScoped<IAccountRepository, Auth0AccountRepository>();
            services.AddScoped<IEmailSender, Auth0EmailSender>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.Active, policy => policy.RequireClaim(Identity.HoodClaimTypes.Active));
                options.AddPolicy(Policies.AccountNotConnected, policy => policy.RequireClaim(Identity.HoodClaimTypes.AccountNotConnected));
            });

            return services;
        }

        private static int SignInManager<T>()
        {
            throw new NotImplementedException();
        }

        public static IServiceCollection ConfigureAuth0(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigureSameSiteNoneCookies();

            services
                .AddAuth0WebAppAuthentication(options =>
                {
                    options.Domain = config["Identity:Auth0:Domain"];
                    options.ClientId = config["Identity:Auth0:ClientId"];
                    options.Scope = "openid profile email";

                    options.OpenIdConnectEvents = new OpenIdConnectEvents()
                    {
                        OnTicketReceived = async e =>
                        {
                            var linkGenerator = Engine.Services.Resolve<LinkGenerator>();
                            var auth0Service = new Auth0Service();
                            var repo = Engine.Services.Resolve<IAccountRepository>();
                            var principal = e.Principal;
                            var userId = e.Principal.GetUserId();
                            var returnUrl = e.ReturnUri;

                            // Check if user exists and is linked to this Auth0 account
                            var user = await repo.GetUserByAuth0Id(userId);
                            if (user != null)
                            {
                                // user exists and has auth0 account linked to it.
                                var emailVerifiedClaim = e.Principal.FindFirst("email_verified");
                                if (user.Active || (emailVerifiedClaim != null && emailVerifiedClaim.Value == "true"))
                                {
                                    var identity = (ClaimsIdentity)principal.Identity;
                                    identity.AddClaim(new Claim(Identity.HoodClaimTypes.Active, "true"));
                                    await e.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, e.Properties);
                                }
                                return;

                            }

                            // check if the user has an account, via email, if so, the should be asked to link to this account                           
                            user = await repo.GetUserByEmailAsync(e.Principal.GetEmail());
                            if (user != null)
                            {
                                // user exists, but the current Auth0 signin method is not saved, force them into the connect account flow.
                                var identity = (ClaimsIdentity)principal.Identity;
                                identity.AddClaim(new Claim(Identity.HoodClaimTypes.AccountNotConnected, "true"));
                                await e.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, e.Properties);

                                e.Response.Redirect(linkGenerator.GetPathByAction("ConnectAccount", "Account", new { e.ReturnUri }));
                                e.HandleResponse();
                                return;
                            }


                            if (!Engine.Settings.Account.AllowRemoteSignups)
                            {
                                // user has not been found, or created & signups are disabled on this end
                                returnUrl = linkGenerator.GetPathByAction("RemoteSigninFailed", "Account", new { r = "signup-disabled" });
                                e.Response.Redirect(linkGenerator.GetPathByAction("SignOut", "Account", new { returnUrl }));
                                e.HandleResponse();
                                return;
                            }

                            // user does not exist, signups are allowed, so create and send them to the complete signup page.
                            var authService = new Auth0Service();
                            var authUser = await authService.GetUserById(userId);
                            if (authUser == null)
                            {
                                throw new ApplicationException("Something went wrong while authorizing your account.");
                            }
                            user = new ApplicationUser
                            {
                                UserName = authUser.UserName.IsSet() ? authUser.UserName : authUser.Email,
                                Email = authUser.Email,
                                FirstName = authUser.FirstName,
                                LastName = authUser.LastName,
                                DisplayName = authUser.NickName,
                                PhoneNumber = authUser.PhoneNumber,
                                EmailConfirmed = authUser.EmailVerified.HasValue ? authUser.EmailVerified.Value : false,
                                Active = authUser.EmailVerified.HasValue ? authUser.EmailVerified.Value : false,
                                Avatar = authUser.Picture.IsSet() ? new MediaObject(authUser.Picture) : null,
                                CreatedOn = DateTime.UtcNow,
                                LastLogOn = DateTime.UtcNow,
                                LastLoginLocation = authUser.LastIpAddress,
                                LastLoginIP = authUser.LastIpAddress
                            };
                            var keygen = new KeyGenerator();
                            var password = keygen.Generate(16);
                            var result = await repo.CreateAsync(user, password);
                            if (!result.Succeeded)
                            {
                                // user has not been found, or created (signups disabled on this end) - signout and forward to failure page.
                                returnUrl = linkGenerator.GetPathByAction("RemoteSigninFailed", "Account", new { r = "account-creation-failed" });
                                e.Response.Redirect(linkGenerator.GetPathByAction("SignOut", "Account", new { returnUrl }));
                                e.HandleResponse();
                                return;
                            }

                            // If the user is still not found, there is a problem... 
                            if (user == null)
                            {
                                // user has not been found, or created (signups disabled on this end) - signout and forward to failure page.
                                returnUrl = linkGenerator.GetPathByAction("RemoteSigninFailed", "Account", new { r = "account-linking-failed" });
                                e.Response.Redirect(linkGenerator.GetPathByAction("SignOut", "Account", new { returnUrl }));
                                e.HandleResponse();
                                return;
                            }

                            // User exists at this point, for sure, but no auth0 connection for it, create it now.
                            await authService.CreateLocalAuth0User(e.Principal.GetUserId(), user);

                            if (user.Active)
                            {
                                // Account is already active, so mark it as such, this allows access to secure areas.
                                var identity = (ClaimsIdentity)principal.Identity;
                                identity.AddClaim(new Claim(Identity.HoodClaimTypes.Active, "true"));
                            }

                            // Set the remote avatar on a local claim, in case the local overrides it. 
                            if (e.Principal.HasClaim(HoodClaimTypes.Picture))
                            {
                                e.Principal.AddOrUpdateClaim(HoodClaimTypes.RemotePicture, e.Principal.GetClaim(HoodClaimTypes.Picture));
                            }

                            // Set the user claims locally
                            e.Principal.SetUserClaims(user);
                            await e.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, e.Principal, e.Properties);

                            // Account setup complete, send user to manage profile with new-account-connection flag.
                            returnUrl = linkGenerator.GetPathByAction("Index", "Manage", new { r = "new-account-connection" });
                            e.Response.Redirect(returnUrl);
                            e.HandleResponse();
                        },

                        OnAccessDenied = e =>
                        {
                            return Task.CompletedTask;
                        },

                        OnTokenValidated = e =>
                        {
                            return Task.CompletedTask;
                        },

                        OnMessageReceived = e =>
                        {
                            return Task.CompletedTask;
                        },

                        OnTokenResponseReceived = e =>
                        {
                            return Task.CompletedTask;
                        },

                        OnAuthorizationCodeReceived = e =>
                        {
                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = e =>
                        {
                            // user has not been found, or created (signups disabled on this end) - signout and forward to failure page.
                            var linkGenerator = Engine.Services.Resolve<LinkGenerator>();
                            var returnUrl = linkGenerator.GetPathByAction("RemoteSigninFailed", "Account", new { r = "auth-failed" });
                            e.Response.Redirect(linkGenerator.GetPathByAction("SignOut", "Account", new { returnUrl }));
                            e.HandleResponse();
                            return Task.CompletedTask;
                        },

                        OnRemoteFailure = e =>
                        {
                            // // user has not been found, or created (signups disabled on this end) - signout and forward to failure page.
                            string reason = "remote-failed";
                            if (e.Failure?.Message != null && e.Failure.Message.ToLower().Contains("message.state"))
                            {
                                reason = "state-failure";
                            }
                            var linkGenerator = Engine.Services.Resolve<LinkGenerator>();
                            var returnUrl = linkGenerator.GetPathByAction("RemoteSigninFailed", "Account", new { r = reason });
                            e.Response.Redirect(linkGenerator.GetPathByAction("SignOut", "Account", new { returnUrl }));
                            e.HandleResponse();
                            return Task.CompletedTask;
                        }

                    };
                });

            // Mock Aspnet Identity stores.
            services.TryAddScoped<IUserValidator<ApplicationUser>, UserValidator<ApplicationUser>>();
            services.TryAddScoped<IPasswordValidator<ApplicationUser>, PasswordValidator<ApplicationUser>>();
            services.TryAddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            services.TryAddScoped<IdentityErrorDescriber>();
            //services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<ApplicationUser>>();

            var identityBuilder = new IdentityBuilder(typeof(ApplicationUser), services);

            identityBuilder.AddRoles<IdentityRole>();
            identityBuilder.AddRoleStore<RoleStore<IdentityRole, HoodDbContext, string>>();
            identityBuilder.AddUserStore<UserStore<ApplicationUser, IdentityRole, HoodDbContext, string>>();
            identityBuilder.AddUserManager<UserManager<ApplicationUser>>();

            services.AddScoped<IAccountRepository, Auth0AccountRepository>();
            services.AddScoped<IEmailSender, Auth0EmailSender>();

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
            string cookieName = config["Cookies:Name"].IsSet() ? config["Cookies:Name"] : Constants.CookieDefaultName;

            options.Cookie.Name = $"{cookieName}_auth";
            options.Cookie.HttpOnly = true;

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

            string cookieName = config["Cookies:Name"].IsSet() ? config["Cookies:Name"] : Constants.CookieDefaultName;
            services.Configure<CookieTempDataProviderOptions>(options =>
                options.Cookie.Name = $"{cookieName}_td"
            );

            int sessionTimeout = 60;
            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.Name = $"{cookieName}_session";
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
            Type roleType = typeof(IdentityRole<string>);
            Type contextType = typeof(HoodDbContext);
            Type keyType = typeof(string);

            Type userStoreType;
            Type roleStoreType;
            var identityContext = typeof(HoodDbContext);

            userStoreType = typeof(UserStore<ApplicationUser, IdentityRole<string>, HoodDbContext, string>);
            roleStoreType = typeof(RoleStore<IdentityRole<string>, HoodDbContext, string>);


            services.TryAddScoped(typeof(IRoleValidator<IdentityRole<string>>), typeof(RoleValidator<IdentityRole<string>>));
            services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
            services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), roleStoreType);
            services.TryAddScoped<RoleManager<IdentityRole<string>>>();
            services.AddScoped(typeof(IUserClaimsPrincipalFactory<>).MakeGenericType(userType), typeof(UserClaimsPrincipalFactory<,>).MakeGenericType(userType, roleType));

            services.AddScoped(typeof(UserManager<>).MakeGenericType(userType), typeof(UserManager<ApplicationUser>));

            return services;
        }
    }
}
