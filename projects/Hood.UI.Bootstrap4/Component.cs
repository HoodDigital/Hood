using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hood.UI.Bootstrap4
{
    public class Component : Hood.Interfaces.IHoodComponent
    {
        public int ServiceConfigurationOrder => 1;

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration config)
        {}

        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {}
    }
}
