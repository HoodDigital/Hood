using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Hood.Core.Admin
{
    public class Component : Hood.Interfaces.IHoodComponent
    {
        public int ServiceConfigurationOrder => 1;

        public string Name => "Hood.Core.Admin";

        public bool IsUIComponent => false;

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration config)
        { }

        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        { }
    }
}
