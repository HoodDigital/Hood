using Hood.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hood.Models
{
    public class ApplicationDbContext : HoodDbContext
    {
        public ApplicationDbContext(DbContextOptions<HoodDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public override void Seed()
        {
            base.Seed();
        }
    }
}
