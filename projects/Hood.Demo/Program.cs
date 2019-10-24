using Hood.Demo.Models;
using Hood.Startup;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Hood.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IWebHost host = BuildWebHost(args);
            host.LoadHood<ApplicationDbContext>().Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
             WebHost.CreateDefaultBuilder(args)
                 .UseStartup<Startup>()
                 .Build();
    }
}
