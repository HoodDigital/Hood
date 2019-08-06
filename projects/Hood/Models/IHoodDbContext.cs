using Hood.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hood.Models
{
    public interface IHoodDbContext : IDbContext
    {
        DbSet<UserAccessCode> AccessCodes { get; set; }
        DbSet<Address> Addresses { get; set; }
        DbSet<ApiEvent> ApiEvents { get; set; }
        DbSet<ApiKey> ApiKeys { get; set; }
        DbSet<Content> Content { get; set; }
        DbSet<ContentCategory> ContentCategories { get; set; }
        DbSet<ForumCategory> ForumCategories { get; set; }
        DbSet<Forum> Forums { get; set; }
        DbSet<Log> Logs { get; set; }
        DbSet<MediaObject> Media { get; set; }
        DbSet<Option> Options { get; set; }
        DbSet<Post> Posts { get; set; }
        DbSet<PropertyListing> Properties { get; set; }
        DbSet<Subscription> Subscriptions { get; set; }
        DbSet<Topic> Topics { get; set; }
        DbSet<UserSubscription> UserSubscriptions { get; set; }

        bool AllMigrationsApplied();
        void Seed(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager);
    }
}