using Hood.Models;
using Microsoft.EntityFrameworkCore;

namespace Hood.Extensions
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
            builder.Entity<Auth0User>().ToTable("AspNetAuth0Users");

            builder.Entity<Option>().ToTable("HoodOptions");
            builder.Entity<Log>().ToTable("HoodLogs");
            builder.Entity<Address>().ToTable("HoodAddresses");
            builder.Entity<Address>().Property(a => a.Latitude).HasDefaultValueSql("0.0");
            builder.Entity<Address>().Property(a => a.Longitude).HasDefaultValueSql("0.0");
            builder.Entity<Address>().HasOne(up => up.User).WithMany(add => add.Addresses).HasForeignKey(au => au.UserId);

            // Media
            builder.Entity<MediaObject>().ToTable("HoodMedia");
            builder.Entity<MediaObject>().Property(b => b.Path).HasColumnName("Directory");
            builder.Entity<MediaDirectory>().ToTable("HoodMediaDirectories");
            builder.Entity<MediaDirectory>().HasOne(m => m.Parent).WithMany(m => m.Children).HasForeignKey(m => m.ParentId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<MediaObject>().HasOne(m => m.Directory).WithMany(m => m.Media).HasForeignKey(m => m.DirectoryId).OnDelete(DeleteBehavior.Restrict);

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

            // Property
            builder.Entity<PropertyListing>().ToTable("HoodProperties");
            builder.Entity<PropertyListing>().Property(a => a.Latitude).HasDefaultValueSql("0.0");
            builder.Entity<PropertyListing>().Property(a => a.Longitude).HasDefaultValueSql("0.0");
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

            builder.Entity<UserProfile>().HasNoKey().ToView("HoodUserProfiles");
        }
    }
}