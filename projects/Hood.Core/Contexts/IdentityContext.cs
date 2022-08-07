using Hood.Contexts;
using Hood.Core;
using Hood.Entities;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Contexts
{

    public class IdentityContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, IHoodIdentityContext
    {
        public IdentityContext(DbContextOptions options)
            : base(options)
        { }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserProfileView<IdentityRole>> UserProfileViews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(
                typeBuilder =>
                {
                    typeBuilder.ToTable("AspNetUsers");
                    typeBuilder.Property(o => o.UserName).HasColumnName("UserName");
                    typeBuilder.Property(o => o.EmailConfirmed).HasColumnName("EmailConfirmed");
                    typeBuilder.Property(o => o.Email).HasColumnName("Email");
                    typeBuilder.Property(o => o.PhoneNumberConfirmed).HasColumnName("PhoneNumberConfirmed");
                    typeBuilder.Property(o => o.PhoneNumber).HasColumnName("PhoneNumber");
                    typeBuilder.Property(o => o.Active).HasColumnName("Active");
                    typeBuilder.Property(o => o.BillingAddressJson).HasColumnName("BillingAddressJson");
                    typeBuilder.Property(o => o.DeliveryAddressJson).HasColumnName("DeliveryAddressJson");
                    typeBuilder.Property(o => o.AvatarJson).HasColumnName("AvatarJson");
                    typeBuilder.Property(o => o.CreatedOn).HasColumnName("CreatedOn");
                    typeBuilder.Property(o => o.LastLogOn).HasColumnName("LastLogOn");
                    typeBuilder.Property(o => o.LastLoginIP).HasColumnName("LastLoginIP");
                    typeBuilder.Property(o => o.LastLoginLocation).HasColumnName("LastLoginLocation");

                    typeBuilder.HasOne(o => o.UserProfile).WithOne().HasForeignKey<UserProfile>(o => o.Id);
                });

            builder.Entity<UserProfile>(
                typeBuilder =>
                {
                    typeBuilder.ToTable("AspNetUsers");
                    typeBuilder.Property(o => o.UserName).HasColumnName("UserName");
                    typeBuilder.Property(o => o.EmailConfirmed).HasColumnName("EmailConfirmed");
                    typeBuilder.Property(o => o.Email).HasColumnName("Email");
                    typeBuilder.Property(o => o.PhoneNumberConfirmed).HasColumnName("PhoneNumberConfirmed");
                    typeBuilder.Property(o => o.PhoneNumber).HasColumnName("PhoneNumber");
                    typeBuilder.Property(o => o.Active).HasColumnName("Active");
                    typeBuilder.Property(o => o.BillingAddressJson).HasColumnName("BillingAddressJson");
                    typeBuilder.Property(o => o.DeliveryAddressJson).HasColumnName("DeliveryAddressJson");
                    typeBuilder.Property(o => o.AvatarJson).HasColumnName("AvatarJson");
                    typeBuilder.Property(o => o.CreatedOn).HasColumnName("CreatedOn");
                    typeBuilder.Property(o => o.LastLogOn).HasColumnName("LastLogOn");
                    typeBuilder.Property(o => o.LastLoginIP).HasColumnName("LastLoginIP");
                    typeBuilder.Property(o => o.LastLoginLocation).HasColumnName("LastLoginLocation");
                });

            builder.Entity<UserProfileView<IdentityRole>>().HasNoKey().ToView("HoodUserProfiles");
        }

        public async Task<IHoodIdentity> GetSiteAdmin()
        {
            try
            {
                IPasswordAccountRepository repo = Engine.Services.Resolve<IPasswordAccountRepository>();

                string ownerEmail = Engine.SiteOwnerEmail;
                if (!Users.Any(u => u.UserName == ownerEmail))
                {
                    ApplicationUser userToInsert = new ApplicationUser
                    {
                        CreatedOn = DateTime.UtcNow,
                        EmailConfirmed = true,
                        PhoneNumber = "",
                        LastLogOn = DateTime.UtcNow,
                        LastLoginIP = "127.0.0.1",
                        LastLoginLocation = "UK",
                        Email = ownerEmail,
                        UserName = ownerEmail,
                        Active = true,
                        UserProfile = new UserProfile
                        {
                            FirstName = "Website",
                            LastName = "Administrator",
                            JobTitle = "Website Administrator",
                            Anonymous = false
                        }
                    };
                    var keygen = new KeyGenerator();
                    var password = keygen.Generate(16);


                    IdentityResult ir = await repo.CreateAsync(userToInsert, password);

                    var tempPasswordFile = Engine.Services.Resolve<IWebHostEnvironment>().ContentRootPath + "/temp.txt";
                    if (File.Exists(tempPasswordFile))
                    {
                        File.Delete(tempPasswordFile);
                    }
                    using (StreamWriter file = new(tempPasswordFile))
                    {
                        await file.WriteLineAsync(password);
                    }
                }

                ApplicationUser siteAdmin = await repo.GetUserByEmailAsync(Engine.SiteOwnerEmail);
                return siteAdmin;

            }
            catch (Exception ex)
            {
                throw new StartupException("An error occurred while loading or creating the admin user.", ex, StartupError.AdminUserSetupError);
            }
        }

    }
}