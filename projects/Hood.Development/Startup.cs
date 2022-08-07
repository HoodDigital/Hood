using Hood.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hood.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            this.Configuration = configuration;
            this.Environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureHood(Configuration, Environment);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseHood(Environment, Configuration);
        }
    }
}
