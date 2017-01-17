using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hood.Models
{
    public class HoodDbContext : IdentityDbContext<ApplicationUser>
    {

        public HoodDbContext(DbContextOptions<HoodDbContext> options)
            : base(options)
        {
        }

        // Identity
        public DbSet<Address> Addresses { get; set; }
        public DbSet<SiteMedia> Media { get; set; }

        // Content
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }

        // Options
        public DbSet<Option> Options { get; set; }

        // Content
        public DbSet<Content> Content { get; set; }
        public DbSet<ContentCategory> ContentCategories { get; set; }
        public DbSet<ContentTag> ContentTags { get; set; }

        // Property
        public DbSet<PropertyListing> Properties { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Identity
            builder.Entity<Option>().ToTable("HoodOptions");
            builder.Entity<SiteMedia>().ToTable("HoodMedia");
            builder.Entity<Address>().ToTable("HoodAddresses");
            builder.Entity<Address>().Property(a => a.Latitude).HasDefaultValue(0.0).HasDefaultValueSql("0.0");
            builder.Entity<Address>().Property(a => a.Longitude).HasDefaultValue(0.0).HasDefaultValueSql("0.0");
            builder.Entity<Address>().HasOne(up => up.User).WithMany(add => add.Addresses).HasForeignKey(au => au.UserId);

            // Subscriptions
            builder.Entity<Subscription>().ToTable("HoodSubscriptions");
            builder.Entity<SubscriptionFeature>().ToTable("HoodSubscriptionFeatures");
            builder.Entity<UserSubscription>().ToTable("HoodUserSubscriptions");
            builder.Entity<UserSubscription>().HasOne(pt => pt.User).WithMany(p => p.Subscriptions).HasForeignKey(pt => pt.UserId);
            builder.Entity<UserSubscription>().HasOne(pt => pt.Subscription).WithMany(t => t.Users).HasForeignKey(pt => pt.SubscriptionId);

            // Content
            builder.Entity<Content>().ToTable("HoodContent");
            builder.Entity<Content>().HasOne(c => c.Author).WithMany(up => up.Content).HasForeignKey(c => c.AuthorId);

            builder.Entity<ContentMedia>().ToTable("HoodContentMedia");
            builder.Entity<ContentMedia>().HasOne(up => up.Content).WithMany(t => t.Media).HasForeignKey(au => au.ContentId);

            // Categories
            builder.Entity<ContentCategory>().ToTable("HoodContentCategories");
            builder.Entity<ContentCategoryJoin>().ToTable("HoodContentCategoryJoins");
            builder.Entity<ContentCategoryJoin>().HasKey(t => new { t.ContentId, t.CategoryId });
            builder.Entity<ContentCategoryJoin>().HasOne(pt => pt.Category).WithMany(p => p.Content).HasForeignKey(pt => pt.CategoryId);
            builder.Entity<ContentCategoryJoin>().HasOne(pt => pt.Content).WithMany(t => t.Categories).HasForeignKey(pt => pt.ContentId);

            // Content Tags
            builder.Entity<ContentTag>().ToTable("HoodContentTags");
            builder.Entity<ContentTagJoin>().ToTable("HoodContentTagJoins");
            builder.Entity<ContentTagJoin>().HasKey(t => new { t.ContentId, t.TagId });
            builder.Entity<ContentTagJoin>().HasOne(pt => pt.Tag).WithMany(p => p.Content).HasForeignKey(pt => pt.TagId);
            builder.Entity<ContentTagJoin>().HasOne(pt => pt.Content).WithMany(t => t.Tags).HasForeignKey(pt => pt.ContentId);

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
    }
}