using Hood.Core;
using Hood.Enums;
using Hood.Models;
using Hood.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Contexts
{
    public class Auth0IdentityContext : DbContext, IHoodIdentityContext
    {
        public Auth0IdentityContext(DbContextOptions<Auth0IdentityContext> options)
            : base(options)
        {
        }

        public DbSet<Auth0User> Users { get; set; } = default!;
        public DbSet<UserProfile> UserProfiles { get; set; } = default!;
        public DbSet<Auth0UserRole> UserRoles { get; set; } = default!;
        public DbSet<Auth0Role> Roles { get; set; } = default!;
        public DbSet<Auth0Identity> Auth0Identities { get; set; } = default!;
        public DbSet<UserProfileView<Auth0Role>> UserProfileViews { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Auth0User>(
                typeBuilder =>
                {
                    typeBuilder.ToTable("AspNetUsers");
                    typeBuilder.Property(o => o.UserName).HasColumnName("UserName");
                    typeBuilder.Property(o => o.Email).HasColumnName("Email");
                    typeBuilder.Property(o => o.PhoneNumber).HasColumnName("PhoneNumber");

                    typeBuilder.HasOne(o => o.UserProfile).WithOne().HasForeignKey<UserProfile>(o => o.Id);
                    typeBuilder.HasMany<Auth0UserRole>().WithOne().HasForeignKey(ur => ur.UserId).IsRequired();
                });

            builder.Entity<UserProfile>(
                typeBuilder =>
                {
                    typeBuilder.ToTable("AspNetUsers");
                    typeBuilder.Property(o => o.UserName).HasColumnName("UserName");
                    typeBuilder.Property(o => o.Email).HasColumnName("Email");
                    typeBuilder.Property(o => o.PhoneNumber).HasColumnName("PhoneNumber");
                });
            builder.Entity<Auth0Identity>().ToTable("AspNetAuth0Identities");
            builder.Entity<Auth0Identity>().HasOne(m => m.User).WithMany(m => m.ConnectedAuth0Accounts).HasForeignKey(m => m.LocalUserId).OnDelete(DeleteBehavior.Cascade);


            builder.Entity<Auth0Role>(b =>
            {
                b.HasKey(r => r.Id);
                b.HasIndex(r => r.NormalizedName).HasDatabaseName("RoleNameIndex").IsUnique();
                b.ToTable("AspNetRoles");
                b.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();

                b.Property(u => u.Name).HasMaxLength(256);
                b.Property(u => u.NormalizedName).HasMaxLength(256);

                b.HasMany<Auth0UserRole>().WithOne().HasForeignKey(ur => ur.RoleId).IsRequired();
            });

            builder.Entity<Auth0UserRole>(b =>
            {
                b.HasKey(r => new { r.UserId, r.RoleId });
                b.ToTable("AspNetUserRoles");
                // b.HasOne<Auth0User>().WithMany(u => u.Roles).HasForeignKey(u => u.UserId);
                // b.HasOne<Auth0Role>().WithMany(u => u.Roles).HasForeignKey(u => u.UserId);
            });

            builder.Entity<UserProfileView<Auth0Role>>().HasNoKey().ToView("HoodAuth0UserProfiles");
        }

        public async Task<IHoodIdentity> GetSiteAdmin()
        {
            try
            {
                IAuth0AccountRepository repo = Engine.Services.Resolve<IAuth0AccountRepository>();
                string ownerEmail = Engine.SiteOwnerEmail;
                if (!Users.Any(u => u.UserName == ownerEmail))
                {
                    Auth0User userToInsert = new Auth0User
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
                            Email = ownerEmail,
                            UserName = ownerEmail,
                            PhoneNumber = "",

                            FirstName = "Website",
                            LastName = "Administrator",
                            JobTitle = "Website Administrator",
                            Anonymous = false
                        }
                    };
                    Users.Add(userToInsert);
                    await SaveChangesAsync();
                }

                Auth0User siteAdmin = await repo.GetUserByEmailAsync(Engine.SiteOwnerEmail);
                return siteAdmin;

            }
            catch (Exception ex)
            {
                throw new StartupException("An error occurred while loading or creating the admin user.", ex, StartupError.AdminUserSetupError);
            }
        }

    }
    
    /// <summary>
    /// Factory for creating the Auth0IdentityContext, only used for script creation.
    /// </summary>
    public class Auth0IdentityContextFactory : IDesignTimeDbContextFactory<Auth0IdentityContext>
    {
        public Auth0IdentityContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<Auth0IdentityContext>();
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=Hood.Web;Trusted_Connection=True;MultipleActiveResultSets=true;");
            return new Auth0IdentityContext(optionsBuilder.Options);
        }
    }
}