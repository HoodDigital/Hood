using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hood.Extensions
{
    public static class IWebHostExtensions
    {
        public static void InstallDatabase<TDbContext>(this IWebHost host)
            where TDbContext : HoodDbContext
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Get the database from the services provider
                    var db = scope.ServiceProvider.GetService<TDbContext>();

                    // Migrate the database
                    db.Database.Migrate();

                    // Seed the database
                    var userManager = services.GetService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetService<RoleManager<IdentityRole>>();
                    db.Seed(userManager, roleManager);

                    // Configure any event listeners. TODO: Make this a TypeFinder iteration.
                    services.GetService<SubscriptionsEventListener>().Configure();
                }
                catch (Exception ex)
                {
                    var logService = scope.ServiceProvider.GetService<ILogService>();
                    logService.AddExceptionAsync<IWebHost>("An error occurred during the seed function.", ex);
                }
            }
        }

    }
}
