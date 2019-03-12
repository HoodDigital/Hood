using Hood.Entities;
using Hood.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Models
{
    /// <summary>
    /// In order to use the Hood functionality on the site, you must call the 'void Configure(DbContextOptionsBuilder optionsBuilder)' function in the OnConfiguring() method, and then call the 'void CreateModels(ModelBuilder builder)' function in the OnModelCreating() method.
    /// </summary>
    /// <param name="optionsBuilder"></param>
    public class HoodDbContext : IdentityDbContext<ApplicationUser>, IHoodDbContext
    {
        public HoodDbContext(DbContextOptions<HoodDbContext> options)
            : base(options)
        {
        }

        // Identity
        public DbSet<UserAccessCode> AccessCodes { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<MediaObject> Media { get; set; }

        // Api
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<ApiEvent> ApiEvents { get; set; }

        // Content
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }

        // Options
        public DbSet<Option> Options { get; set; }

        // Content
        public DbSet<Content> Content { get; set; }
        public DbSet<ContentCategory> ContentCategories { get; set; }
        public DbSet<ContentTag> ContentTags { get; set; }

        // Forums
        public DbSet<Forum> Forums { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<ForumCategory> ForumCategories { get; set; }

        // Property
        public DbSet<PropertyListing> Properties { get; set; }

        // Logs
        public DbSet<Log> Logs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureForHood();
        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.CreateHoodModels();
        }

        public bool AllMigrationsApplied()
        {
            var applied = this.GetService<IHistoryRepository>()
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);

            var total = this.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(m => m.Key);

            return !total.Except(applied).Any();
        }

        public virtual void Seed(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (AllMigrationsApplied())
            {
                foreach (var role in Models.Roles.All)
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                    {
                        IdentityResult irAdmin = roleManager.CreateAsync(new IdentityRole(role)).Result;
                    }
                }
                if (!Users.Any(u => u.UserName == "admin@hooddigital.com"))
                {
                    var userToInsert = new ApplicationUser
                    {
                        CompanyName = "Hood",
                        CreatedOn = DateTime.Now,
                        FirstName = "Hood",
                        LastName = "Digital",
                        EmailConfirmed = true,
                        Anonymous = false,
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
                            foreach (var role in Models.Roles.All)
                            {
                                if (!userManager.IsInRoleAsync(user, role.ToUpper()).Result)
                                {
                                    IdentityResult addToRole = userManager.AddToRoleAsync(user, role).Result;
                                }
                            }
                        }
                    }
                }
                if (!Options.Any(o => o.Id == "Hood.Settings.Theme"))
                {
                    Options.Add(new Option { Id = "Hood.Settings.Theme", Value = JsonConvert.SerializeObject("default") });
                }

                if (!Options.Any(o => o.Id == "Hood.Settings.Basic"))
                {
                    Options.Add(new Option { Id = "Hood.Settings.Basic", Value = JsonConvert.SerializeObject(new ContactSettings()) });
                }

                if (!Options.Any(o => o.Id == "Hood.Settings.Contact"))
                {
                    Options.Add(new Option { Id = "Hood.Settings.Contact", Value = JsonConvert.SerializeObject(new ContactSettings()) });
                }

                if (!Options.Any(o => o.Id == "Hood.Settings.Billing"))
                {
                    Options.Add(new Option { Id = "Hood.Settings.Billing", Value = JsonConvert.SerializeObject(new BillingSettings()) });
                }

                if (!Options.Any(o => o.Id == "Hood.Settings.Media"))
                {
                    Options.Add(new Option { Id = "Hood.Settings.Media", Value = JsonConvert.SerializeObject(new MediaSettings()) });
                }

                if (!Options.Any(o => o.Id == "Hood.Settings.Content"))
                {
                    Options.Add(new Option { Id = "Hood.Settings.Content", Value = JsonConvert.SerializeObject(new ContentSettings()) });
                }

                if (!Options.Any(o => o.Id == "Hood.Settings.Property"))
                {
                    Options.Add(new Option { Id = "Hood.Settings.Property", Value = JsonConvert.SerializeObject(new PropertySettings()) });
                }

                if (!Options.Any(o => o.Id == "Hood.Settings.Seo"))
                {
                    Options.Add(new Option { Id = "Hood.Settings.Seo", Value = JsonConvert.SerializeObject(new SeoSettings()) });
                }

                if (!Options.Any(o => o.Id == "Hood.Api.SystemPrivateKey"))
                {
                    var generator = new KeyGenerator(true, true, true, false);
                    var key = generator.Generate(24);
                    Options.Add(new Option { Id = "Hood.Api.SystemPrivateKey", Value = JsonConvert.SerializeObject(key) });
                }

                SaveChanges();
            }
        }

        public DbSet<TEntity> Set<TEntity, TKey>() where TEntity : BaseEntity<TKey>
        {
            return base.Set<TEntity>();
        }
    }
}