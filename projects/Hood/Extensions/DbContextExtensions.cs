using Hood.Models.Payments;
using Microsoft.EntityFrameworkCore;

namespace Hood.Models
{
    public static class DbContextExtensions
    {
        public static void ConfigureForHood(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging(true);
        }

        public static void CreateHoodModels(this ModelBuilder builder)
        {
            // Identity
            builder.Entity<Option>().ToTable("HoodOptions");
            builder.Entity<Log>().ToTable("HoodLogs");
            builder.Entity<ApiKey>().ToTable("HoodApiKeys");
            builder.Entity<ApiEvent>().ToTable("HoodApiEvents");
            builder.Entity<Address>().ToTable("HoodAddresses");
            builder.Entity<UserAccessCode>().ToTable("AspNetUserAccessCodes");
            builder.Entity<Address>().Property(a => a.Latitude).HasDefaultValue(0.0).HasDefaultValueSql("0.0");
            builder.Entity<Address>().Property(a => a.Longitude).HasDefaultValue(0.0).HasDefaultValueSql("0.0");
            builder.Entity<Address>().HasOne(up => up.User).WithMany(add => add.Addresses).HasForeignKey(au => au.UserId);
            builder.Entity<UserAccessCode>().HasOne(up => up.User).WithMany(add => add.AccessCodes).HasForeignKey(au => au.UserId);

            // Media
            builder.Entity<MediaObject>().ToTable("HoodMedia");
            builder.Entity<MediaObject>().Property(b => b.Path).HasColumnName("Directory");
            builder.Entity<MediaDirectory>().ToTable("HoodMediaDirectories");
            builder.Entity<MediaDirectory>().HasOne(m => m.Parent).WithMany(m => m.Children).HasForeignKey(m => m.ParentId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<MediaObject>().HasOne(m => m.Directory).WithMany(m => m.Media).HasForeignKey(m => m.DirectoryId).OnDelete(DeleteBehavior.Restrict);

            // Subscriptions
            builder.Entity<SubscriptionGroup>().ToTable("HoodSubscriptionGroups");
            builder.Entity<Subscription>().ToTable("HoodSubscriptions");
            builder.Entity<SubscriptionFeature>().ToTable("HoodSubscriptionFeatures");
            builder.Entity<UserSubscription>().ToTable("HoodUserSubscriptions");
            builder.Entity<UserSubscription>().Property("Id").HasColumnName("UserSubscriptionId");
            builder.Entity<UserSubscription>().HasOne(pt => pt.User).WithMany(p => p.Subscriptions).HasForeignKey(pt => pt.UserId);
            builder.Entity<UserSubscription>().HasOne(pt => pt.Subscription).WithMany(t => t.Users).HasForeignKey(pt => pt.SubscriptionId);

            // Content
            builder.Entity<Content>().ToTable("HoodContent");
            builder.Entity<Content>().HasOne(c => c.Author).WithMany(up => up.Content).HasForeignKey(c => c.AuthorId);

            // Content Media
            builder.Entity<ContentMedia>().ToTable("HoodContentMedia");
            builder.Entity<ContentMedia>().HasOne(up => up.Content).WithMany(t => t.Media).HasForeignKey(au => au.ContentId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ContentMedia>().Property(b => b.Path).HasColumnName("Directory");

            // Categories
            builder.Entity<ContentCategory>().ToTable("HoodContentCategories");
            builder.Entity<ContentCategory>().Property("Id").HasColumnName("ContentCategoryId");
            builder.Entity<ContentCategoryJoin>().ToTable("HoodContentCategoryJoins");
            builder.Entity<ContentCategoryJoin>().HasKey(t => new { t.ContentId, t.CategoryId });
            builder.Entity<ContentCategoryJoin>().HasOne(pt => pt.Category).WithMany(p => p.Content).HasForeignKey(pt => pt.CategoryId);
            builder.Entity<ContentCategoryJoin>().HasOne(pt => pt.Content).WithMany(t => t.Categories).HasForeignKey(pt => pt.ContentId);

            // Content Metadata
            builder.Entity<ContentMeta>().ToTable("HoodContentMetadata");
            builder.Entity<ContentMeta>().HasAlternateKey(ol => new { ol.ContentId, ol.Name });
            builder.Entity<ContentMeta>().HasOne(c => c.Content).WithMany(cc => cc.Metadata).HasForeignKey(au => au.ContentId);

            // Forums
            builder.Entity<Forum>().ToTable("HoodForums");
            builder.Entity<Forum>().HasOne(c => c.Author).WithMany(up => up.Forums).HasForeignKey(c => c.AuthorId);

            // Forum Categories
            builder.Entity<ForumCategory>().ToTable("HoodForumCategories");
            builder.Entity<ForumCategory>().Property("Id").HasColumnName("ForumCategoryId");
            builder.Entity<ForumCategoryJoin>().ToTable("HoodForumCategoryJoins");
            builder.Entity<ForumCategoryJoin>().HasKey(t => new { t.ForumId, t.CategoryId });
            builder.Entity<ForumCategoryJoin>().HasOne(pt => pt.Category).WithMany(p => p.Forum).HasForeignKey(pt => pt.CategoryId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ForumCategoryJoin>().HasOne(pt => pt.Forum).WithMany(t => t.Categories).HasForeignKey(pt => pt.ForumId).OnDelete(DeleteBehavior.Cascade);

            // Forum Topics
            builder.Entity<Topic>().ToTable("HoodForumTopics");
            builder.Entity<Topic>().HasOne(c => c.Author).WithMany(up => up.Topics).HasForeignKey(c => c.AuthorId);
            builder.Entity<Topic>().HasOne(c => c.Forum).WithMany(up => up.Topics).HasForeignKey(c => c.ForumId).OnDelete(DeleteBehavior.Cascade);

            // Forum Posts
            builder.Entity<Post>().ToTable("HoodForumPosts");
            builder.Entity<Post>().HasOne(c => c.Author).WithMany(up => up.Posts).HasForeignKey(c => c.AuthorId);
            builder.Entity<Post>().HasOne(c => c.EditedBy).WithMany(up => up.EditedPosts).HasForeignKey(c => c.EditedById);
            builder.Entity<Post>().HasOne(c => c.DeletedBy).WithMany(up => up.DeletedPosts).HasForeignKey(c => c.DeletedById);
            builder.Entity<Post>().HasOne(c => c.Topic).WithMany(up => up.Posts).HasForeignKey(c => c.TopicId).OnDelete(DeleteBehavior.Cascade);

            // Property
            builder.Entity<PropertyListing>().ToTable("HoodProperties");
            builder.Entity<PropertyListing>().Property(a => a.Latitude).HasDefaultValue(0.0).HasDefaultValueSql("0.0");
            builder.Entity<PropertyListing>().Property(a => a.Longitude).HasDefaultValue(0.0).HasDefaultValueSql("0.0");
            builder.Entity<PropertyListing>().HasOne(c => c.Agent).WithMany(up => up.Properties).HasForeignKey(c => c.AgentId);

            builder.Entity<PropertyMeta>().ToTable("HoodPropertyMetadata");
            builder.Entity<PropertyMeta>().HasAlternateKey(ol => new { ol.PropertyId, ol.Name });
            builder.Entity<PropertyMeta>().HasOne(c => c.Property).WithMany(cc => cc.Metadata).HasForeignKey(au => au.PropertyId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PropertyMedia>().ToTable("HoodPropertyMedia");
            builder.Entity<PropertyMedia>().HasOne(up => up.Property).WithMany(t => t.Media).HasForeignKey(au => au.PropertyId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PropertyMedia>().Property(b => b.Path).HasColumnName("Directory");

            builder.Entity<PropertyFloorplan>().ToTable("HoodPropertyFloorplans");
            builder.Entity<PropertyFloorplan>().HasOne(up => up.Property).WithMany(t => t.FloorPlans).HasForeignKey(au => au.PropertyId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PropertyFloorplan>().Property(b => b.Path).HasColumnName("Directory");

            builder.Query<UserProfile>().ToView("HoodUserProfiles");
            builder.Query<UserProfile>().Property(b => b.RolesJson).HasColumnName("Roles");
            builder.Query<UserProfile>().Property(b => b.SubscriptionsJson).HasColumnName("Subscriptions");

            builder.Query<SubscriptionPlan>().ToView("HoodSubscriptionPlans");

        }

        public static void RegisterSagePayBackingFields<T>(this ModelBuilder builder) where T : SagePayTransaction
        {
            builder.Entity<T>().Property<string>("NotesJson").HasField("_NotesJson").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("PaymentMethodJson").HasField("_PaymentMethodJson").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("AmountJson").HasField("_AmountJson").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("BillingAddressJson").HasField("_BillingAddressJson").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("ShippingAddressJson").HasField("_ShippingAddressJson").UsePropertyAccessMode(PropertyAccessMode.Field);
        }

    }
}