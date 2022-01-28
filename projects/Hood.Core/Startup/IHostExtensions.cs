using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Startup
{
    public static class IHostExtensions
    {
        public static async Task<IHost> LoadHoodAsync<TDbContext>(this IHost host)
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

                    Engine.Services.DatabaseConnectionFailed = false;
                    Engine.Services.DatabaseMigrationsMissing = false;
                    Engine.Services.MigrationNotApplied = false;
                    Engine.Services.AdminUserSetupError = false;
                    Engine.Services.DatabaseMediaTimeout = false;

                    if (Engine.Configuration.SeedOnStart)
                    {
                        if (Engine.Configuration.ApplyMigrationsAutomatically && !db.AllMigrationsApplied())
                        {
                            // attempt to apply migrations. 
                            db.Database.Migrate();
                            
                        }
                        else if (!db.AllMigrationsApplied())
                        {
                            throw new StartupException("There are migrations that are not applied to the database.", StartupError.MigrationNotApplied);
                        }
                        // Seed the database
                        await db.Seed();
                    }
                    else
                    {
                        // Ensure the database is seeded, or throw issue.
                        if (!db.Options.Any(o => o.Id == "Hood.Version"))
                        {
                            // No version set in the database... this means unseeded.
                            throw new Exception("Database is not initialised.");
                        }
                    }


                    Engine.Services.DatabaseSeedFailed = false;

                }
                catch (StartupException ex)
                {
                    Engine.Services.DatabaseSeedFailed = true;
                    Engine.Services.Details = ex.InnerException;
                }
                catch (Exception ex)
                {
                    Engine.Services.DatabaseSeedFailed = true;
                    Engine.Services.Details = ex;
                }

            }
            return host;
        }

    }
}
