using Hood.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hood.Web
{
    public class ApplicationDbContext : HoodDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
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

        public override void Seed(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            base.Seed(userManager, roleManager);


        }
    }
}
