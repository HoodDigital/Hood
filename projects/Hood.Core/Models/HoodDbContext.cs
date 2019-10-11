using Hood.Core;
using Hood.Entities;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Linq;

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
        public DbSet<MediaDirectory> MediaDirectories { get; set; }

        // Api
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<ApiEvent> ApiEvents { get; set; }

        // Content
        public DbSet<SubscriptionProduct> SubscriptionProducts { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }

        // Options
        public DbSet<Option> Options { get; set; }

        // Content
        public DbSet<Content> Content { get; set; }
        public DbSet<ContentMeta> ContentMetadata { get; set; }
        public DbSet<ContentCategory> ContentCategories { get; set; }

        // Forums
        public DbSet<Forum> Forums { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<ForumCategory> ForumCategories { get; set; }

        // Property
        public DbSet<PropertyListing> Properties { get; set; }
        public DbSet<PropertyMeta> PropertyMetadata { get; set; }

        // Logs
        public DbSet<Log> Logs { get; set; }

        // Views
        public DbQuery<UserProfile> UserProfiles { get; set; }
        public DbQuery<SubscriptionPlan> SubscriptionPlans { get; set; }

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
            System.Collections.Generic.IEnumerable<string> applied = this.GetService<IHistoryRepository>()
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);

            System.Collections.Generic.IEnumerable<string> total = this.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(m => m.Key);

            return !total.Except(applied).Any();
        }

        public DbSet<TEntity> Set<TEntity, TKey>() where TEntity : BaseEntity<TKey>
        {
            return base.Set<TEntity>();
        }

        public virtual void Seed(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            try
            {
                Option option = Options.FirstOrDefault();
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Login failed for user") || ex.Message.Contains("permission was denied"))
                {
                    throw new StartupException("There was a problem connecting to the database.", StartupError.DatabaseConnectionFailed);
                }
                else if (ex.Message.Contains("Invalid object name"))
                {
                    if (!AllMigrationsApplied())
                    {
                        throw new StartupException("There are migrations that are not applied to the database.", StartupError.MigrationNotApplied);
                    }

                    throw new StartupException("There are migrations missing.", StartupError.MigrationMissing);
                }
            }

            if (!AllMigrationsApplied())
            {
                throw new StartupException("There are migrations that are not applied to the database.", StartupError.MigrationNotApplied);
            }

            foreach (string role in Models.Roles.All)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    IdentityResult irAdmin = roleManager.CreateAsync(new IdentityRole(role)).Result;
                }
            }

            try
            {
                string ownerEmail = Engine.SiteOwnerEmail;
                if (!Users.Any(u => u.UserName == ownerEmail))
                {
                    ApplicationUser userToInsert = new ApplicationUser
                    {
                        CompanyName = "",
                        CreatedOn = DateTime.Now,
                        FirstName = "Website",
                        LastName = "Administrator",
                        EmailConfirmed = true,
                        Anonymous = false,
                        PhoneNumber = "",
                        JobTitle = "Website Administrator",
                        LastLogOn = DateTime.Now,
                        LastLoginIP = "127.0.0.1",
                        LastLoginLocation = "UK",
                        Email = ownerEmail,
                        UserName = ownerEmail,
                        Active = true
                    };
                    IdentityResult ir = userManager.CreateAsync(userToInsert, "Password@123").Result;
                    if (!ir.Succeeded)
                    {
                        throw new StartupException("Could not create the admin user.", StartupError.AdminUserSetupError);
                    }
                }

            }
            catch (Exception)
            {
                throw new StartupException("An error occurred while loading or creating the admin user.", StartupError.AdminUserSetupError);
            }

            ApplicationUser siteAdmin = userManager.FindByEmailAsync(Engine.SiteOwnerEmail).Result;
            if (userManager.SupportsUserRole)
            {
                foreach (string role in Models.Roles.All)
                {
                    if (!userManager.IsInRoleAsync(siteAdmin, role.ToUpper()).Result)
                    {
                        IdentityResult addToRole = userManager.AddToRoleAsync(siteAdmin, role).Result;
                    }
                }
            }

            if (!Options.Any(o => o.Id == "Hood.Settings.SiteOwner"))
            {
                Options.Add(new Option { Id = "Hood.Settings.SiteOwner", Value = siteAdmin.Id });
            }

            if (!MediaDirectories.Any(o => o.Slug == MediaManager.SiteDirectorySlug && o.Type == DirectoryType.System))
            {
                MediaDirectories.Add(new MediaDirectory { DisplayName = "Default", Slug = MediaManager.SiteDirectorySlug, OwnerId = siteAdmin.Id, Type = DirectoryType.System });
            }

            if (!MediaDirectories.Any(o => o.Slug == MediaManager.UserDirectorySlug && o.Type == DirectoryType.System))
            {
                MediaDirectories.Add(new MediaDirectory { DisplayName = "User Media", Slug = MediaManager.UserDirectorySlug, OwnerId = siteAdmin.Id, Type = DirectoryType.System });
            }

            if (!MediaDirectories.Any(o => o.Slug == MediaManager.ContentDirectorySlug && o.Type == DirectoryType.System))
            {
                MediaDirectories.Add(new MediaDirectory { DisplayName = "Content", Slug = MediaManager.ContentDirectorySlug, OwnerId = siteAdmin.Id, Type = DirectoryType.System });
            }

            if (!MediaDirectories.Any(o => o.Slug == MediaManager.PropertyDirectorySlug && o.Type == DirectoryType.System))
            {
                MediaDirectories.Add(new MediaDirectory { DisplayName = "Property", Slug = MediaManager.PropertyDirectorySlug, OwnerId = siteAdmin.Id, Type = DirectoryType.System });
            }


            if (Media.Any(o => o.DirectoryId == null))
            {
                // Save any existing seeding, in case directories needed creating.
                SaveChanges();

                // Translate any un directoried images.
                MediaDirectory defaultDir = MediaDirectories.SingleOrDefault(o => o.Slug == MediaManager.SiteDirectorySlug && o.Type == DirectoryType.System);
                Media.Where(o => o.DirectoryId == null).ToList().ForEach(a => a.DirectoryId = defaultDir.Id);
                Media.Where(o => o.FileType == "directory/dir").ToList().ForEach(a => Entry(a).State = EntityState.Deleted);
            }

            if (!Options.Any(o => o.Id == "Hood.Settings.Theme"))
            {
                Options.Add(new Option { Id = "Hood.Settings.Theme", Value = JsonConvert.SerializeObject("default") });
            }

            if (!Options.Any(o => o.Id == typeof(ScheduledTaskSettings).ToString()))
            {
                Options.Add(new Option { Id = typeof(ScheduledTaskSettings).ToString(), Value = JsonConvert.SerializeObject(new ScheduledTaskSettings()) });
            }
            else
            {
                // Check all system tasks exist.
                Option option = Options.First(o => o.Id == typeof(ScheduledTaskSettings).ToString());
                ScheduledTaskSettings sheduledTaskSettings = JsonConvert.DeserializeObject<ScheduledTaskSettings>(option.Value);
                sheduledTaskSettings.CheckTasks();
                option.Value = JsonConvert.SerializeObject(sheduledTaskSettings);
            }

            if (!Options.Any(o => o.Id == typeof(AccountSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Account"))
                {
                    Option option = Options.Find("Hood.Settings.Account");
                    AccountSettings setting = JsonConvert.DeserializeObject<AccountSettings>(option.Value);
                    Options.Add(new Option { Id = typeof(AccountSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                }
                else
                {
                    Options.Add(new Option { Id = typeof(AccountSettings).ToString(), Value = JsonConvert.SerializeObject(new AccountSettings()) });
                }
            }

            if (!Options.Any(o => o.Id == typeof(BasicSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Basic"))
                {
                    Option option = Options.Find("Hood.Settings.Basic");
                    BasicSettings setting = JsonConvert.DeserializeObject<BasicSettings>(option.Value);
                    Options.Add(new Option { Id = typeof(BasicSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                }
                else
                {
                    Options.Add(new Option { Id = typeof(BasicSettings).ToString(), Value = JsonConvert.SerializeObject(new BasicSettings()) });
                }
            }

            if (!Options.Any(o => o.Id == typeof(BillingSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Billing"))
                {
                    Option option = Options.Find("Hood.Settings.Billing");
                    BillingSettings setting = JsonConvert.DeserializeObject<BillingSettings>(option.Value);
                    Options.Add(new Option { Id = typeof(BillingSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                }
                else
                {
                    Options.Add(new Option { Id = typeof(BillingSettings).ToString(), Value = JsonConvert.SerializeObject(new BillingSettings()) });
                }
            }

            if (!Options.Any(o => o.Id == typeof(ContactSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Contact"))
                {
                    Option option = Options.Find("Hood.Settings.Contact");
                    ContactSettings setting = JsonConvert.DeserializeObject<ContactSettings>(option.Value);
                    Options.Add(new Option { Id = typeof(ContactSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                }
                else
                {
                    Options.Add(new Option { Id = typeof(ContactSettings).ToString(), Value = JsonConvert.SerializeObject(new ContactSettings()) });
                }
            }

            if (!Options.Any(o => o.Id == typeof(ContentSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Content"))
                {
                    Option option = Options.Find("Hood.Settings.Content");
                    ContentSettings setting = JsonConvert.DeserializeObject<ContentSettings>(option.Value);
                    Options.Add(new Option { Id = typeof(ContentSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                }
                else
                {
                    Options.Add(new Option { Id = typeof(ContentSettings).ToString(), Value = JsonConvert.SerializeObject(new ContentSettings()) });
                }
            }

            if (!Options.Any(o => o.Id == typeof(ForumSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Forum"))
                {
                    Option option = Options.Find("Hood.Settings.Seo");
                    ForumSettings setting = JsonConvert.DeserializeObject<ForumSettings>(option.Value);
                    Options.Add(new Option { Id = typeof(ForumSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                }
                else
                {
                    Options.Add(new Option { Id = typeof(ForumSettings).ToString(), Value = JsonConvert.SerializeObject(new ForumSettings()) });
                }
            }

            if (!Options.Any(o => o.Id == typeof(IntegrationSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Integrations"))
                {
                    Option option = Options.Find("Hood.Settings.Integrations");
                    IntegrationSettings setting = JsonConvert.DeserializeObject<IntegrationSettings>(option.Value);
                    Options.Add(new Option { Id = typeof(IntegrationSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                }
                else
                {
                    Options.Add(new Option { Id = typeof(IntegrationSettings).ToString(), Value = JsonConvert.SerializeObject(new IntegrationSettings()) });
                }
            }

            if (!Options.Any(o => o.Id == typeof(MailSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Mail"))
                {
                    Option option = Options.Find("Hood.Settings.Mail");
                    MailSettings setting = JsonConvert.DeserializeObject<MailSettings>(option.Value);
                    Options.Add(new Option { Id = typeof(MailSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                }
                else
                {
                    Options.Add(new Option { Id = typeof(MailSettings).ToString(), Value = JsonConvert.SerializeObject(new MailSettings()) });
                }
            }

            if (!Options.Any(o => o.Id == typeof(MediaSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Media"))
                {
                    Option option = Options.Find("Hood.Settings.Media");
                    MediaSettings setting = JsonConvert.DeserializeObject<MediaSettings>(option.Value);
                    Options.Add(new Option { Id = typeof(MediaSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                }
                else
                {
                    Options.Add(new Option { Id = typeof(MediaSettings).ToString(), Value = JsonConvert.SerializeObject(new MediaSettings()) });
                }
            }

            if (!Options.Any(o => o.Id == typeof(PropertySettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Property"))
                {
                    Option option = Options.Find("Hood.Settings.Property");
                    PropertySettings setting = JsonConvert.DeserializeObject<PropertySettings>(option.Value);
                    Options.Add(new Option { Id = typeof(PropertySettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                }
                else
                {
                    Options.Add(new Option { Id = typeof(PropertySettings).ToString(), Value = JsonConvert.SerializeObject(new PropertySettings()) });
                }
            }

            if (!Options.Any(o => o.Id == typeof(SeoSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Seo"))
                {
                    Option option = Options.Find("Hood.Settings.Seo");
                    SeoSettings setting = JsonConvert.DeserializeObject<SeoSettings>(option.Value);
                    Options.Add(new Option { Id = typeof(SeoSettings).ToString(), Value = JsonConvert.SerializeObject(setting) });
                }
                else
                {
                    Options.Add(new Option { Id = typeof(SeoSettings).ToString(), Value = JsonConvert.SerializeObject(new SeoSettings()) });
                }
            }
            if (!Options.Any(o => o.Id == "Hood.Api.SystemPrivateKey"))
            {
                string guid = Guid.NewGuid().ToString();
                string key = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(guid));
                Options.Add(new Option { Id = "Hood.Api.SystemPrivateKey", Value = JsonConvert.SerializeObject(key) });
            }

            // Mark the database with the current version of Hood.
            if (!Options.Any(o => o.Id == "Hood.Version"))
            {
                Options.Add(new Option { Id = "Hood.Version", Value = Engine.Version });
            }
            else
            {
                Option option = Options.SingleOrDefault(o => o.Id == "Hood.Version");
                option.Value = Engine.Version;
            }

            SaveChanges();
        }

    }
}