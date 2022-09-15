using Hood.Contexts;
using Hood.Core;
using Hood.Entities;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Contexts
{
    public class ContentContext : DbContext
    {
        public ContentContext(DbContextOptions<ContentContext> options)
            : base(options)
        { }

        // Content
        public DbSet<Content> Content { get; set; }
        public DbSet<ContentView> ContentViews { get; set; }
        public DbSet<ContentMeta> ContentMetadata { get; set; }
        public DbSet<ContentMedia> ContentMedia { get; set; }
        public DbSet<ContentCategory> ContentCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Content>().ToTable("HoodContent");

            builder.Entity<ContentMedia>().ToTable("HoodContentMedia");
            builder.Entity<ContentMedia>().HasOne(up => up.Content).WithMany(t => t.Media).HasForeignKey(au => au.ContentId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ContentMedia>().Property(b => b.Path).HasColumnName("Directory");

            builder.Entity<ContentCategory>().ToTable("HoodContentCategories");
            builder.Entity<ContentCategory>().Property("Id").HasColumnName("ContentCategoryId");
            builder.Entity<ContentCategoryJoin>().ToTable("HoodContentCategoryJoins");
            builder.Entity<ContentCategoryJoin>().HasKey(t => new { t.ContentId, t.CategoryId });
            builder.Entity<ContentCategoryJoin>().HasOne(pt => pt.Category).WithMany(p => p.Content).HasForeignKey(pt => pt.CategoryId);
            builder.Entity<ContentCategoryJoin>().HasOne(pt => pt.Content).WithMany(t => t.Categories).HasForeignKey(pt => pt.ContentId);

            builder.Entity<ContentMeta>().ToTable("HoodContentMetadata");
            builder.Entity<ContentMeta>().HasAlternateKey(ol => new { ol.ContentId, ol.Name });
            builder.Entity<ContentMeta>().HasOne(c => c.Content).WithMany(cc => cc.Metadata).HasForeignKey(au => au.ContentId);
            
            builder.Entity<ContentView>().ToView("HoodContentViews");
            builder.Entity<ContentView>().HasMany(c => c.Metadata).WithOne(c=> c.ContentView).HasForeignKey(c => c.ContentId);
            builder.Entity<ContentView>().HasMany(c => c.Categories).WithOne(c=> c.ContentView).HasForeignKey(c => c.ContentId);
            builder.Entity<ContentView>().HasMany(c => c.Media).WithOne(c=> c.ContentView).HasForeignKey(c => c.ContentId);
        }

    }
    
    /// <summary>
    /// Factory for creating the ContentContext, only used for script creation.
    /// </summary>
    public class ContentContextFactory : IDesignTimeDbContextFactory<ContentContext>
    {
        public ContentContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ContentContext>();
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=Hood.Web;Trusted_Connection=True;MultipleActiveResultSets=true;");
            return new ContentContext(optionsBuilder.Options);
        }
    }
}