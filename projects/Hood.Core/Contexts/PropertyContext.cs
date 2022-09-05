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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Contexts
{
    public class PropertyContext : DbContext
    {
        public PropertyContext(DbContextOptions<PropertyContext> options)
            : base(options)
        {}

        public DbSet<PropertyListing> Properties { get; set; }
        public DbSet<PropertyListingView> PropertyViews { get; set; }
        public DbSet<PropertyMedia> PropertyMedia { get; set; }
        public DbSet<PropertyFloorplan> PropertyFloorplans { get; set; }
        public DbSet<PropertyMeta> PropertyMetadata { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);            
        
            builder.Entity<PropertyListing>().ToTable("HoodProperties");
            builder.Entity<PropertyListing>().Property(a => a.Latitude).HasDefaultValueSql("0.0");
            builder.Entity<PropertyListing>().Property(a => a.Longitude).HasDefaultValueSql("0.0");

            builder.Entity<PropertyMeta>().ToTable("HoodPropertyMetadata");
            builder.Entity<PropertyMeta>().HasAlternateKey(ol => new { ol.PropertyId, ol.Name });
            builder.Entity<PropertyMeta>().HasOne(c => c.Property).WithMany(cc => cc.Metadata).HasForeignKey(au => au.PropertyId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PropertyMedia>().ToTable("HoodPropertyMedia");
            builder.Entity<PropertyMedia>().HasOne(up => up.Property).WithMany(t => t.Media).HasForeignKey(au => au.PropertyId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PropertyMedia>().Property(b => b.Path).HasColumnName("Directory");

            builder.Entity<PropertyFloorplan>().ToTable("HoodPropertyFloorplans");
            builder.Entity<PropertyFloorplan>().HasOne(up => up.Property).WithMany(t => t.FloorPlans).HasForeignKey(au => au.PropertyId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PropertyFloorplan>().Property(b => b.Path).HasColumnName("Directory");
            
            builder.Entity<PropertyListingView>().ToView("HoodPropertyViews");
            builder.Entity<PropertyListingView>().HasMany(c => c.Metadata).WithOne(c=> c.PropertyListingView).HasForeignKey(c => c.PropertyId);
            builder.Entity<PropertyListingView>().HasMany(c => c.Media).WithOne(c=> c.PropertyListingView).HasForeignKey(c => c.PropertyId);
            builder.Entity<PropertyListingView>().HasMany(c => c.FloorPlans).WithOne(c=> c.PropertyListingView).HasForeignKey(c => c.PropertyId);
        }

    }
}