using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Hood.Models
{
    public static class DbContextExtensions
    {
        public static bool AllMigrationsApplied(this HoodDbContext context)
        {
            var applied = context.GetService<IHistoryRepository>()
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);

            var total = context.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(m => m.Key);

            return !total.Except(applied).Any();
        }

        public static void EnsureSetup(this HoodDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (context.AllMigrationsApplied())
            {
                foreach (var role in Roles.All)
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                    {
                        IdentityResult irAdmin = roleManager.CreateAsync(new IdentityRole(role)).Result;
                    }
                }
                if (!context.Users.Any(u => u.UserName == "admin@hooddigital.com"))
                {
                    var userToInsert = new ApplicationUser
                    {
                        CompanyName = "Hood",
                        CreatedOn = DateTime.Now,
                        FirstName = "Hood",
                        LastName = "Digital",
                        EmailConfirmed = true,
                        EmailOptin = false,
                        PhoneNumber = "03309001900",
                        JobTitle = "Website Administrator",
                        LastLogOn = DateTime.Now,
                        LastLoginIP = "127.0.0.1",
                        LastLoginLocation = "Nottingham, UK",
                        Email = "admin@hooddigital.com",
                        UserName = "admin@hooddigital.com"
                    };
                    IdentityResult ir = userManager.CreateAsync(userToInsert, "Password@123").Result;
                    if (ir.Succeeded)
                    {
                        var user = userManager.FindByEmailAsync(userToInsert.UserName).Result;
                        if (userManager.SupportsUserRole)
                        {
                            foreach (var role in Roles.All)
                            {
                                if (!userManager.IsInRoleAsync(user, role.ToUpper()).Result)
                                {
                                    IdentityResult addToRole = userManager.AddToRoleAsync(user, role).Result;
                                }
                            }
                        }
                    }
                }
                if (!context.Options.Any(o => o.Id == "Hood.Settings.Theme"))
                {
                    context.Options.Add(new Option { Id = "Hood.Settings.Theme", Value = JsonConvert.SerializeObject("default") });
                }

                if (!context.Options.Any(o => o.Id == "Hood.Settings.Basic"))
                {
                    context.Options.Add(new Option { Id = "Hood.Settings.Basic", Value = JsonConvert.SerializeObject(new ContactSettings()) });
                }

                if (!context.Options.Any(o => o.Id == "Hood.Settings.Contact"))
                {
                    context.Options.Add(new Option { Id = "Hood.Settings.Contact", Value = JsonConvert.SerializeObject(new ContactSettings()) });
                }

                if (!context.Options.Any(o => o.Id == "Hood.Settings.Billing"))
                {
                    context.Options.Add(new Option { Id = "Hood.Settings.Billing", Value = JsonConvert.SerializeObject(new BillingSettings()) });
                }

                if (!context.Options.Any(o => o.Id == "Hood.Settings.Media"))
                {
                    context.Options.Add(new Option { Id = "Hood.Settings.Media", Value = JsonConvert.SerializeObject(new MediaSettings()) });
                }

                if (!context.Options.Any(o => o.Id == "Hood.Settings.Content"))
                {
                    context.Options.Add(new Option { Id = "Hood.Settings.Content", Value = JsonConvert.SerializeObject(new ContentSettings()) });
                }

                if (!context.Options.Any(o => o.Id == "Hood.Settings.Property"))
                {
                    context.Options.Add(new Option { Id = "Hood.Settings.Property", Value = JsonConvert.SerializeObject(new PropertySettings()) });
                }

                if (!context.Options.Any(o => o.Id == "Hood.Settings.Seo"))
                {
                    context.Options.Add(new Option { Id = "Hood.Settings.Seo", Value = JsonConvert.SerializeObject(new SeoSettings()) });
                }

                if (!context.Options.Any(o => o.Id == "Hood.Version"))
                {
                    context.Options.Add(new Option { Id = "Hood.Version", Value = JsonConvert.SerializeObject(Versions.Current()) });
                }
                else
                {
                    Option option = context.Options.Where(o => o.Id == "Hood.Version").FirstOrDefault();
                    string setting = JsonConvert.DeserializeObject<string>(option.Value);
                    // Ensure the context is up to the latest version.
                    context.ToLatest(new Version(setting));
                }

                context.SaveChanges();
            }
        }
    }
}