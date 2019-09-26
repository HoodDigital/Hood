using Hood.Core;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace Hood.Extensions
{
    public static class IWebHostExtensions
    {
        public static IWebHost LoadHood<TDbContext>(this IWebHost host)
            where TDbContext : HoodDbContext
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                // Get the database from the services provider
                var db = scope.ServiceProvider.GetService<TDbContext>();
                if (db == null)
                {
                    Engine.Services.DatabaseConnectionFailed = true;
                }

                try
                {
                    // Seed the database
                    var userManager = services.GetService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetService<RoleManager<IdentityRole>>();
                    db.Seed(userManager, roleManager);
                }
                catch (Exception ex)
                {
                    var logService = scope.ServiceProvider.GetService<ILogService>();
                    if (ex.Message.Contains("Invalid object name"))
                    {
                        // migrations have failed, suggest adding migrations.
                        logService.AddExceptionAsync<IWebHost>("An error occurred while seeding the database with base settings due to an invalid database object, perhaps migrations are missing.", ex);
                        Engine.Services.DatabaseSeedFailed = true;
                        Engine.Services.DatabaseMigrationsMissing = true;
                    }
                    else
                    {
                        logService.AddExceptionAsync<IWebHost>("An error occurred while seeding the database with base settings.", ex);
                        Engine.Services.DatabaseSeedFailed = true;
                    }
                }

                try
                {
                    // Configure any event listeners. TODO: Make this a TypeFinder iteration.
                    services.GetService<SubscriptionsEventListener>().Configure();
                }
                catch (Exception ex)
                {
                    var logService = scope.ServiceProvider.GetService<ILogService>();
                    logService.AddExceptionAsync<IWebHost>("An error occurred while configuring event listeners.", ex);
                }
            }
            return host;
        }

    }
}
