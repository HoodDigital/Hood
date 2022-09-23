using System.Threading.Tasks;
using Hood.Startup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Hood.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = await CreateHostBuilder(args)
                .Build()
                .LoadHoodAsync();

            builder.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}
