using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Hood.Interfaces;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hood.Services;
using Hood.Models;
using Hood.Caching;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Hood.Infrastructure;
using Newtonsoft.Json.Serialization;
using Hood.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Razor;
using Hood.IO;
using Hood.Extensions;

namespace Hood.Core
{
    class HoodEngine : IEngine
    {
        private IServiceProvider _serviceProvider { get; set; }
        public virtual IServiceProvider ServiceProvider => _serviceProvider;
        protected IServiceProvider GetServiceProvider()
        {
            var accessor = ServiceProvider.GetService<IHttpContextAccessor>();
            var context = accessor.HttpContext;
            return context != null ? context.RequestServices : ServiceProvider;
        }

        public IServiceProvider ConfigureServices<TDbContext>(IServiceCollection services, IConfiguration configuration)
            where TDbContext : DbContext
        {
            var builder = new ContainerBuilder();

            //register engine
            builder.RegisterInstance(this).As<IEngine>().SingleInstance();

            services.AddSingleton(configuration);

            services.AddSingleton<EventsService>();
            services.AddSingleton<SubscriptionsEventListener>();
            services.AddSingleton<ContentCategoryCache>();
            services.AddSingleton<ContentByTypeCache>();
            services.AddSingleton<IHoodCache, HoodCache>();
            services.AddSingleton<IFTPService, FTPService>();
            services.AddSingleton<IPropertyImporter, PropertyImporter>();
            services.AddSingleton<IMediaRefreshService, MediaRefreshService>();
            services.AddSingleton<IPropertyExporter, PropertyExporter>();
            services.AddSingleton<IContentExporter, ContentExporter>();
            services.AddSingleton<IThemesService, ThemesService>();
            services.AddSingleton<IAddressService, AddressService>();
            services.AddSingleton<ISettingsRepository, SettingsRepository>();
            services.AddSingleton<IMediaManager<MediaObject>, MediaManager<MediaObject>>();
            services.AddTransient<IStripeService, StripeService>();
            services.AddTransient<ISubscriptionPlanService, SubscriptionPlanService>();
            services.AddTransient<ISubscriptionService, SubscriptionService>();
            services.AddTransient<ICardService, CardService>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<IInvoiceService, InvoiceService>();
            services.AddTransient<IBillingService, BillingService>();
            services.AddTransient<FormSenderService>();
            services.AddTransient<WelcomeEmailSender>();
            services.AddScoped<IRazorViewRenderer, RazorViewRenderer>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IContentRepository, ContentRepository>();
            services.AddScoped<IStripeWebHookService, StripeWebHookService>();
            services.AddScoped<ISmsSender, SmsSender>();
            services.AddScoped<IEmailSender, EmailSender>();

            //populate Autofac container builder with the set of registered service descriptors
            builder.Populate(services);

            services.AddMvc();

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(EmbeddedFiles.GetProvider());
                if (configuration.IsDatabaseConfigured())
                {
                    options.ViewLocationExpanders.Add(new ViewLocationExpander());
                }
            });

            // Add framework services.
            if (configuration.IsDatabaseConfigured())
            {
                services.AddDbContext<TDbContext>(options => options.UseSqlServer(configuration["ConnectionStrings:DefaultConnection"], b => { b.UseRowNumberForPaging(); }));
                services.AddDbContext<HoodDbContext>(options => options.UseSqlServer(configuration["ConnectionStrings:DefaultConnection"], b => { b.UseRowNumberForPaging(); }));

                services.AddIdentity<ApplicationUser, IdentityRole>(o =>
                {
                    // configure identity options
                    o.Password.RequireDigit = configuration["Identity:Password:RequireDigit"].IsSet() ? bool.Parse(configuration["Identity:Password:RequireDigit"]) : true;
                    o.Password.RequireLowercase = configuration["Identity:Password:RequireLowercase"].IsSet() ? bool.Parse(configuration["Identity:Password:RequireLowercase"]) : false;
                    o.Password.RequireUppercase = configuration["Identity:Password:RequireUppercase"].IsSet() ? bool.Parse(configuration["Identity:Password:RequireUppercase"]) : false;
                    o.Password.RequireNonAlphanumeric = configuration["Identity:Password:RequireNonAlphanumeric"].IsSet() ? bool.Parse(configuration["Identity:Password:RequireNonAlphanumeric"]) : true;
                    o.Password.RequiredLength = configuration["Identity:Password:RequiredLength"].IsSet() ? int.Parse(configuration["Identity:Password:RequiredLength"]) : 6;
                })
                .AddEntityFrameworkStores<HoodDbContext>()
                .AddDefaultTokenProviders();

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

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = ".Hood.Antiforgery";
            });

            services.AddSession(options =>
            {
                options.Cookie.Name = ".Hood.Session";
                options.Cookie.HttpOnly = true;
            });

            services.AddApplicationInsightsTelemetry(configuration);


            //create service provider
            _serviceProvider = new AutofacServiceProvider(builder.Build());
            return _serviceProvider;
        }

        public void Initialize(IServiceCollection services)
        {
            //most of API providers require TLS 1.2 nowadays
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //set base application path
            var provider = services.BuildServiceProvider();
            var hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();

            //initialize plugins
            var mvcCoreBuilder = services.AddMvcCore();
        }

        public T Resolve<T>() where T : class
        {
            return (T)GetServiceProvider().GetRequiredService(typeof(T));
        }

        public object Resolve(Type type)
        {
            return GetServiceProvider().GetRequiredService(type);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return (IEnumerable<T>)GetServiceProvider().GetServices(typeof(T));
        }

        public object ResolveUnregistered(Type type)
        {
            Exception innerException = null;
            foreach (var constructor in type.GetConstructors())
            {
                try
                {
                    //try to resolve constructor parameters
                    var parameters = constructor.GetParameters().Select(parameter =>
                    {
                        var service = Resolve(parameter.ParameterType);
                        if (service == null)
                            throw new Exception("Unknown dependency");
                        return service;
                    });

                    //all is ok, so create instance
                    return Activator.CreateInstance(type, parameters.ToArray());
                }
                catch (Exception ex)
                {
                    innerException = ex;
                }
            }
            throw new Exception("No constructor was found that had all the dependencies satisfied.", innerException);
        }
    }
}
