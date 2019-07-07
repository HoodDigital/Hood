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

        // Views
        public DbQuery<UserProfile> UserProfiles { get; set; }

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
                        UserName = "admin@hooddigital.com",
                        Active = true
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

                if (!Options.Any(o => o.Id == typeof(AccountSettings).ToString()))
                {
                    // No new settings exist, attempt to copy from deprecated settings, or set new.
                    if (Options.Any(o => o.Id == "Hood.Settings.Account"))
                    {
                        var option = Options.Find("Hood.Settings.Account");
                        AccountSettings setting = JsonConvert.DeserializeObject<AccountSettings>(option.Value);
                        Options.Add(new Option { Id = typeof(AccountSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                    }
                    else
                        Options.Add(new Option { Id = typeof(AccountSettings).ToString(), Value = JsonConvert.SerializeObject(new AccountSettings()) });
                }

                if (!Options.Any(o => o.Id == typeof(BasicSettings).ToString()))
                {
                    // No new settings exist, attempt to copy from deprecated settings, or set new.
                    if (Options.Any(o => o.Id == "Hood.Settings.Basic"))
                    {
                        var option = Options.Find("Hood.Settings.Basic");
                        BasicSettings setting = JsonConvert.DeserializeObject<BasicSettings>(option.Value);
                        Options.Add(new Option { Id = typeof(BasicSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                    }
                    else
                        Options.Add(new Option { Id = typeof(BasicSettings).ToString(), Value = JsonConvert.SerializeObject(new BasicSettings()) });
                }

                if (!Options.Any(o => o.Id == typeof(BillingSettings).ToString()))
                {
                    // No new settings exist, attempt to copy from deprecated settings, or set new.
                    if (Options.Any(o => o.Id == "Hood.Settings.Billing"))
                    {
                        var option = Options.Find("Hood.Settings.Billing");
                        BillingSettings setting = JsonConvert.DeserializeObject<BillingSettings>(option.Value);
                        Options.Add(new Option { Id = typeof(BillingSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                    }
                    else
                        Options.Add(new Option { Id = typeof(BillingSettings).ToString(), Value = JsonConvert.SerializeObject(new BillingSettings()) });
                }

                if (!Options.Any(o => o.Id == typeof(ContactSettings).ToString()))
                {
                    // No new settings exist, attempt to copy from deprecated settings, or set new.
                    if (Options.Any(o => o.Id == "Hood.Settings.Contact"))
                    {
                        var option = Options.Find("Hood.Settings.Contact");
                        ContactSettings setting = JsonConvert.DeserializeObject<ContactSettings>(option.Value);
                        Options.Add(new Option { Id = typeof(ContactSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                    }
                    else
                        Options.Add(new Option { Id = typeof(ContactSettings).ToString(), Value = JsonConvert.SerializeObject(new ContactSettings()) });
                }

                if (!Options.Any(o => o.Id == typeof(ContentSettings).ToString()))
                {
                    // No new settings exist, attempt to copy from deprecated settings, or set new.
                    if (Options.Any(o => o.Id == "Hood.Settings.Content"))
                    {
                        var option = Options.Find("Hood.Settings.Content");
                        ContentSettings setting = JsonConvert.DeserializeObject<ContentSettings>(option.Value);
                        Options.Add(new Option { Id = typeof(ContentSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                    }
                    else
                        Options.Add(new Option { Id = typeof(ContentSettings).ToString(), Value = JsonConvert.SerializeObject(new ContentSettings()) });
                }

                if (!Options.Any(o => o.Id == typeof(ForumSettings).ToString()))
                {
                    // No new settings exist, attempt to copy from deprecated settings, or set new.
                    if (Options.Any(o => o.Id == "Hood.Settings.Forum"))
                    {
                        var option = Options.Find("Hood.Settings.Seo");
                        ForumSettings setting = JsonConvert.DeserializeObject<ForumSettings>(option.Value);
                        Options.Add(new Option { Id = typeof(ForumSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                    }
                    else
                        Options.Add(new Option { Id = typeof(ForumSettings).ToString(), Value = JsonConvert.SerializeObject(new ForumSettings()) });
                }

                if (!Options.Any(o => o.Id == typeof(IntegrationSettings).ToString()))
                {
                    // No new settings exist, attempt to copy from deprecated settings, or set new.
                    if (Options.Any(o => o.Id == "Hood.Settings.Integrations"))
                    {
                        var option = Options.Find("Hood.Settings.Integrations");
                        IntegrationSettings setting = JsonConvert.DeserializeObject<IntegrationSettings>(option.Value);
                        Options.Add(new Option { Id = typeof(IntegrationSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                    }
                    else
                        Options.Add(new Option { Id = typeof(IntegrationSettings).ToString(), Value = JsonConvert.SerializeObject(new IntegrationSettings()) });
                }

                if (!Options.Any(o => o.Id == typeof(MailSettings).ToString()))
                {
                    // No new settings exist, attempt to copy from deprecated settings, or set new.
                    if (Options.Any(o => o.Id == "Hood.Settings.Mail"))
                    {
                        var option = Options.Find("Hood.Settings.Mail");
                        MailSettings setting = JsonConvert.DeserializeObject<MailSettings>(option.Value);
                        Options.Add(new Option { Id = typeof(MailSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                    }
                    else
                        Options.Add(new Option { Id = typeof(MailSettings).ToString(), Value = JsonConvert.SerializeObject(new MailSettings()) });
                }

                if (!Options.Any(o => o.Id == typeof(MediaSettings).ToString()))
                {
                    // No new settings exist, attempt to copy from deprecated settings, or set new.
                    if (Options.Any(o => o.Id == "Hood.Settings.Media"))
                    {
                        var option = Options.Find("Hood.Settings.Media");
                        MediaSettings setting = JsonConvert.DeserializeObject<MediaSettings>(option.Value);
                        Options.Add(new Option { Id = typeof(MediaSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                    }
                    else
                        Options.Add(new Option { Id = typeof(MediaSettings).ToString(), Value = JsonConvert.SerializeObject(new MediaSettings()) });
                }

                if (!Options.Any(o => o.Id == typeof(PropertySettings).ToString()))
                {
                    // No new settings exist, attempt to copy from deprecated settings, or set new.
                    if (Options.Any(o => o.Id == "Hood.Settings.Property"))
                    {
                        var option = Options.Find("Hood.Settings.Property");
                        PropertySettings setting = JsonConvert.DeserializeObject<PropertySettings>(option.Value);
                        Options.Add(new Option { Id = typeof(PropertySettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                    }
                    else
                        Options.Add(new Option { Id = typeof(PropertySettings).ToString(), Value = JsonConvert.SerializeObject(new PropertySettings()) });
                }

                if (!Options.Any(o => o.Id == typeof(SeoSettings).ToString()))
                {
                    // No new settings exist, attempt to copy from deprecated settings, or set new.
                    if (Options.Any(o => o.Id == "Hood.Settings.Seo"))
                    {
                        var option = Options.Find("Hood.Settings.Seo");
                        SeoSettings setting = JsonConvert.DeserializeObject<SeoSettings>(option.Value);
                        Options.Add(new Option { Id = typeof(SeoSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                    }
                    else
                        Options.Add(new Option { Id = typeof(SeoSettings).ToString(), Value = JsonConvert.SerializeObject(new SeoSettings()) });
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