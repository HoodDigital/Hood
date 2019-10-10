using Hood.Startup;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Hood.Web
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
