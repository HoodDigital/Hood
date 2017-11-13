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

        public void ConfigureRequestPipeline(IApplicationBuilder application)
        {
            throw new NotImplementedException();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var builder = new ContainerBuilder();

            //register engine
            builder.RegisterInstance(this).As<IEngine>().SingleInstance();


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
            services.AddSingleton<ISettingsRepository, SettingsRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IMediaManager<MediaObject>, MediaManager<MediaObject>>();
            services.AddScoped<IContentRepository, ContentRepository>();
            services.AddScoped<IStripeWebHookService, StripeWebHookService>();
            services.AddTransient<ISmsSender, SmsSender>();
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

            //populate Autofac container builder with the set of registered service descriptors
            builder.Populate(services);

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
