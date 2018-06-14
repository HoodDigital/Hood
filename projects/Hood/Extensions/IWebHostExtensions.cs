using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Hood.Extensions
{
    public static class IWebHostExtensions
    {
        public static void SeedHoodData<TDbContext>(this IWebHost host)
            where TDbContext : HoodDbContext
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var config = services.GetService<IConfiguration>();
                    scope.ServiceProvider.GetService<TDbContext>().Database.Migrate();
                    var userManager = services.GetService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetService<RoleManager<IdentityRole>>();
                    var options = new DbContextOptionsBuilder<HoodDbContext>();
                    options.UseSqlServer(config["ConnectionStrings:DefaultConnection"]);
                    var db = new HoodDbContext(options.Options);
                    db.EnsureSetup(userManager, roleManager);

                    // Initialise events
                    services.GetService<SubscriptionsEventListener>().Configure();
                }
                catch (Exception ex)
                {
                    var logService = services.GetService<ILogService>();
                    logService.AddLogAsync("An error occurred during the seed function.", ex, Models.LogType.Error, Models.LogSource.System, null, null, nameof(IWebHostExtensions), null);
                }
            }
            host.Run();
        }

    }
}
