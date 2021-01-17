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

namespace Hood.Admin
{
    public class Component : Hood.Interfaces.IHoodComponent
    {
        public int ServiceConfigurationOrder => 1;

        public string Name => "Hood.Admin";

        public bool IsUIComponent => true;

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration config)
        { }

        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
            {
                // Get a reference to the assembly that contains the view components
                Assembly assembly = typeof(Component).Assembly;
                // Create an EmbeddedFileProvider for that assembly
                EmbeddedFileProvider embeddedFileProvider = new EmbeddedFileProvider(
                    assembly,
                    "Hood.Admin"
                );
                options.FileProviders.Add(embeddedFileProvider);
            });
        }
    }
}
