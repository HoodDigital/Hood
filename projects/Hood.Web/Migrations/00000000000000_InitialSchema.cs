using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Hood.Web.Migrations
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    AvatarJson = table.Column<string>(nullable: true),
                    BillingAddressJson = table.Column<string>(nullable: true),
                    Bio = table.Column<string>(nullable: true),
                    ClientCode = table.Column<string>(nullable: true),
                    CompanyName = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    DeliveryAddressJson = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    EmailOptin = table.Column<bool>(nullable: false),
                    Facebook = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    GooglePlus = table.Column<string>(nullable: true),
                    JobTitle = table.Column<string>(nullable: true),
                    LastLogOn = table.Column<DateTime>(nullable: false),
                    LastLoginIP = table.Column<string>(nullable: true),
                    LastLoginLocation = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    LinkedIn = table.Column<string>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    Longitude = table.Column<string>(nullable: true),
                    Mobile = table.Column<string>(nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    SecurityStamp = table.Column<string>(nullable: true),
                    StripeId = table.Column<string>(nullable: true),
                    SystemNotes = table.Column<string>(nullable: true),
                    Twitter = table.Column<string>(nullable: true),
                    TwitterHandle = table.Column<string>(nullable: true),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    UserVars = table.Column<string>(nullable: true),
                    VATNumber = table.Column<string>(nullable: true),
                    WebsiteUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoodContentCategories",
                columns: table => new
                {
                    ContentCategoryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ContentType = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    ParentCategoryId = table.Column<int>(nullable: true),
                    Slug = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodContentCategories", x => x.ContentCategoryId);
                    table.ForeignKey(
                        name: "FK_HoodContentCategories_HoodContentCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "HoodContentCategories",
                        principalColumn: "ContentCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoodContentTags",
                columns: table => new
                {
                    Value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodContentTags", x => x.Value);
                });

            migrationBuilder.CreateTable(
                name: "HoodOptions",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoodMedia",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlobReference = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Directory = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    FileType = table.Column<string>(nullable: true),
                    Filename = table.Column<string>(nullable: true),
                    GeneralFileType = table.Column<string>(nullable: true),
                    LargeUrl = table.Column<string>(nullable: true),
                    MediumUrl = table.Column<string>(nullable: true),
                    SmallUrl = table.Column<string>(nullable: true),
                    ThumbUrl = table.Column<string>(nullable: true),
                    UniqueId = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodMedia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoodSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Addon = table.Column<bool>(nullable: false),
                    Amount = table.Column<int>(nullable: false),
                    Colour = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    Currency = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    FeaturedImageUrl = table.Column<string>(nullable: true),
                    Interval = table.Column<string>(nullable: true),
                    IntervalCount = table.Column<int>(nullable: false),
                    LastEditedBy = table.Column<string>(nullable: true),
                    LastEditedOn = table.Column<DateTime>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    LiveMode = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    NumberAllowed = table.Column<int>(nullable: false),
                    Public = table.Column<bool>(nullable: false),
                    StatementDescriptor = table.Column<string>(nullable: true),
                    StripeId = table.Column<string>(nullable: true),
                    TrialPeriodDays = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "HoodAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Address1 = table.Column<string>(nullable: true),
                    Address2 = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: false),
                    ContactName = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: false),
                    County = table.Column<string>(nullable: false),
                    Latitude = table.Column<double>(nullable: false, defaultValueSql: "0.0"),
                    Longitude = table.Column<double>(nullable: false, defaultValueSql: "0.0"),
                    Number = table.Column<string>(nullable: false),
                    Postcode = table.Column<string>(nullable: false),
                    QuickName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodAddresses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoodContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AllowComments = table.Column<bool>(nullable: false),
                    AuthorId = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Excerpt = table.Column<string>(nullable: true),
                    Featured = table.Column<bool>(nullable: false),
                    FeaturedImageJson = table.Column<string>(nullable: true),
                    LastEditedBy = table.Column<string>(nullable: true),
                    LastEditedOn = table.Column<DateTime>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    ParentId = table.Column<int>(nullable: true),
                    Public = table.Column<bool>(nullable: false),
                    PublishDate = table.Column<DateTime>(nullable: false),
                    ShareCount = table.Column<int>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    SystemNotes = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    UserVars = table.Column<string>(nullable: true),
                    Views = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodContent_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoodProperties",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Additional = table.Column<string>(nullable: true),
                    Address1 = table.Column<string>(nullable: true),
                    Address2 = table.Column<string>(nullable: true),
                    AgentId = table.Column<string>(nullable: true),
                    AgentInfo = table.Column<string>(nullable: true),
                    AllowComments = table.Column<bool>(nullable: false),
                    Areas = table.Column<string>(nullable: true),
                    AskingPrice = table.Column<decimal>(nullable: true),
                    AskingPriceDisplay = table.Column<string>(nullable: true),
                    Bedrooms = table.Column<int>(nullable: false),
                    City = table.Column<string>(nullable: true),
                    Confidential = table.Column<bool>(nullable: false),
                    ContactName = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    County = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Featured = table.Column<bool>(nullable: false),
                    FeaturedImageJson = table.Column<string>(nullable: true),
                    Fees = table.Column<decimal>(nullable: true),
                    FeesDisplay = table.Column<string>(nullable: true),
                    Floors = table.Column<string>(nullable: true),
                    InfoDownloadJson = table.Column<string>(nullable: true),
                    LastEditedBy = table.Column<string>(nullable: true),
                    LastEditedOn = table.Column<DateTime>(nullable: false),
                    Latitude = table.Column<double>(nullable: false, defaultValueSql: "0.0"),
                    Lease = table.Column<string>(nullable: true),
                    ListingType = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: true),
                    Longitude = table.Column<double>(nullable: false, defaultValueSql: "0.0"),
                    Notes = table.Column<string>(nullable: true),
                    Number = table.Column<string>(nullable: true),
                    Planning = table.Column<string>(nullable: true),
                    Postcode = table.Column<string>(nullable: true),
                    Premium = table.Column<decimal>(nullable: true),
                    PremiumDisplay = table.Column<string>(nullable: true),
                    PropertyType = table.Column<string>(nullable: true),
                    Public = table.Column<bool>(nullable: false),
                    PublishDate = table.Column<DateTime>(nullable: false),
                    Reference = table.Column<string>(nullable: true),
                    Rent = table.Column<decimal>(nullable: true),
                    RentDisplay = table.Column<string>(nullable: true),
                    ShareCount = table.Column<int>(nullable: false),
                    ShortDescription = table.Column<string>(nullable: true),
                    Size = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    SystemNotes = table.Column<string>(nullable: true),
                    Tags = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    UserVars = table.Column<string>(nullable: true),
                    Views = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodProperties_AspNetUsers_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodSubscriptionFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BaseValue = table.Column<string>(nullable: true),
                    IsStored = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    SubscriptionId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodSubscriptionFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodSubscriptionFeatures_HoodSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "HoodSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodUserSubscriptions",
                columns: table => new
                {
                    UserSubscriptionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CancelAtPeriodEnd = table.Column<bool>(nullable: false),
                    CanceledAt = table.Column<DateTime>(nullable: true),
                    Confirmed = table.Column<bool>(nullable: false),
                    Created = table.Column<DateTime>(nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(nullable: true),
                    CurrentPeriodStart = table.Column<DateTime>(nullable: true),
                    CustomerId = table.Column<string>(nullable: true),
                    Deleted = table.Column<bool>(nullable: false),
                    DeletedAt = table.Column<DateTime>(nullable: false),
                    EndedAt = table.Column<DateTime>(nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    Quantity = table.Column<int>(nullable: false),
                    Start = table.Column<DateTime>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    StripeId = table.Column<string>(nullable: true),
                    SubscriptionId = table.Column<int>(nullable: false),
                    TaxPercent = table.Column<decimal>(nullable: true),
                    TrialEnd = table.Column<DateTime>(nullable: true),
                    TrialStart = table.Column<DateTime>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodUserSubscriptions", x => x.UserSubscriptionId);
                    table.ForeignKey(
                        name: "FK_HoodUserSubscriptions_HoodSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "HoodSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoodUserSubscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodContentCategoryJoins",
                columns: table => new
                {
                    ContentId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodContentCategoryJoins", x => new { x.ContentId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_HoodContentCategoryJoins_HoodContentCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "HoodContentCategories",
                        principalColumn: "ContentCategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoodContentCategoryJoins_HoodContent_ContentId",
                        column: x => x.ContentId,
                        principalTable: "HoodContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodContentMedia",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlobReference = table.Column<string>(nullable: true),
                    ContentId = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Directory = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    FileType = table.Column<string>(nullable: true),
                    Filename = table.Column<string>(nullable: true),
                    GeneralFileType = table.Column<string>(nullable: true),
                    LargeUrl = table.Column<string>(nullable: true),
                    MediumUrl = table.Column<string>(nullable: true),
                    SmallUrl = table.Column<string>(nullable: true),
                    ThumbUrl = table.Column<string>(nullable: true),
                    UniqueId = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodContentMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodContentMedia_HoodContent_ContentId",
                        column: x => x.ContentId,
                        principalTable: "HoodContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodContentMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BaseValue = table.Column<string>(nullable: true),
                    ContentId = table.Column<int>(nullable: false),
                    IsStored = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodContentMetadata", x => x.Id);
                    table.UniqueConstraint("AK_HoodContentMetadata_ContentId_Name", x => new { x.ContentId, x.Name });
                    table.ForeignKey(
                        name: "FK_HoodContentMetadata_HoodContent_ContentId",
                        column: x => x.ContentId,
                        principalTable: "HoodContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodContentTagJoins",
                columns: table => new
                {
                    ContentId = table.Column<int>(nullable: false),
                    TagId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodContentTagJoins", x => new { x.ContentId, x.TagId });
                    table.ForeignKey(
                        name: "FK_HoodContentTagJoins_HoodContent_ContentId",
                        column: x => x.ContentId,
                        principalTable: "HoodContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoodContentTagJoins_HoodContentTags_TagId",
                        column: x => x.TagId,
                        principalTable: "HoodContentTags",
                        principalColumn: "Value",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodPropertyFloorplans",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlobReference = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Directory = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    FileType = table.Column<string>(nullable: true),
                    Filename = table.Column<string>(nullable: true),
                    GeneralFileType = table.Column<string>(nullable: true),
                    LargeUrl = table.Column<string>(nullable: true),
                    MediumUrl = table.Column<string>(nullable: true),
                    PropertyId = table.Column<int>(nullable: false),
                    SmallUrl = table.Column<string>(nullable: true),
                    ThumbUrl = table.Column<string>(nullable: true),
                    UniqueId = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodPropertyFloorplans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodPropertyFloorplans_HoodProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "HoodProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodPropertyMedia",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlobReference = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Directory = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    FileType = table.Column<string>(nullable: true),
                    Filename = table.Column<string>(nullable: true),
                    GeneralFileType = table.Column<string>(nullable: true),
                    LargeUrl = table.Column<string>(nullable: true),
                    MediumUrl = table.Column<string>(nullable: true),
                    PropertyId = table.Column<int>(nullable: false),
                    SmallUrl = table.Column<string>(nullable: true),
                    ThumbUrl = table.Column<string>(nullable: true),
                    UniqueId = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodPropertyMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodPropertyMedia_HoodProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "HoodProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodPropertyMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BaseValue = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    PropertyId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodPropertyMetadata", x => x.Id);
                    table.UniqueConstraint("AK_HoodPropertyMetadata_PropertyId_Name", x => new { x.PropertyId, x.Name });
                    table.ForeignKey(
                        name: "FK_HoodPropertyMetadata_HoodProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "HoodProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoodAddresses_UserId",
                table: "HoodAddresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoodContent_AuthorId",
                table: "HoodContent",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodContentCategories_ParentCategoryId",
                table: "HoodContentCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodContentCategoryJoins_CategoryId",
                table: "HoodContentCategoryJoins",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodContentMedia_ContentId",
                table: "HoodContentMedia",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodContentTagJoins_TagId",
                table: "HoodContentTagJoins",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodPropertyFloorplans_PropertyId",
                table: "HoodPropertyFloorplans",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodProperties_AgentId",
                table: "HoodProperties",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodPropertyMedia_PropertyId",
                table: "HoodPropertyMedia",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodSubscriptionFeatures_SubscriptionId",
                table: "HoodSubscriptionFeatures",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodUserSubscriptions_SubscriptionId",
                table: "HoodUserSubscriptions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodUserSubscriptions_UserId",
                table: "HoodUserSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoodAddresses");

            migrationBuilder.DropTable(
                name: "HoodContentCategoryJoins");

            migrationBuilder.DropTable(
                name: "HoodContentMedia");

            migrationBuilder.DropTable(
                name: "HoodContentMetadata");

            migrationBuilder.DropTable(
                name: "HoodContentTagJoins");

            migrationBuilder.DropTable(
                name: "HoodOptions");

            migrationBuilder.DropTable(
                name: "HoodPropertyFloorplans");

            migrationBuilder.DropTable(
                name: "HoodPropertyMedia");

            migrationBuilder.DropTable(
                name: "HoodPropertyMetadata");

            migrationBuilder.DropTable(
                name: "HoodMedia");

            migrationBuilder.DropTable(
                name: "HoodSubscriptionFeatures");

            migrationBuilder.DropTable(
                name: "HoodUserSubscriptions");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "HoodContentCategories");

            migrationBuilder.DropTable(
                name: "HoodContent");

            migrationBuilder.DropTable(
                name: "HoodContentTags");

            migrationBuilder.DropTable(
                name: "HoodProperties");

            migrationBuilder.DropTable(
                name: "HoodSubscriptions");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
