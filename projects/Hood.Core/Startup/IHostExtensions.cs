using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Data.SqlClient;
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

                    if (Engine.Configuration.InitializeOnStartup)
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
                        try
                        {
                            // Ensure the database is seeded, or throw issue.
                            if (!db.Options.Any(o => o.Id == "Hood.Version"))
                            {
                                // No version set in the database... this means unseeded.
                                throw new StartupException("Database is not initialised. Could not get version from database. Ensure database is up to date with migrations and seeding.", StartupError.DatabaseNotSeeded);
                            }
                        }
                        catch (Microsoft.Data.SqlClient.SqlException sqlException)
                        {
                            throw new StartupException("Failed to connect to the database in startup procedure. Ensure database is up to date with migrations and seeding.", sqlException, StartupError.DatabaseConnectionFailed);
                        }
                    }


                }
                catch (StartupException)
                { }

            }
            return host;
        }

    }
}
