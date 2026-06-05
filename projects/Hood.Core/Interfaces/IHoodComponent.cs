using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hood.Interfaces
{
    /// <summary>
    /// Dependency list interface, used to define what dependencies need to be registered or added in Startup
    /// </summary>
    public interface IHoodComponent
    {
        string Name { get; }
        bool IsUIComponent { get; }

        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="services">Services collection</param>
        /// <param name="config">Application configuration</param>
        void ConfigureServices(IServiceCollection services, IConfiguration config);

        /// <summary>
        /// Gets order of this dependency registrar implementation
        /// </summary>
        int ServiceConfigurationOrder { get; }

        /// <summary>
        /// Configure the application pipeline for this component
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="env">Hosting environment</param>
        /// <param name="config">Application configuration</param>
        void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration config);
    }
}
