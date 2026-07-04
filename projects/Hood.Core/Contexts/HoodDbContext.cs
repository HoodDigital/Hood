using System.Linq;
using System.Threading.Tasks;
using Hood.Contexts;
using Hood.Core;
using Hood.Entities;
using Hood.Enums;
using Hood.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json;

namespace Hood.Models
{
    public class HoodDbContext : DbContext
    {
        public HoodDbContext(DbContextOptions<HoodDbContext> options)
            : base(options) { }

        // Media
        public DbSet<MediaObject> Media { get; set; }
        public DbSet<MediaDirectory> MediaDirectories { get; set; }

        // Options
        public DbSet<Option> Options { get; set; }

        // Logs
        public DbSet<Log> Logs { get; set; }

        // NOTE: UserProfile is owned by the identity contexts (IdentityContext / Auth0IdentityContext),
        // where it is mapped as a shared-table 1:1 onto AspNetUsers. HoodDbContext used to expose a
        // DbSet<UserProfile> that mapped to a standalone "UserProfiles" table — that legacy mapping was
        // unused (nothing read or wrote _hoodDb.UserProfiles) and is dropped in v7. AspNetUsers is the
        // single authoritative store. The v6->v7 update script carries DROP TABLE UserProfiles for upgraders.

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Option>().ToTable("HoodOptions");
            builder.Entity<Log>().ToTable("HoodLogs");
            // UserId bounded + indexed to converge with the shape upgraded DBs carry.
            builder.Entity<Log>().Property(l => l.UserId).HasMaxLength(450);
            builder.Entity<Log>().HasIndex(l => l.UserId);

            // Media
            builder.Entity<MediaObject>().ToTable("HoodMedia");
            builder.Entity<MediaObject>().Property(b => b.Path).HasColumnName("Directory");
            builder.Entity<MediaDirectory>().ToTable("HoodMediaDirectories");
            builder
                .Entity<MediaDirectory>()
                .HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            builder
                .Entity<MediaObject>()
                .HasOne(m => m.Directory)
                .WithMany(m => m.Media)
                .HasForeignKey(m => m.DirectoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Log.User is the only path that reaches the identity entities from HoodDbContext. The nav is
            // unused (nothing .Include()s it) and pulling it in made EF generate standalone ApplicationUser
            // + UserProfile tables that shadow the authoritative AspNetUsers. Ignore it so HoodDb owns only
            // its own tables; Log.UserId stays as a plain column (logs intentionally don't hard-FK users).
            builder.Entity<Log>().Ignore(l => l.User);
        }

        public DbSet<TEntity> Set<TEntity, TKey>()
            where TEntity : BaseEntity<TKey>
        {
            return base.Set<TEntity>();
        }

        public virtual async Task Seed(IHoodIdentityContext identityContext)
        {
            await CheckDatabaseIsInitialisedAsync();

            var siteAdmin = await identityContext.GetSiteAdmin();

            var siteOwnerRef = await Options.SingleOrDefaultAsync(o =>
                o.Id == "Hood.Settings.SiteOwner"
            );
            if (siteOwnerRef == null)
            {
                Options.Add(new Option { Id = "Hood.Settings.SiteOwner", Value = siteAdmin.Id });
            }
            else
            {
                siteOwnerRef.Value = siteAdmin.Id;
            }

            // Persists the administrator's email as the runtime source of truth for
            // Engine.SiteOwnerEmail, so consumers don't have to hardcode Hood:SuperAdminEmail.
            var siteOwnerEmailRef = await Options.SingleOrDefaultAsync(o =>
                o.Id == "Hood.Settings.SuperAdminEmail"
            );
            string encodedSiteOwnerEmail = JsonConvert.SerializeObject(siteAdmin.Email);
            if (siteOwnerEmailRef == null)
            {
                Options.Add(
                    new Option
                    {
                        Id = "Hood.Settings.SuperAdminEmail",
                        Value = encodedSiteOwnerEmail,
                    }
                );
            }
            else
            {
                siteOwnerEmailRef.Value = encodedSiteOwnerEmail;
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
                // Probe read — succeeds only when the database/tables exist.
                await Options.FirstOrDefaultAsync();
            }
            catch (SqlException ex)
            {
                if (
                    ex.Message.Contains("Login failed for user")
                    || ex.Message.Contains("permission was denied")
                )
                {
                    throw new StartupException(
                        "There was a problem connecting to the database.",
                        ex,
                        StartupError.DatabaseConnectionFailed
                    );
                }
                else if (ex.Message.Contains("Invalid object name"))
                {
                    throw new StartupException(
                        "There are migrations missing.",
                        ex,
                        StartupError.MigrationMissing
                    );
                }
            }
        }

        protected virtual async Task SetupHoodMediaDirectoriesAsync(string siteAdminId)
        {
            if (
                !MediaDirectories.Any(o =>
                    o.Slug == MediaManager.SiteDirectorySlug && o.Type == DirectoryType.System
                )
            )
            {
                MediaDirectories.Add(
                    new MediaDirectory
                    {
                        DisplayName = "Default",
                        Slug = MediaManager.SiteDirectorySlug,
                        OwnerId = siteAdminId,
                        Type = DirectoryType.System,
                    }
                );
            }

            if (
                !MediaDirectories.Any(o =>
                    o.Slug == MediaManager.UserDirectorySlug && o.Type == DirectoryType.System
                )
            )
            {
                MediaDirectories.Add(
                    new MediaDirectory
                    {
                        DisplayName = "User Media",
                        Slug = MediaManager.UserDirectorySlug,
                        OwnerId = siteAdminId,
                        Type = DirectoryType.System,
                    }
                );
            }

            if (
                !MediaDirectories.Any(o =>
                    o.Slug == MediaManager.ContentDirectorySlug && o.Type == DirectoryType.System
                )
            )
            {
                MediaDirectories.Add(
                    new MediaDirectory
                    {
                        DisplayName = "Content",
                        Slug = MediaManager.ContentDirectorySlug,
                        OwnerId = siteAdminId,
                        Type = DirectoryType.System,
                    }
                );
            }

            if (
                !MediaDirectories.Any(o =>
                    o.Slug == MediaManager.PropertyDirectorySlug && o.Type == DirectoryType.System
                )
            )
            {
                MediaDirectories.Add(
                    new MediaDirectory
                    {
                        DisplayName = "Property",
                        Slug = MediaManager.PropertyDirectorySlug,
                        OwnerId = siteAdminId,
                        Type = DirectoryType.System,
                    }
                );
            }
            await SaveChangesAsync();
        }

        protected virtual async Task InitialiseHoodSettingsAsync()
        {
            if (!Options.Any(o => o.Id == "Hood.Settings.Theme"))
            {
                Options.Add(
                    new Option
                    {
                        Id = "Hood.Settings.Theme",
                        Value = JsonConvert.SerializeObject("default"),
                    }
                );
            }

            if (!Options.Any(o => o.Id == typeof(AccountSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Account"))
                {
                    Option option = Options.Find("Hood.Settings.Account");
                    AccountSettings setting = JsonConvert.DeserializeObject<AccountSettings>(
                        option.Value
                    );
                    Options.Add(
                        new Option
                        {
                            Id = typeof(AccountSettings).ToString(),
                            Value = JsonConvert.SerializeObject(setting),
                        }
                    );
                }
                else
                {
                    Options.Add(
                        new Option
                        {
                            Id = typeof(AccountSettings).ToString(),
                            Value = JsonConvert.SerializeObject(new AccountSettings()),
                        }
                    );
                }
            }

            if (!Options.Any(o => o.Id == typeof(BasicSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Basic"))
                {
                    Option option = Options.Find("Hood.Settings.Basic");
                    BasicSettings setting = JsonConvert.DeserializeObject<BasicSettings>(
                        option.Value
                    );
                    Options.Add(
                        new Option
                        {
                            Id = typeof(BasicSettings).ToString(),
                            Value = JsonConvert.SerializeObject(setting),
                        }
                    );
                }
                else
                {
                    Options.Add(
                        new Option
                        {
                            Id = typeof(BasicSettings).ToString(),
                            Value = JsonConvert.SerializeObject(new BasicSettings()),
                        }
                    );
                }
            }

            if (!Options.Any(o => o.Id == typeof(ContactSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Contact"))
                {
                    Option option = Options.Find("Hood.Settings.Contact");
                    ContactSettings setting = JsonConvert.DeserializeObject<ContactSettings>(
                        option.Value
                    );
                    Options.Add(
                        new Option
                        {
                            Id = typeof(ContactSettings).ToString(),
                            Value = JsonConvert.SerializeObject(setting),
                        }
                    );
                }
                else
                {
                    Options.Add(
                        new Option
                        {
                            Id = typeof(ContactSettings).ToString(),
                            Value = JsonConvert.SerializeObject(new ContactSettings()),
                        }
                    );
                }
            }

            if (!Options.Any(o => o.Id == typeof(ContentSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Content"))
                {
                    Option option = Options.Find("Hood.Settings.Content");
                    ContentSettings setting = JsonConvert.DeserializeObject<ContentSettings>(
                        option.Value
                    );
                    Options.Add(
                        new Option
                        {
                            Id = typeof(ContentSettings).ToString(),
                            Value = JsonConvert.SerializeObject(setting),
                        }
                    );
                }
                else
                {
                    Options.Add(
                        new Option
                        {
                            Id = typeof(ContentSettings).ToString(),
                            Value = JsonConvert.SerializeObject(new ContentSettings()),
                        }
                    );
                }
            }

            if (!Options.Any(o => o.Id == typeof(IntegrationSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Integrations"))
                {
                    Option option = Options.Find("Hood.Settings.Integrations");
                    IntegrationSettings setting =
                        JsonConvert.DeserializeObject<IntegrationSettings>(option.Value);
                    Options.Add(
                        new Option
                        {
                            Id = typeof(IntegrationSettings).ToString(),
                            Value = JsonConvert.SerializeObject(setting),
                        }
                    );
                }
                else
                {
                    Options.Add(
                        new Option
                        {
                            Id = typeof(IntegrationSettings).ToString(),
                            Value = JsonConvert.SerializeObject(new IntegrationSettings()),
                        }
                    );
                }
            }

            if (!Options.Any(o => o.Id == typeof(MailSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Mail"))
                {
                    Option option = Options.Find("Hood.Settings.Mail");
                    MailSettings setting = JsonConvert.DeserializeObject<MailSettings>(
                        option.Value
                    );
                    Options.Add(
                        new Option
                        {
                            Id = typeof(MailSettings).ToString(),
                            Value = JsonConvert.SerializeObject(setting),
                        }
                    );
                }
                else
                {
                    Options.Add(
                        new Option
                        {
                            Id = typeof(MailSettings).ToString(),
                            Value = JsonConvert.SerializeObject(new MailSettings()),
                        }
                    );
                }
            }

            if (!Options.Any(o => o.Id == typeof(MediaSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Media"))
                {
                    Option option = Options.Find("Hood.Settings.Media");
                    MediaSettings setting = JsonConvert.DeserializeObject<MediaSettings>(
                        option.Value
                    );
                    Options.Add(
                        new Option
                        {
                            Id = typeof(MediaSettings).ToString(),
                            Value = JsonConvert.SerializeObject(setting),
                        }
                    );
                }
                else
                {
                    Options.Add(
                        new Option
                        {
                            Id = typeof(MediaSettings).ToString(),
                            Value = JsonConvert.SerializeObject(new MediaSettings()),
                        }
                    );
                }
            }

            if (!Options.Any(o => o.Id == typeof(PropertySettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Property"))
                {
                    Option option = Options.Find("Hood.Settings.Property");
                    PropertySettings setting = JsonConvert.DeserializeObject<PropertySettings>(
                        option.Value
                    );
                    Options.Add(
                        new Option
                        {
                            Id = typeof(PropertySettings).ToString(),
                            Value = JsonConvert.SerializeObject(setting),
                        }
                    );
                }
                else
                {
                    Options.Add(
                        new Option
                        {
                            Id = typeof(PropertySettings).ToString(),
                            Value = JsonConvert.SerializeObject(new PropertySettings()),
                        }
                    );
                }
            }

            if (!Options.Any(o => o.Id == typeof(SeoSettings).ToString()))
            {
                // No new settings exist, attempt to copy from deprecated settings, or set new.
                if (Options.Any(o => o.Id == "Hood.Settings.Seo"))
                {
                    Option option = Options.Find("Hood.Settings.Seo");
                    SeoSettings setting = JsonConvert.DeserializeObject<SeoSettings>(option.Value);
                    Options.Add(
                        new Option
                        {
                            Id = typeof(SeoSettings).ToString(),
                            Value = JsonConvert.SerializeObject(setting),
                        }
                    );
                }
                else
                {
                    Options.Add(
                        new Option
                        {
                            Id = typeof(SeoSettings).ToString(),
                            Value = JsonConvert.SerializeObject(new SeoSettings()),
                        }
                    );
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
                MediaDirectory defaultDir = MediaDirectories
                    .AsNoTracking()
                    .SingleOrDefault(o =>
                        o.Slug == MediaManager.SiteDirectorySlug && o.Type == DirectoryType.System
                    );
                MediaDirectory contentDir = MediaDirectories
                    .AsNoTracking()
                    .SingleOrDefault(o =>
                        o.Slug == MediaManager.ContentDirectorySlug
                        && o.Type == DirectoryType.System
                    );
                MediaDirectory propertyDir = MediaDirectories
                    .AsNoTracking()
                    .SingleOrDefault(o =>
                        o.Slug == MediaManager.PropertyDirectorySlug
                        && o.Type == DirectoryType.System
                    );
                Media
                    .Where(o => o.FileType == "directory/dir")
                    .ToList()
                    .ForEach(a => Entry(a).State = EntityState.Deleted);
                try
                {
                    if (Media.Any(o => o.DirectoryId == null))
                    {
                        SaveChanges();

                        // ExecuteSql (FormattableString) parameterises the interpolated values
                        // automatically.
                        Database.ExecuteSql(
                            $"UPDATE HoodMedia SET DirectoryId = {propertyDir.Id} WHERE DirectoryId IS NULL AND Directory = 'Property'"
                        );

                        Option option = Options.Find(typeof(ContentSettings).ToString());
                        var contentSettings = JsonConvert.DeserializeObject<ContentSettings>(
                            option.Value
                        );
                        foreach (var type in contentSettings.Types)
                        {
                            // Interpolation via ExecuteSql makes type.TypeName a real SQL parameter, so each
                            // content type's media is re-pointed — a quoted string literal would only
                            // ever match 'Property'.
                            Database.ExecuteSql(
                                $"UPDATE HoodMedia SET DirectoryId = {contentDir.Id} WHERE DirectoryId IS NULL AND Directory = {type.TypeName}"
                            );
                        }

                        Database.ExecuteSql(
                            $"UPDATE HoodMedia SET DirectoryId = {defaultDir.Id} WHERE DirectoryId IS NULL"
                        );
                    }
                }
                catch (SqlException ex)
                {
                    throw new StartupException(
                        "Error updating the media entries.",
                        ex,
                        StartupError.DatabaseMediaError
                    );
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("Timeout"))
                    {
                        throw new StartupException(
                            "Error updating the media entries.",
                            ex,
                            StartupError.DatabaseMediaError
                        );
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

    /// <summary>
    /// Factory for creating the HoodDbContext, only used for script creation.
    /// </summary>
    public class HoodDbContextFactory : IDesignTimeDbContextFactory<HoodDbContext>
    {
        public HoodDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HoodDbContext>();
            optionsBuilder.UseSqlServer(DesignTimeConnection.ConnectionString);
            return new HoodDbContext(optionsBuilder.Options);
        }
    }
}
