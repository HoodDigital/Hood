using Hood.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Web.MVC
{
    public class ApplicationDbContext : HoodDbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        // Application Specific Datasets go here
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Application Specific model setup goes here
        }
    }
}
