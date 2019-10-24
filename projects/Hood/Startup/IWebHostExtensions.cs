using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hood.Startup
{
    public static class IWebHostExtensions
    {
        public static IWebHost LoadHood<TDbContext>(this IWebHost host)
            where TDbContext : HoodDbContext
        {
            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                // Get the database from the services provider
                TDbContext db = scope.ServiceProvider.GetService<TDbContext>();

                try
                {
                    if (db == null)
                    {
                        throw new StartupException("No database connection could be established.", StartupError.DatabaseConnectionFailed);
                    }
                    // Seed the database
                    UserManager<ApplicationUser> userManager = services.GetService<UserManager<ApplicationUser>>();
                    RoleManager<IdentityRole> roleManager = services.GetService<RoleManager<IdentityRole>>();
                    db.Seed(userManager, roleManager);

                    Engine.Services.DatabaseConnectionFailed = false;
                    Engine.Services.DatabaseMigrationsMissing = false;
                    Engine.Services.MigrationNotApplied = false;
                    Engine.Services.AdminUserSetupError = false;
                    Engine.Services.DatabaseMediaTimeout = false;
                    Engine.Services.DatabaseSeedFailed = false;

                }
                catch (StartupException startupException)
                {
                    switch (startupException.Error)
                    {
                        case StartupError.MigrationMissing:
                            Engine.Services.DatabaseConnectionFailed = false;
                            Engine.Services.DatabaseMigrationsMissing = true;
                            break;
                        case StartupError.MigrationNotApplied:
                            Engine.Services.DatabaseMigrationsMissing = false;
                            Engine.Services.DatabaseConnectionFailed = false;
                            Engine.Services.MigrationNotApplied = true;
                            break;
                        case StartupError.AdminUserSetupError:
                            Engine.Services.DatabaseConnectionFailed = false;
                            Engine.Services.DatabaseMigrationsMissing = false;
                            Engine.Services.MigrationNotApplied = false;
                            Engine.Services.AdminUserSetupError = true;
                            break;
                        case StartupError.DatabaseConnectionFailed:
                            Engine.Services.DatabaseConnectionFailed = true;
                            Engine.Services.DatabaseMigrationsMissing = true;
                            Engine.Services.MigrationNotApplied = true;
                            Engine.Services.AdminUserSetupError = true;
                            break;
                        case StartupError.DatabaseMediaTimeout:
                            Engine.Services.DatabaseConnectionFailed = false;
                            Engine.Services.DatabaseMigrationsMissing = false;
                            Engine.Services.MigrationNotApplied = false;
                            Engine.Services.AdminUserSetupError = false;
                            Engine.Services.DatabaseMediaTimeout = true;
                            break;
                    }
                    Engine.Services.DatabaseSeedFailed = true;
                }
                catch (Exception)
                {
                    Engine.Services.DatabaseSeedFailed = true;
                }

                try
                {
                    // Configure any event listeners. TODO: Make this a TypeFinder iteration.
                    if (Engine.Services.Installed)
                    {
                        services.GetService<SubscriptionsEventListener>().Configure();
                    }
                }
                catch (Exception ex)
                {
                    if (Engine.Services.Installed)
                    {
                        ILogService logService = scope.ServiceProvider.GetService<ILogService>();
                        logService.AddExceptionAsync<IWebHost>("An error occurred while configuring event listeners.", ex);
                    }
                }
            }
            return host;
        }

    }
}
