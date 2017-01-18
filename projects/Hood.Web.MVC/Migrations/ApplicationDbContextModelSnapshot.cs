using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Hood.Web.MVC;

namespace Hood.Web.MVC.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Hood.Models.Address", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address1");

                    b.Property<string>("Address2");

                    b.Property<string>("City")
                        .IsRequired();

                    b.Property<string>("ContactName");

                    b.Property<string>("Country")
                        .IsRequired();

                    b.Property<string>("County")
                        .IsRequired();

                    b.Property<double>("Latitude")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0.0");

                    b.Property<double>("Longitude")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0.0");

                    b.Property<string>("Number")
                        .IsRequired();

                    b.Property<string>("Postcode")
                        .IsRequired();

                    b.Property<string>("QuickName");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("HoodAddresses");
                });

            modelBuilder.Entity("Hood.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("AvatarJson");

                    b.Property<string>("BillingAddressJson");

                    b.Property<string>("Bio");

                    b.Property<string>("ClientCode");

                    b.Property<string>("CompanyName");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("DeliveryAddressJson");

                    b.Property<string>("DisplayName");

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("EmailOptin");

                    b.Property<string>("Facebook");

                    b.Property<string>("FirstName");

                    b.Property<string>("GooglePlus");

                    b.Property<string>("JobTitle");

                    b.Property<DateTime>("LastLogOn");

                    b.Property<string>("LastLoginIP");

                    b.Property<string>("LastLoginLocation");

                    b.Property<string>("LastName");

                    b.Property<string>("Latitude");

                    b.Property<string>("LinkedIn");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("Longitude");

                    b.Property<string>("Mobile");

                    b.Property<string>("NormalizedEmail")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedUserName")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("Notes");

                    b.Property<string>("PasswordHash");

                    b.Property<string>("Phone");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<string>("StripeId");

                    b.Property<string>("SystemNotes");

                    b.Property<string>("Twitter");

                    b.Property<string>("TwitterHandle");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("UserVars");

                    b.Property<string>("VATNumber");

                    b.Property<string>("WebsiteUrl");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Hood.Models.Content", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("AllowComments");

                    b.Property<string>("AuthorId");

                    b.Property<string>("Body");

                    b.Property<string>("ContentType");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Excerpt");

                    b.Property<bool>("Featured");

                    b.Property<string>("FeaturedImageJson");

                    b.Property<string>("LastEditedBy");

                    b.Property<DateTime>("LastEditedOn");

                    b.Property<string>("Notes");

                    b.Property<int?>("ParentId");

                    b.Property<bool>("Public");

                    b.Property<DateTime>("PublishDate");

                    b.Property<int>("ShareCount");

                    b.Property<string>("Slug");

                    b.Property<int>("Status");

                    b.Property<string>("SystemNotes");

                    b.Property<string>("Title");

                    b.Property<string>("UserVars");

                    b.Property<int>("Views");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("HoodContent");
                });

            modelBuilder.Entity("Hood.Models.ContentCategory", b =>
                {
                    b.Property<int>("ContentCategoryId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ContentType");

                    b.Property<string>("DisplayName");

                    b.Property<int?>("ParentCategoryId");

                    b.Property<string>("Slug");

                    b.HasKey("ContentCategoryId");

                    b.HasIndex("ParentCategoryId");

                    b.ToTable("HoodContentCategories");
                });

            modelBuilder.Entity("Hood.Models.ContentCategoryJoin", b =>
                {
                    b.Property<int>("ContentId");

                    b.Property<int>("CategoryId");

                    b.HasKey("ContentId", "CategoryId");

                    b.HasIndex("CategoryId");

                    b.ToTable("HoodContentCategoryJoins");
                });

            modelBuilder.Entity("Hood.Models.ContentMedia", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BlobReference");

                    b.Property<int>("ContentId");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Directory");

                    b.Property<long>("FileSize");

                    b.Property<string>("FileType");

                    b.Property<string>("Filename");

                    b.Property<string>("GeneralFileType");

                    b.Property<string>("LargeUrl");

                    b.Property<string>("MediumUrl");

                    b.Property<string>("SmallUrl");

                    b.Property<string>("ThumbUrl");

                    b.Property<string>("UniqueId");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.HasIndex("ContentId");

                    b.ToTable("HoodContentMedia");
                });

            modelBuilder.Entity("Hood.Models.ContentMeta", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BaseValue");

                    b.Property<int>("ContentId");

                    b.Property<bool>("IsStored");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Type");

                    b.HasKey("Id");

                    b.HasAlternateKey("ContentId", "Name");

                    b.ToTable("HoodContentMetadata");
                });

            modelBuilder.Entity("Hood.Models.ContentTag", b =>
                {
                    b.Property<string>("Value")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Value");

                    b.ToTable("HoodContentTags");
                });

            modelBuilder.Entity("Hood.Models.ContentTagJoin", b =>
                {
                    b.Property<int>("ContentId");

                    b.Property<string>("TagId");

                    b.HasKey("ContentId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("HoodContentTagJoins");
                });

            modelBuilder.Entity("Hood.Models.Option", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("HoodOptions");
                });

            modelBuilder.Entity("Hood.Models.PropertyFloorplan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BlobReference");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Directory");

                    b.Property<long>("FileSize");

                    b.Property<string>("FileType");

                    b.Property<string>("Filename");

                    b.Property<string>("GeneralFileType");

                    b.Property<string>("LargeUrl");

                    b.Property<string>("MediumUrl");

                    b.Property<int>("PropertyId");

                    b.Property<string>("SmallUrl");

                    b.Property<string>("ThumbUrl");

                    b.Property<string>("UniqueId");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.HasIndex("PropertyId");

                    b.ToTable("HoodPropertyFloorplans");
                });

            modelBuilder.Entity("Hood.Models.PropertyListing", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Additional");

                    b.Property<string>("Address1");

                    b.Property<string>("Address2");

                    b.Property<string>("AgentId");

                    b.Property<string>("AgentInfo");

                    b.Property<bool>("AllowComments");

                    b.Property<string>("Areas");

                    b.Property<decimal?>("AskingPrice");

                    b.Property<string>("AskingPriceDisplay");

                    b.Property<int>("Bedrooms");

                    b.Property<string>("City");

                    b.Property<bool>("Confidential");

                    b.Property<string>("ContactName");

                    b.Property<string>("Country");

                    b.Property<string>("County");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Description");

                    b.Property<bool>("Featured");

                    b.Property<string>("FeaturedImageJson");

                    b.Property<decimal?>("Fees");

                    b.Property<string>("FeesDisplay");

                    b.Property<string>("Floors");

                    b.Property<string>("InfoDownloadJson");

                    b.Property<string>("LastEditedBy");

                    b.Property<DateTime>("LastEditedOn");

                    b.Property<double>("Latitude")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0.0");

                    b.Property<string>("Lease");

                    b.Property<string>("ListingType");

                    b.Property<string>("Location");

                    b.Property<double>("Longitude")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0.0");

                    b.Property<string>("Notes");

                    b.Property<string>("Number");

                    b.Property<string>("Planning");

                    b.Property<string>("Postcode");

                    b.Property<decimal?>("Premium");

                    b.Property<string>("PremiumDisplay");

                    b.Property<string>("PropertyType");

                    b.Property<bool>("Public");

                    b.Property<DateTime>("PublishDate");

                    b.Property<string>("Reference");

                    b.Property<decimal?>("Rent");

                    b.Property<string>("RentDisplay");

                    b.Property<int>("ShareCount");

                    b.Property<string>("ShortDescription");

                    b.Property<string>("Size");

                    b.Property<int>("Status");

                    b.Property<string>("SystemNotes");

                    b.Property<string>("Tags");

                    b.Property<string>("Title");

                    b.Property<string>("UserVars");

                    b.Property<int>("Views");

                    b.HasKey("Id");

                    b.HasIndex("AgentId");

                    b.ToTable("HoodProperties");
                });

            modelBuilder.Entity("Hood.Models.PropertyMedia", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BlobReference");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Directory");

                    b.Property<long>("FileSize");

                    b.Property<string>("FileType");

                    b.Property<string>("Filename");

                    b.Property<string>("GeneralFileType");

                    b.Property<string>("LargeUrl");

                    b.Property<string>("MediumUrl");

                    b.Property<int>("PropertyId");

                    b.Property<string>("SmallUrl");

                    b.Property<string>("ThumbUrl");

                    b.Property<string>("UniqueId");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.HasIndex("PropertyId");

                    b.ToTable("HoodPropertyMedia");
                });

            modelBuilder.Entity("Hood.Models.PropertyMeta", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BaseValue");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("PropertyId");

                    b.Property<string>("Type");

                    b.HasKey("Id");

                    b.HasAlternateKey("PropertyId", "Name");

                    b.ToTable("HoodPropertyMetadata");
                });

            modelBuilder.Entity("Hood.Models.SiteMedia", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BlobReference");

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("Directory");

                    b.Property<long>("FileSize");

                    b.Property<string>("FileType");

                    b.Property<string>("Filename");

                    b.Property<string>("GeneralFileType");

                    b.Property<string>("LargeUrl");

                    b.Property<string>("MediumUrl");

                    b.Property<string>("SmallUrl");

                    b.Property<string>("ThumbUrl");

                    b.Property<string>("UniqueId");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.ToTable("HoodMedia");
                });

            modelBuilder.Entity("Hood.Models.Subscription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Addon");

                    b.Property<int>("Amount");

                    b.Property<string>("Colour");

                    b.Property<DateTime>("Created");

                    b.Property<string>("CreatedBy");

                    b.Property<string>("Currency");

                    b.Property<string>("Description");

                    b.Property<string>("FeaturedImageUrl");

                    b.Property<string>("Interval");

                    b.Property<int>("IntervalCount");

                    b.Property<string>("LastEditedBy");

                    b.Property<DateTime>("LastEditedOn");

                    b.Property<int>("Level");

                    b.Property<bool>("LiveMode");

                    b.Property<string>("Name");

                    b.Property<int>("NumberAllowed");

                    b.Property<bool>("Public");

                    b.Property<string>("StatementDescriptor");

                    b.Property<string>("StripeId");

                    b.Property<int?>("TrialPeriodDays");

                    b.HasKey("Id");

                    b.ToTable("HoodSubscriptions");
                });

            modelBuilder.Entity("Hood.Models.SubscriptionFeature", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BaseValue");

                    b.Property<bool>("IsStored");

                    b.Property<string>("Name");

                    b.Property<int>("SubscriptionId");

                    b.Property<string>("Type");

                    b.HasKey("Id");

                    b.HasIndex("SubscriptionId");

                    b.ToTable("HoodSubscriptionFeatures");
                });

            modelBuilder.Entity("Hood.Models.UserSubscription", b =>
                {
                    b.Property<int>("UserSubscriptionId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("CancelAtPeriodEnd");

                    b.Property<DateTime?>("CanceledAt");

                    b.Property<bool>("Confirmed");

                    b.Property<DateTime?>("Created");

                    b.Property<DateTime?>("CurrentPeriodEnd");

                    b.Property<DateTime?>("CurrentPeriodStart");

                    b.Property<string>("CustomerId");

                    b.Property<bool>("Deleted");

                    b.Property<DateTime>("DeletedAt");

                    b.Property<DateTime?>("EndedAt");

                    b.Property<DateTime>("LastUpdated");

                    b.Property<string>("Notes");

                    b.Property<int>("Quantity");

                    b.Property<DateTime?>("Start");

                    b.Property<string>("Status");

                    b.Property<string>("StripeId");

                    b.Property<int>("SubscriptionId");

                    b.Property<decimal?>("TaxPercent");

                    b.Property<DateTime?>("TrialEnd");

                    b.Property<DateTime?>("TrialStart");

                    b.Property<string>("UserId");

                    b.HasKey("UserSubscriptionId");

                    b.HasIndex("SubscriptionId");

                    b.HasIndex("UserId");

                    b.ToTable("HoodUserSubscriptions");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Hood.Models.Address", b =>
                {
                    b.HasOne("Hood.Models.ApplicationUser", "User")
                        .WithMany("Addresses")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Hood.Models.Content", b =>
                {
                    b.HasOne("Hood.Models.ApplicationUser", "Author")
                        .WithMany("Content")
                        .HasForeignKey("AuthorId");
                });

            modelBuilder.Entity("Hood.Models.ContentCategory", b =>
                {
                    b.HasOne("Hood.Models.ContentCategory", "ParentCategory")
                        .WithMany("Children")
                        .HasForeignKey("ParentCategoryId");
                });

            modelBuilder.Entity("Hood.Models.ContentCategoryJoin", b =>
                {
                    b.HasOne("Hood.Models.ContentCategory", "Category")
                        .WithMany("Content")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Hood.Models.Content", "Content")
                        .WithMany("Categories")
                        .HasForeignKey("ContentId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hood.Models.ContentMedia", b =>
                {
                    b.HasOne("Hood.Models.Content", "Content")
                        .WithMany("Media")
                        .HasForeignKey("ContentId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hood.Models.ContentMeta", b =>
                {
                    b.HasOne("Hood.Models.Content", "Content")
                        .WithMany("Metadata")
                        .HasForeignKey("ContentId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hood.Models.ContentTagJoin", b =>
                {
                    b.HasOne("Hood.Models.Content", "Content")
                        .WithMany("Tags")
                        .HasForeignKey("ContentId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Hood.Models.ContentTag", "Tag")
                        .WithMany("Content")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hood.Models.PropertyFloorplan", b =>
                {
                    b.HasOne("Hood.Models.PropertyListing", "Property")
                        .WithMany("FloorPlans")
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hood.Models.PropertyListing", b =>
                {
                    b.HasOne("Hood.Models.ApplicationUser", "Agent")
                        .WithMany("Properties")
                        .HasForeignKey("AgentId");
                });

            modelBuilder.Entity("Hood.Models.PropertyMedia", b =>
                {
                    b.HasOne("Hood.Models.PropertyListing", "Property")
                        .WithMany("Media")
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hood.Models.PropertyMeta", b =>
                {
                    b.HasOne("Hood.Models.PropertyListing", "Property")
                        .WithMany("Metadata")
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hood.Models.SubscriptionFeature", b =>
                {
                    b.HasOne("Hood.Models.Subscription", "Subscription")
                        .WithMany("Features")
                        .HasForeignKey("SubscriptionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hood.Models.UserSubscription", b =>
                {
                    b.HasOne("Hood.Models.Subscription", "Subscription")
                        .WithMany("Users")
                        .HasForeignKey("SubscriptionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Hood.Models.ApplicationUser", "User")
                        .WithMany("Subscriptions")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Hood.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Hood.Models.ApplicationUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Hood.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
