using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Hood.Web.MVC
{
    public class Startup
    {
        private IConfigurationRoot _config { get; }

        public Startup(IHostingEnvironment env)
        {
            _config = HoodStartup.Config(env, settings => { });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            HoodStartup.ConfigureServices(services, _config);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            HoodStartup.Configure(app, env, loggerFactory, _config, customRoutes => { });
        }
    }
}
