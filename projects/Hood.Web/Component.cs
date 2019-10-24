using Hood.Interfaces;
using Hood.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hood.Web
{
    public class Component : IHoodComponent
    {
        public int ServiceConfigurationOrder => 0;

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration config)
        {
            app.UseHoodDefaults(env, config);
            app.UseHoodDefaultRoutes(config);
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.ConfigureHood<ApplicationDbContext>(config);
        }
    }
}
