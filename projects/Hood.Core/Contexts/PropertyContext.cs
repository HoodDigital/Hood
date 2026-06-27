using Hood.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hood.Contexts
{
    public class PropertyContext : DbContext
    {
        public PropertyContext(DbContextOptions<PropertyContext> options)
            : base(options) { }

        public DbSet<PropertyListing> Properties { get; set; }
        public DbSet<PropertyListingView> PropertyViews { get; set; }
        public DbSet<PropertyMedia> PropertyMedia { get; set; }
        public DbSet<PropertyFloorplan> PropertyFloorplans { get; set; }
        public DbSet<PropertyMeta> PropertyMetadata { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PropertyListing>().ToTable("HoodProperties");
            // AgentId bounded + indexed to converge with the shape upgraded DBs carry.
            builder.Entity<PropertyListing>().Property(p => p.AgentId).HasMaxLength(450);
            builder.Entity<PropertyListing>().HasIndex(p => p.AgentId);
            // HasSentinel(-1) so a real (0,0) coordinate is INSERTed explicitly instead of being
            // treated as "unset" and omitted under the EF Core 8+ sentinel rules.
            // The DB-side DEFAULT (0.0) is unchanged, so there is no schema delta.
            builder
                .Entity<PropertyListing>()
                .Property(a => a.Latitude)
                .HasDefaultValueSql("0.0")
                .HasSentinel(-1d);
            builder
                .Entity<PropertyListing>()
                .Property(a => a.Longitude)
                .HasDefaultValueSql("0.0")
                .HasSentinel(-1d);

            builder.Entity<PropertyMeta>().ToTable("HoodPropertyMetadata");
            // Alternate-key columns must be non-nullable under EF Core 9+.
            builder.Entity<PropertyMeta>().Property(o => o.Name).IsRequired();
            builder.Entity<PropertyMeta>().HasAlternateKey(ol => new { ol.PropertyId, ol.Name });
            builder
                .Entity<PropertyMeta>()
                .HasOne(c => c.Property)
                .WithMany(cc => cc.Metadata)
                .HasForeignKey(au => au.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PropertyMedia>().ToTable("HoodPropertyMedia");
            builder
                .Entity<PropertyMedia>()
                .HasOne(up => up.Property)
                .WithMany(t => t.Media)
                .HasForeignKey(au => au.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PropertyMedia>().Property(b => b.Path).HasColumnName("Directory");

            builder.Entity<PropertyFloorplan>().ToTable("HoodPropertyFloorplans");
            builder
                .Entity<PropertyFloorplan>()
                .HasOne(up => up.Property)
                .WithMany(t => t.FloorPlans)
                .HasForeignKey(au => au.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PropertyFloorplan>().Property(b => b.Path).HasColumnName("Directory");

            builder.Entity<PropertyListingView>().ToView("HoodPropertyViews");
            builder
                .Entity<PropertyListingView>()
                .HasMany(c => c.Metadata)
                .WithOne(c => c.PropertyListingView)
                .HasForeignKey(c => c.PropertyId);
            builder
                .Entity<PropertyListingView>()
                .HasMany(c => c.Media)
                .WithOne(c => c.PropertyListingView)
                .HasForeignKey(c => c.PropertyId);
            builder
                .Entity<PropertyListingView>()
                .HasMany(c => c.FloorPlans)
                .WithOne(c => c.PropertyListingView)
                .HasForeignKey(c => c.PropertyId);
        }
    }

    /// <summary>
    /// Factory for creating the PropertyContext, only used for script creation.
    /// </summary>
    public class PropertyContextFactory : IDesignTimeDbContextFactory<PropertyContext>
    {
        public PropertyContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PropertyContext>();
            optionsBuilder.UseSqlServer(DesignTimeConnection.ConnectionString);
            return new PropertyContext(optionsBuilder.Options);
        }
    }
}
