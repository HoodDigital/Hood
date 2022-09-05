using Hood.Contexts;
using Hood.Core;
using Hood.Entities;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
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

namespace Hood.Models
{
    public class HoodDbContext : DbContext
    {
        public HoodDbContext(DbContextOptions<HoodDbContext> options)
            : base(options)
        {
        }

        // Media
        public DbSet<MediaObject> Media { get; set; }
        public DbSet<MediaDirectory> MediaDirectories { get; set; }

        // Options
        public DbSet<Option> Options { get; set; }

        // Logs
        public DbSet<Log> Logs { get; set; }

        // Auth0
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Option>().ToTable("HoodOptions");
            builder.Entity<Log>().ToTable("HoodLogs");

            // Media
            builder.Entity<MediaObject>().ToTable("HoodMedia");
            builder.Entity<MediaObject>().Property(b => b.Path).HasColumnName("Directory");
            builder.Entity<MediaDirectory>().ToTable("HoodMediaDirectories");
            builder.Entity<MediaDirectory>().HasOne(m => m.Parent).WithMany(m => m.Children).HasForeignKey(m => m.ParentId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<MediaObject>().HasOne(m => m.Directory).WithMany(m => m.Media).HasForeignKey(m => m.DirectoryId).OnDelete(DeleteBehavior.Restrict);
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

        public async virtual Task Seed(IHoodIdentityContext identityContext)
        {
            await CheckDatabaseIsInitialisedAsync();

            var siteAdmin = await identityContext.GetSiteAdmin();

            var siteOwnerRef = await Options.SingleOrDefaultAsync(o => o.Id == "Hood.Settings.SiteOwner");
            if (siteOwnerRef == null)
            {
                Options.Add(new Option
                {
                    Id = "Hood.Settings.SiteOwner",
                    Value = siteAdmin.Id
                });
            }
            else
            {
                siteOwnerRef.Value = siteAdmin.Id;
            }
            await SaveChangesAsync();
            await SetupHoodMediaDirectoriesAsync(siteAdmin.Id);
            await InitialiseHoodSettingsAsync();
            await UpdateLegacyMediaDirectoryReferencesAsync();
            await SetDatabaseVersionAsync();
        }

        public virtual async Task CheckDatabaseIsInitialisedAsync()
        {
            try
            {
                Option option = await Options.FirstOrDefaultAsync();
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Login failed for user") || ex.Message.Contains("permission was denied"))
                {
                    throw new StartupException("There was a problem connecting to the database.", ex, StartupError.DatabaseConnectionFailed);
                }
                else if (ex.Message.Contains("Invalid object name"))
                {
                    throw new StartupException("There are migrations missing.", ex, StartupError.MigrationMissing);
                }
            }
        }

        protected virtual async Task SetupHoodMediaDirectoriesAsync(string siteAdminId)
        {
            if (!MediaDirectories.Any(o => o.Slug == MediaManager.SiteDirectorySlug && o.Type == DirectoryType.System))
            {
                MediaDirectories.Add(new MediaDirectory { DisplayName = "Default", Slug = MediaManager.SiteDirectorySlug, OwnerId = siteAdminId, Type = DirectoryType.System });
            }

            if (!MediaDirectories.Any(o => o.Slug == MediaManager.UserDirectorySlug && o.Type == DirectoryType.System))
            {
                MediaDirectories.Add(new MediaDirectory { DisplayName = "User Media", Slug = MediaManager.UserDirectorySlug, OwnerId = siteAdminId, Type = DirectoryType.System });
            }

            if (!MediaDirectories.Any(o => o.Slug == MediaManager.ContentDirectorySlug && o.Type == DirectoryType.System))
            {
                MediaDirectories.Add(new MediaDirectory { DisplayName = "Content", Slug = MediaManager.ContentDirectorySlug, OwnerId = siteAdminId, Type = DirectoryType.System });
            }

            if (!MediaDirectories.Any(o => o.Slug == MediaManager.PropertyDirectorySlug && o.Type == DirectoryType.System))
            {
                MediaDirectories.Add(new MediaDirectory { DisplayName = "Property", Slug = MediaManager.PropertyDirectorySlug, OwnerId = siteAdminId, Type = DirectoryType.System });
            }
            await SaveChangesAsync();
        }

        protected virtual async Task InitialiseHoodSettingsAsync()
        {
            if (!Options.Any(o => o.Id == "Hood.Settings.Theme"))
            {
                Options.Add(new Option { Id = "Hood.Settings.Theme", Value = JsonConvert.SerializeObject("default") });
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
            await SaveChangesAsync();
        }

        protected virtual async Task UpdateLegacyMediaDirectoryReferencesAsync()
        {
            if (Media.Any(o => o.DirectoryId == null))
            {
                // Save any existing seeding, in case directories needed creating.
                await SaveChangesAsync();

                // Translate any un directoried images.
                MediaDirectory defaultDir = MediaDirectories.AsNoTracking().SingleOrDefault(o => o.Slug == MediaManager.SiteDirectorySlug && o.Type == DirectoryType.System);
                MediaDirectory contentDir = MediaDirectories.AsNoTracking().SingleOrDefault(o => o.Slug == MediaManager.ContentDirectorySlug && o.Type == DirectoryType.System);
                MediaDirectory propertyDir = MediaDirectories.AsNoTracking().SingleOrDefault(o => o.Slug == MediaManager.PropertyDirectorySlug && o.Type == DirectoryType.System);
                Media.Where(o => o.FileType == "directory/dir").ToList().ForEach(a => Entry(a).State = EntityState.Deleted);
                try
                {
                    if (Media.Any(o => o.DirectoryId == null))
                    {
                        SaveChanges();

                        string commandText = "UPDATE HoodMedia SET DirectoryId = @DirectoryId WHERE DirectoryId IS NULL AND Directory = 'Property'";
                        SqlParameter sqlParameter = new SqlParameter("@DirectoryId", propertyDir.Id);
                        int affectedRows = Database.ExecuteSqlRaw(commandText, sqlParameter);

                        Option option = Options.Find(typeof(ContentSettings).ToString());
                        var contentSettings = JsonConvert.DeserializeObject<ContentSettings>(option.Value);
                        foreach (var type in contentSettings.Types)
                        {
                            commandText = "UPDATE HoodMedia SET DirectoryId = @DirectoryId WHERE DirectoryId IS NULL AND Directory = '@Directory'";
                            sqlParameter = new SqlParameter("@DirectoryId", contentDir.Id);
                            SqlParameter sqlParameterType = new SqlParameter("@Directory", type.TypeName);
                            affectedRows = Database.ExecuteSqlRaw(commandText, sqlParameter, sqlParameterType);
                        }

                        commandText = "UPDATE HoodMedia SET DirectoryId = @DirectoryId WHERE DirectoryId IS NULL";
                        sqlParameter = new SqlParameter("@DirectoryId", defaultDir.Id);
                        affectedRows = Database.ExecuteSqlRaw(commandText, sqlParameter);

                    }
                }
                catch (SqlException ex)
                {
                    throw new StartupException("Error updating the media entries.", ex, StartupError.DatabaseMediaError);
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("Timeout"))
                    {
                        throw new StartupException("Error updating the media entries.", ex, StartupError.DatabaseMediaError);
                    }
                }
            }
        }

        protected virtual async Task SetDatabaseVersionAsync()
        {
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

            await SaveChangesAsync();
        }
    }
}