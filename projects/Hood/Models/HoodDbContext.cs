using Hood.Models.Payments;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hood.Models
{
    /// <summary>
    /// In order to use the Hood functionality on the site, you must call the 'void Configure(DbContextOptionsBuilder optionsBuilder)' function in the OnConfiguring() method, and then call the 'void CreateModels(ModelBuilder builder)' function in the OnModelCreating() method.
    /// </summary>
    /// <param name="optionsBuilder"></param>
    public class HoodDbContext : IdentityDbContext<ApplicationUser>
    {

        public HoodDbContext(DbContextOptions options)
            : base(options)
        {
        }

        // Identity
        public DbSet<UserAccessCode> AccessCodes { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<MediaObject> Media { get; set; }

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
            DbContextExtensions.ConfigureForHood(optionsBuilder);
        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            DbContextExtensions.CreateHoodModels(builder);
        }

        public void RegisterSagePayBackingFields<T>(ModelBuilder builder) where T : SagePayTransaction
        {
            builder.Entity<T>().Property<string>("CardIdentifier").HasField("_CardIdentifier").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("MerchantSessionKey").HasField("_MerchantSessionKey").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("Reusable").HasField("_Reusable").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("Save").HasField("_Save").UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Entity<T>().Property<string>("BillingNumber").HasField("_BillingNumber").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("BillingAddress1").HasField("_BillingAddress1").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("BillingAddress2").HasField("_BillingAddress2").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("BillingCity").HasField("_BillingCity").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("BillingCounty").HasField("_BillingCounty").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("BillingCountry").HasField("_BillingCountry").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("BillingPostcode").HasField("_BillingPostcode").UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Entity<T>().Property<string>("ShippingNumber").HasField("_ShippingNumber").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("ShippingAddress1").HasField("_ShippingAddress1").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("ShippingAddress2").HasField("_ShippingAddress2").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("ShippingCity").HasField("_ShippingCity").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("ShippingCounty").HasField("_ShippingCounty").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("ShippingCountry").HasField("_ShippingCountry").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Entity<T>().Property<string>("ShippingPostcode").HasField("_BillingPostcode").UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}