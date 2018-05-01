using Hood.Models.Payments;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Hood.Models
{
    public static class DbContextExtensions
    {
        public static void ConfigureForHood(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging(true);
        }
        public static void CreateHoodModels(this ModelBuilder builder)
        {
            // Identity
            builder.Entity<Option>().ToTable("HoodOptions");
            builder.Entity<MediaObject>().ToTable("HoodMedia");
            builder.Entity<Address>().ToTable("HoodAddresses");
            builder.Entity<UserAccessCode>().ToTable("AspNetUserAccessCodes");
            builder.Entity<Address>().Property(a => a.Latitude).HasDefaultValue(0.0).HasDefaultValueSql("0.0");
            builder.Entity<Address>().Property(a => a.Longitude).HasDefaultValue(0.0).HasDefaultValueSql("0.0");
            builder.Entity<Address>().HasOne(up => up.User).WithMany(add => add.Addresses).HasForeignKey(au => au.UserId);
            builder.Entity<UserAccessCode>().HasOne(up => up.User).WithMany(add => add.AccessCodes).HasForeignKey(au => au.UserId);

            // Subscriptions
            builder.Entity<Subscription>().ToTable("HoodSubscriptions");
            builder.Entity<SubscriptionFeature>().ToTable("HoodSubscriptionFeatures");
            builder.Entity<UserSubscription>().ToTable("HoodUserSubscriptions");
            builder.Entity<Subscription>().Property("Category").HasColumnName("Colour");
            builder.Entity<UserSubscription>().Property("Id").HasColumnName("UserSubscriptionId");
            builder.Entity<UserSubscription>().HasOne(pt => pt.User).WithMany(p => p.Subscriptions).HasForeignKey(pt => pt.UserId);
            builder.Entity<UserSubscription>().HasOne(pt => pt.Subscription).WithMany(t => t.Users).HasForeignKey(pt => pt.SubscriptionId);

            // Content
            builder.Entity<Content>().ToTable("HoodContent");
            builder.Entity<Content>().HasOne(c => c.Author).WithMany(up => up.Content).HasForeignKey(c => c.AuthorId);

            // Content Tags
            builder.Entity<ContentTag>().ToTable("HoodContentTags");
            builder.Entity<ContentTagJoin>().ToTable("HoodContentTagJoins");
            builder.Entity<ContentTagJoin>().HasKey(t => new { t.ContentId, t.TagId });
            builder.Entity<ContentTagJoin>().HasOne(pt => pt.Tag).WithMany(p => p.Content).HasForeignKey(pt => pt.TagId);
            builder.Entity<ContentTagJoin>().HasOne(pt => pt.Content).WithMany(t => t.Tags).HasForeignKey(pt => pt.ContentId);
            builder.Entity<ContentMedia>().ToTable("HoodContentMedia");
            builder.Entity<ContentMedia>().HasOne(up => up.Content).WithMany(t => t.Media).HasForeignKey(au => au.ContentId);

            // Categories
            builder.Entity<ContentCategory>().ToTable("HoodContentCategories");
            builder.Entity<ContentCategory>().Property("Id").HasColumnName("ContentCategoryId");
            builder.Entity<ContentCategoryJoin>().ToTable("HoodContentCategoryJoins");
            builder.Entity<ContentCategoryJoin>().HasKey(t => new { t.ContentId, t.CategoryId });
            builder.Entity<ContentCategoryJoin>().HasOne(pt => pt.Category).WithMany(p => p.Content).HasForeignKey(pt => pt.CategoryId);
            builder.Entity<ContentCategoryJoin>().HasOne(pt => pt.Content).WithMany(t => t.Categories).HasForeignKey(pt => pt.ContentId);

            // Content Metadata
            builder.Entity<ContentMeta>().ToTable("HoodContentMetadata");
            builder.Entity<ContentMeta>().HasAlternateKey(ol => new { ol.ContentId, ol.Name });
            builder.Entity<ContentMeta>().HasOne(c => c.Content).WithMany(cc => cc.Metadata).HasForeignKey(au => au.ContentId);

            // Property
            builder.Entity<PropertyListing>().ToTable("HoodProperties");
            builder.Entity<PropertyListing>().Property(a => a.Latitude).HasDefaultValue(0.0).HasDefaultValueSql("0.0");
            builder.Entity<PropertyListing>().Property(a => a.Longitude).HasDefaultValue(0.0).HasDefaultValueSql("0.0");
            builder.Entity<PropertyListing>().HasOne(c => c.Agent).WithMany(up => up.Properties).HasForeignKey(c => c.AgentId);

            builder.Entity<PropertyMeta>().ToTable("HoodPropertyMetadata");
            builder.Entity<PropertyMeta>().HasAlternateKey(ol => new { ol.PropertyId, ol.Name });
            builder.Entity<PropertyMeta>().HasOne(c => c.Property).WithMany(cc => cc.Metadata).HasForeignKey(au => au.PropertyId);

            builder.Entity<PropertyMedia>().ToTable("HoodPropertyMedia");
            builder.Entity<PropertyMedia>().HasOne(up => up.Property).WithMany(t => t.Media).HasForeignKey(au => au.PropertyId);

            builder.Entity<PropertyFloorplan>().ToTable("HoodPropertyFloorplans");
            builder.Entity<PropertyFloorplan>().HasOne(up => up.Property).WithMany(t => t.FloorPlans).HasForeignKey(au => au.PropertyId);
        }
        public static void RegisterSagePayBackingFields<T>(this ModelBuilder builder) where T : SagePayTransaction
        {
            builder.Entity<T>().Property<string>("PaymentMethodJson").HasField("_PaymentMethodJson").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("AmountJson").HasField("_AmountJson").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("BillingAddressJson").HasField("_BillingAddressJson").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("ShippingAddressJson").HasField("_ShippingAddressJson").UsePropertyAccessMode(PropertyAccessMode.Field);
        }
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
                context.SaveChanges();
            }
        }
    }
}