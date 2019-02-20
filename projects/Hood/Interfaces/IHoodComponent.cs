using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hood.Interfaces
{
    /// <summary>
    /// Dependency list interface, used to define what dependencies need to be registered or added in Startup
    /// </summary>
    public interface IHoodComponent
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Services collection</param>
        void ConfigureServices(IServiceCollection services, IConfiguration config);

        /// <summary>
        /// Gets order of this dependency registrar implementation
        /// </summary>
        int ServiceConfigurationOrder { get; }

        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Services collection</param>
        void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration config);
    }
}
