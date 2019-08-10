using Hood.Interfaces;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Hood.Core
{
    class HoodServiceProvider : IHoodServiceProvider
    {
        public bool DatabaseMigrationFailed { get; set; }
        public bool DatabaseMigrationsMissing { get; set; }
        public bool DatabaseSeedFailed { get; set; }
        private IServiceProvider _serviceProvider { get; set; }
        public virtual IServiceProvider ServiceProvider => _serviceProvider;
        protected IServiceProvider GetServiceProvider()
        {
            var accessor = ServiceProvider.GetService<IHttpContextAccessor>();
            var context = accessor.HttpContext;
            return context != null ? context.RequestServices : ServiceProvider;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration config)
        {

            // Register engine
            services.AddSingleton<IHoodServiceProvider>(this);

            // Register type finder
            var typeFinder = new TypeFinder();
            services.AddSingleton<ITypeFinder>(typeFinder);

            // Register single instance singletons
            services.AddSingleton(config);
            services.AddSingleton<IEventsService>(new EventsService());

            // Register all Hood Components
            var dependencies = typeFinder.FindClassesOfType<IHoodComponent>();

            var instances = dependencies
                                .Select(dependencyRegistrar => (IHoodComponent)Activator.CreateInstance(dependencyRegistrar))
                                .OrderBy(dependencyRegistrar => dependencyRegistrar.ServiceConfigurationOrder);

            foreach (var dependency in instances)
                dependency.ConfigureServices(services, config);

            _serviceProvider = services.BuildServiceProvider();
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
