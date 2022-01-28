using Hood.Core;
using Hood.Entities;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
    /// <summary>
    /// In order to use the Hood functionality on the site, you must call the 'void Configure(DbContextOptionsBuilder optionsBuilder)' function in the OnConfiguring() method, and then call the 'void CreateModels(ModelBuilder builder)' function in the OnModelCreating() method.
    /// </summary>
    /// <param name="optionsBuilder"></param>
    public class HoodDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IHoodDbContext
    {
        public HoodDbContext(DbContextOptions<HoodDbContext> options)
            : base(options)
        {
        }

        // Identity
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Auth0User> Auth0Users { get; set; }

        // Media
        public DbSet<MediaObject> Media { get; set; }
        public DbSet<MediaDirectory> MediaDirectories { get; set; }


        // Options
        public DbSet<Option> Options { get; set; }

        // Content
        public DbSet<Content> Content { get; set; }
        public DbSet<ContentMeta> ContentMetadata { get; set; }
        public DbSet<ContentMedia> ContentMedia { get; set; }
        public DbSet<ContentCategory> ContentCategories { get; set; }


        // Property
        public DbSet<PropertyListing> Properties { get; set; }
        public DbSet<PropertyMedia> PropertyMedia { get; set; }
        public DbSet<PropertyFloorplan> PropertyFloorplans { get; set; }
        public DbSet<PropertyMeta> PropertyMetadata { get; set; }

        // Logs
        public DbSet<Log> Logs { get; set; }


        // Auth0
        public DbSet<UserProfile> UserProfiles { get; set; }


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

        public async virtual Task Seed()
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

            try
            {
                string ownerEmail = Engine.SiteOwnerEmail;
                if (!Users.Any(u => u.UserName == ownerEmail))
                {
                    ApplicationUser userToInsert = new ApplicationUser
                    {
                        CompanyName = "",
                        CreatedOn = DateTime.UtcNow,
                        FirstName = "Website",
                        LastName = "Administrator",
                        EmailConfirmed = true,
                        Anonymous = false,
                        PhoneNumber = "",
                        JobTitle = "Website Administrator",
                        LastLogOn = DateTime.UtcNow,
                        LastLoginIP = "127.0.0.1",
                        LastLoginLocation = "UK",
                        Email = ownerEmail,
                        UserName = ownerEmail,
                        Active = true
                    };
                    var keygen = new KeyGenerator();
                    var password = keygen.Generate(16);
                    IdentityResult ir = await Engine.AccountManager.CreateAsync(userToInsert, password);

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

            }
            catch (Exception ex)
            {
                throw new StartupException("An error occurred while loading or creating the admin user.", ex, StartupError.AdminUserSetupError);
            }

            ApplicationUser siteAdmin = await Engine.AccountManager.GetUserByEmailAsync(Engine.SiteOwnerEmail);
            try
            {
                if (Engine.AccountManager.SupportsRoles())
                {
                    // Check all required roles exist locally and are 
                    foreach (string role in Models.Roles.All)
                    {
                        var roleManager = Engine.Services.Resolve<RoleManager<IdentityRole>>();
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await Engine.AccountManager.CreateRoleAsync(role);
                        }
                        await Engine.AccountManager.AddUserToRoleAsync(siteAdmin, role);
                    }
                    var allRoles = await Engine.AccountManager.GetRolesAsync();
                    var extraRoles = allRoles.List.Where(r => !Models.Roles.All.Any(rr => r.NormalizedName == rr.ToUpperInvariant()));
                    foreach (var role in extraRoles)
                    {
                        await Engine.AccountManager.CreateRoleAsync(role.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new StartupException("An error occurred syncing local roles with Auth0.", ex, StartupError.Auth0Issue);
            }

            if (Engine.Auth0Enabled)
            {
                // Check the rule is set to provide roles from Auth0
                var auth0Service = new Auth0Service();
                var client = await auth0Service.GetClientAsync();
                var rules = await client.Rules.GetAllAsync(new Auth0.ManagementApi.Models.GetRulesRequest()
                {
                    Stage = "login_success"
                });
                var roleRule = rules.SingleOrDefault(r => r.Name == Hood.Identity.Constants.AddRoleClaimsRuleName);
                if (roleRule != null)
                {
                    await client.Rules.DeleteAsync(roleRule.Id);
                }
                var roleScript = @"
function (user, context, callback) {
    const role_namespace = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    if (context.authorization !== null) {
        if (context.authorization.roles !== null) {
            context.idToken[role_namespace] = context.authorization.roles;
        } else {
            console.log('context.authorization.roles is null');
        }
    } else {
        console.log('context.authorization is null');
    }
    return callback(null, user, context);
}";
                await client.Rules.CreateAsync(new Auth0.ManagementApi.Models.RuleCreateRequest()
                {
                    Script = roleScript,
                    Name = Hood.Identity.Constants.AddRoleClaimsRuleName,
                    Enabled = true,
                    Stage = "login_success"
                });
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

            await SaveChangesAsync();
        }

    }
}