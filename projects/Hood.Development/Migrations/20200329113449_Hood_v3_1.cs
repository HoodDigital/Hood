using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hood.Development.Migrations
{
    public partial class Hood_v3_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    LastLogOn = table.Column<DateTime>(nullable: false),
                    LastLoginIP = table.Column<string>(nullable: true),
                    LastLoginLocation = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    Longitude = table.Column<string>(nullable: true),
                    Anonymous = table.Column<bool>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    BillingAddressJson = table.Column<string>(nullable: true),
                    DeliveryAddressJson = table.Column<string>(nullable: true),
                    StripeId = table.Column<string>(nullable: true),
                    AvatarJson = table.Column<string>(nullable: true),
                    UserVars = table.Column<string>(nullable: true)
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
                    DisplayName = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    ParentCategoryId = table.Column<int>(nullable: true)
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
                name: "HoodForumCategories",
                columns: table => new
                {
                    ForumCategoryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DisplayName = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    ParentCategoryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodForumCategories", x => x.ForumCategoryId);
                    table.ForeignKey(
                        name: "FK_HoodForumCategories_HoodForumCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "HoodForumCategories",
                        principalColumn: "ForumCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoodMediaDirectories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DisplayName = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    OwnerId = table.Column<string>(nullable: true),
                    ParentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodMediaDirectories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodMediaDirectories_HoodMediaDirectories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "HoodMediaDirectories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "HoodSubscriptionGroups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DisplayName = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    Public = table.Column<bool>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    LastEditedOn = table.Column<DateTime>(nullable: false),
                    LastEditedBy = table.Column<string>(nullable: true),
                    StripeId = table.Column<string>(nullable: true),
                    FeaturedImageJson = table.Column<string>(nullable: true),
                    FeaturesJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodSubscriptionGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
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
                name: "AspNetUserAccessCodes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(nullable: true),
                    Expiry = table.Column<DateTime>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    Used = table.Column<bool>(nullable: false),
                    DateUsed = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserAccessCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserAccessCodes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
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
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ContactName = table.Column<string>(nullable: true),
                    QuickName = table.Column<string>(nullable: true),
                    Number = table.Column<string>(nullable: false),
                    Address1 = table.Column<string>(nullable: true),
                    Address2 = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: false),
                    County = table.Column<string>(nullable: false),
                    Country = table.Column<string>(nullable: false),
                    Postcode = table.Column<string>(nullable: false),
                    Latitude = table.Column<double>(nullable: false, defaultValueSql: "0.0"),
                    Longitude = table.Column<double>(nullable: false, defaultValueSql: "0.0"),
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
                name: "HoodApiKeys",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Key = table.Column<string>(nullable: true),
                    Active = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    AccessLevel = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodApiKeys_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodContent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(nullable: false),
                    Excerpt = table.Column<string>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    ParentId = table.Column<int>(nullable: true),
                    PublishDate = table.Column<DateTime>(nullable: false),
                    ContentType = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    LastEditedOn = table.Column<DateTime>(nullable: false),
                    LastEditedBy = table.Column<string>(nullable: true),
                    UserVars = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    SystemNotes = table.Column<string>(nullable: true),
                    Views = table.Column<int>(nullable: false),
                    ShareCount = table.Column<int>(nullable: false),
                    AllowComments = table.Column<bool>(nullable: false),
                    Public = table.Column<bool>(nullable: false),
                    Featured = table.Column<bool>(nullable: false),
                    AuthorId = table.Column<string>(nullable: true),
                    FeaturedImageJson = table.Column<string>(nullable: true),
                    ShareImageJson = table.Column<string>(nullable: true)
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
                name: "HoodForums",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ViewingRequiresLogin = table.Column<bool>(nullable: false),
                    ViewingSubscriptions = table.Column<string>(nullable: true),
                    ViewingRoles = table.Column<string>(nullable: true),
                    PostingRequiresLogin = table.Column<bool>(nullable: false),
                    PostingSubscriptions = table.Column<string>(nullable: true),
                    PostingRoles = table.Column<string>(nullable: true),
                    AuthorId = table.Column<string>(nullable: true),
                    AuthorName = table.Column<string>(nullable: true),
                    AuthorDisplayName = table.Column<string>(nullable: true),
                    AuthorRoles = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    LastEditedOn = table.Column<DateTime>(nullable: false),
                    LastEditedBy = table.Column<string>(nullable: true),
                    LastPosted = table.Column<DateTime>(nullable: true),
                    LastTopicId = table.Column<int>(nullable: true),
                    LastPostId = table.Column<long>(nullable: true),
                    LastUserId = table.Column<string>(nullable: true),
                    LastUserName = table.Column<string>(nullable: true),
                    LastUserDisplayName = table.Column<string>(nullable: true),
                    UserVars = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    SystemNotes = table.Column<string>(nullable: true),
                    Views = table.Column<int>(nullable: false),
                    ShareCount = table.Column<int>(nullable: false),
                    Published = table.Column<bool>(nullable: false),
                    Featured = table.Column<bool>(nullable: false),
                    FeaturedImageJson = table.Column<string>(nullable: true),
                    ShareImageJson = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    NumTopics = table.Column<int>(nullable: false),
                    NumPosts = table.Column<int>(nullable: false),
                    ModeratedPostCount = table.Column<int>(nullable: false),
                    RequireTopicModeration = table.Column<bool>(nullable: false),
                    RequirePostModeration = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodForums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodForums_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoodLogs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Time = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Detail = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    Source = table.Column<string>(nullable: true),
                    SourceUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
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
                    Title = table.Column<string>(nullable: true),
                    Reference = table.Column<string>(nullable: true),
                    Tags = table.Column<string>(nullable: true),
                    ContactName = table.Column<string>(nullable: true),
                    Number = table.Column<string>(nullable: true),
                    Address1 = table.Column<string>(nullable: true),
                    Address2 = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    County = table.Column<string>(nullable: true),
                    Postcode = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    Latitude = table.Column<double>(nullable: false, defaultValueSql: "0.0"),
                    Longitude = table.Column<double>(nullable: false, defaultValueSql: "0.0"),
                    Status = table.Column<int>(nullable: false),
                    PublishDate = table.Column<DateTime>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    LastEditedOn = table.Column<DateTime>(nullable: false),
                    LastEditedBy = table.Column<string>(nullable: true),
                    UserVars = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    SystemNotes = table.Column<string>(nullable: true),
                    AllowComments = table.Column<bool>(nullable: false),
                    Public = table.Column<bool>(nullable: false),
                    Views = table.Column<int>(nullable: false),
                    ShareCount = table.Column<int>(nullable: false),
                    FeaturedImageJson = table.Column<string>(nullable: true),
                    InfoDownloadJson = table.Column<string>(nullable: true),
                    ListingType = table.Column<string>(nullable: true),
                    LeaseStatus = table.Column<string>(nullable: true),
                    PropertyType = table.Column<string>(nullable: true),
                    Size = table.Column<string>(nullable: true),
                    Bedrooms = table.Column<int>(nullable: false),
                    Confidential = table.Column<bool>(nullable: false),
                    Featured = table.Column<bool>(nullable: false),
                    ShortDescription = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Additional = table.Column<string>(nullable: true),
                    Lease = table.Column<string>(nullable: true),
                    Areas = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: true),
                    AgentInfo = table.Column<string>(nullable: true),
                    Planning = table.Column<string>(nullable: true),
                    Rent = table.Column<decimal>(nullable: true),
                    AskingPrice = table.Column<decimal>(nullable: true),
                    Premium = table.Column<decimal>(nullable: true),
                    Fees = table.Column<decimal>(nullable: true),
                    RentDisplay = table.Column<string>(nullable: true),
                    AskingPriceDisplay = table.Column<string>(nullable: true),
                    PremiumDisplay = table.Column<string>(nullable: true),
                    FeesDisplay = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    QuickName = table.Column<string>(nullable: true),
                    Addressee = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    AgentId = table.Column<string>(nullable: true),
                    Floors = table.Column<string>(nullable: true)
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
                name: "HoodMedia",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FileSize = table.Column<long>(nullable: false),
                    FileType = table.Column<string>(nullable: true),
                    Filename = table.Column<string>(nullable: true),
                    BlobReference = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    ThumbUrl = table.Column<string>(nullable: true),
                    SmallUrl = table.Column<string>(nullable: true),
                    MediumUrl = table.Column<string>(nullable: true),
                    LargeUrl = table.Column<string>(nullable: true),
                    UniqueId = table.Column<string>(nullable: true),
                    GenericFileType = table.Column<int>(nullable: false),
                    DirectoryId = table.Column<int>(nullable: true),
                    Directory = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodMedia_HoodMediaDirectories_DirectoryId",
                        column: x => x.DirectoryId,
                        principalTable: "HoodMediaDirectories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoodSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    StripeId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Colour = table.Column<string>(nullable: true),
                    Public = table.Column<bool>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    Addon = table.Column<bool>(nullable: false),
                    NumberAllowed = table.Column<int>(nullable: false),
                    Amount = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Currency = table.Column<string>(nullable: true),
                    Interval = table.Column<string>(nullable: true),
                    IntervalCount = table.Column<int>(nullable: false),
                    LiveMode = table.Column<bool>(nullable: false),
                    StatementDescriptor = table.Column<string>(nullable: true),
                    TrialPeriodDays = table.Column<int>(nullable: true),
                    FeaturedImageJson = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    LastEditedOn = table.Column<DateTime>(nullable: false),
                    LastEditedBy = table.Column<string>(nullable: true),
                    SubscriptionProductId = table.Column<int>(nullable: true),
                    FeaturesJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodSubscriptions_HoodSubscriptionGroups_SubscriptionProductId",
                        column: x => x.SubscriptionProductId,
                        principalTable: "HoodSubscriptionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoodApiEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ApiKeyId = table.Column<string>(nullable: true),
                    Time = table.Column<DateTime>(nullable: false),
                    IpAddress = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Action = table.Column<string>(nullable: true),
                    RouteDataJson = table.Column<string>(nullable: true),
                    RequiredAccessLevel = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodApiEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodApiEvents_HoodApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "HoodApiKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodContentCategoryJoins",
                columns: table => new
                {
                    CategoryId = table.Column<int>(nullable: false),
                    ContentId = table.Column<int>(nullable: false)
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
                    FileSize = table.Column<long>(nullable: false),
                    FileType = table.Column<string>(nullable: true),
                    Filename = table.Column<string>(nullable: true),
                    BlobReference = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    ThumbUrl = table.Column<string>(nullable: true),
                    SmallUrl = table.Column<string>(nullable: true),
                    MediumUrl = table.Column<string>(nullable: true),
                    LargeUrl = table.Column<string>(nullable: true),
                    UniqueId = table.Column<string>(nullable: true),
                    Directory = table.Column<string>(nullable: true),
                    GenericFileType = table.Column<int>(nullable: false),
                    ContentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodContentMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodContentMedia_HoodContent_ContentId",
                        column: x => x.ContentId,
                        principalTable: "HoodContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoodContentMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BaseValue = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    ContentId = table.Column<int>(nullable: false)
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
                name: "HoodForumCategoryJoins",
                columns: table => new
                {
                    CategoryId = table.Column<int>(nullable: false),
                    ForumId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodForumCategoryJoins", x => new { x.ForumId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_HoodForumCategoryJoins_HoodForumCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "HoodForumCategories",
                        principalColumn: "ForumCategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoodForumCategoryJoins_HoodForums_ForumId",
                        column: x => x.ForumId,
                        principalTable: "HoodForums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodForumTopics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ViewingRequiresLogin = table.Column<bool>(nullable: false),
                    ViewingSubscriptions = table.Column<string>(nullable: true),
                    ViewingRoles = table.Column<string>(nullable: true),
                    PostingRequiresLogin = table.Column<bool>(nullable: false),
                    PostingSubscriptions = table.Column<string>(nullable: true),
                    PostingRoles = table.Column<string>(nullable: true),
                    AuthorId = table.Column<string>(nullable: true),
                    AuthorName = table.Column<string>(nullable: true),
                    AuthorDisplayName = table.Column<string>(nullable: true),
                    AuthorRoles = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    LastEditedOn = table.Column<DateTime>(nullable: false),
                    LastEditedBy = table.Column<string>(nullable: true),
                    LastPosted = table.Column<DateTime>(nullable: true),
                    LastTopicId = table.Column<int>(nullable: true),
                    LastPostId = table.Column<long>(nullable: true),
                    LastUserId = table.Column<string>(nullable: true),
                    LastUserName = table.Column<string>(nullable: true),
                    LastUserDisplayName = table.Column<string>(nullable: true),
                    UserVars = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    SystemNotes = table.Column<string>(nullable: true),
                    Views = table.Column<int>(nullable: false),
                    ShareCount = table.Column<int>(nullable: false),
                    Published = table.Column<bool>(nullable: false),
                    Featured = table.Column<bool>(nullable: false),
                    FeaturedImageJson = table.Column<string>(nullable: true),
                    ShareImageJson = table.Column<string>(nullable: true),
                    ForumId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Approved = table.Column<bool>(nullable: false),
                    ApprovedTime = table.Column<DateTime>(nullable: true),
                    NumPosts = table.Column<int>(nullable: false),
                    ModeratedPostCount = table.Column<int>(nullable: false),
                    AllowReplies = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodForumTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodForumTopics_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoodForumTopics_HoodForums_ForumId",
                        column: x => x.ForumId,
                        principalTable: "HoodForums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoodPropertyFloorplans",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FileSize = table.Column<long>(nullable: false),
                    FileType = table.Column<string>(nullable: true),
                    Filename = table.Column<string>(nullable: true),
                    BlobReference = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    ThumbUrl = table.Column<string>(nullable: true),
                    SmallUrl = table.Column<string>(nullable: true),
                    MediumUrl = table.Column<string>(nullable: true),
                    LargeUrl = table.Column<string>(nullable: true),
                    UniqueId = table.Column<string>(nullable: true),
                    Directory = table.Column<string>(nullable: true),
                    GenericFileType = table.Column<int>(nullable: false),
                    PropertyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodPropertyFloorplans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodPropertyFloorplans_HoodProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "HoodProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoodPropertyMedia",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FileSize = table.Column<long>(nullable: false),
                    FileType = table.Column<string>(nullable: true),
                    Filename = table.Column<string>(nullable: true),
                    BlobReference = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    ThumbUrl = table.Column<string>(nullable: true),
                    SmallUrl = table.Column<string>(nullable: true),
                    MediumUrl = table.Column<string>(nullable: true),
                    LargeUrl = table.Column<string>(nullable: true),
                    UniqueId = table.Column<string>(nullable: true),
                    Directory = table.Column<string>(nullable: true),
                    GenericFileType = table.Column<int>(nullable: false),
                    PropertyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodPropertyMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodPropertyMedia_HoodProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "HoodProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoodPropertyMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BaseValue = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    PropertyId = table.Column<int>(nullable: false)
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoodSubscriptionFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BaseValue = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    SubscriptionId = table.Column<int>(nullable: false)
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
                    Confirmed = table.Column<bool>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    StripeId = table.Column<string>(nullable: true),
                    CancelAtPeriodEnd = table.Column<bool>(nullable: false),
                    CanceledAt = table.Column<DateTime>(nullable: true),
                    Created = table.Column<DateTime>(nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(nullable: true),
                    CurrentPeriodStart = table.Column<DateTime>(nullable: true),
                    CustomerId = table.Column<string>(nullable: true),
                    EndedAt = table.Column<DateTime>(nullable: true),
                    Quantity = table.Column<long>(nullable: false),
                    Start = table.Column<DateTime>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    TaxPercent = table.Column<decimal>(nullable: true),
                    TrialEnd = table.Column<DateTime>(nullable: true),
                    TrialStart = table.Column<DateTime>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: false),
                    DeletedAt = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    SubscriptionId = table.Column<int>(nullable: false)
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
                name: "HoodForumPosts",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TopicId = table.Column<int>(nullable: false),
                    ReplyId = table.Column<long>(nullable: true),
                    AuthorId = table.Column<string>(nullable: true),
                    AuthorName = table.Column<string>(nullable: true),
                    AuthorDisplayName = table.Column<string>(nullable: true),
                    AuthorIp = table.Column<string>(nullable: true),
                    AuthorRoles = table.Column<string>(nullable: true),
                    PostedTime = table.Column<DateTime>(nullable: false),
                    Body = table.Column<string>(nullable: false),
                    Signature = table.Column<string>(nullable: true),
                    Approved = table.Column<bool>(nullable: false),
                    ApprovedTime = table.Column<DateTime>(nullable: true),
                    Edited = table.Column<bool>(nullable: false),
                    EditReason = table.Column<string>(nullable: true),
                    EditedTime = table.Column<DateTime>(nullable: true),
                    EditedById = table.Column<string>(nullable: true),
                    Deleted = table.Column<bool>(nullable: false),
                    DeleteReason = table.Column<string>(nullable: true),
                    DeletedTime = table.Column<DateTime>(nullable: true),
                    DeletedById = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodForumPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodForumPosts_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoodForumPosts_AspNetUsers_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoodForumPosts_AspNetUsers_EditedById",
                        column: x => x.EditedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoodForumPosts_HoodForumPosts_ReplyId",
                        column: x => x.ReplyId,
                        principalTable: "HoodForumPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoodForumPosts_HoodForumTopics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "HoodForumTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserAccessCodes_UserId",
                table: "AspNetUserAccessCodes",
                column: "UserId");

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

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HoodAddresses_UserId",
                table: "HoodAddresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodApiEvents_ApiKeyId",
                table: "HoodApiEvents",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodApiKeys_UserId",
                table: "HoodApiKeys",
                column: "UserId");

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
                name: "IX_HoodForumCategories_ParentCategoryId",
                table: "HoodForumCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodForumCategoryJoins_CategoryId",
                table: "HoodForumCategoryJoins",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodForumPosts_AuthorId",
                table: "HoodForumPosts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodForumPosts_DeletedById",
                table: "HoodForumPosts",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_HoodForumPosts_EditedById",
                table: "HoodForumPosts",
                column: "EditedById");

            migrationBuilder.CreateIndex(
                name: "IX_HoodForumPosts_ReplyId",
                table: "HoodForumPosts",
                column: "ReplyId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodForumPosts_TopicId",
                table: "HoodForumPosts",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodForums_AuthorId",
                table: "HoodForums",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodForumTopics_AuthorId",
                table: "HoodForumTopics",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodForumTopics_ForumId",
                table: "HoodForumTopics",
                column: "ForumId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodLogs_UserId",
                table: "HoodLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodMedia_DirectoryId",
                table: "HoodMedia",
                column: "DirectoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodMediaDirectories_ParentId",
                table: "HoodMediaDirectories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodProperties_AgentId",
                table: "HoodProperties",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodPropertyFloorplans_PropertyId",
                table: "HoodPropertyFloorplans",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodPropertyMedia_PropertyId",
                table: "HoodPropertyMedia",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodSubscriptionFeatures_SubscriptionId",
                table: "HoodSubscriptionFeatures",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodSubscriptions_SubscriptionProductId",
                table: "HoodSubscriptions",
                column: "SubscriptionProductId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodUserSubscriptions_SubscriptionId",
                table: "HoodUserSubscriptions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodUserSubscriptions_UserId",
                table: "HoodUserSubscriptions",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserAccessCodes");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "HoodAddresses");

            migrationBuilder.DropTable(
                name: "HoodApiEvents");

            migrationBuilder.DropTable(
                name: "HoodContentCategoryJoins");

            migrationBuilder.DropTable(
                name: "HoodContentMedia");

            migrationBuilder.DropTable(
                name: "HoodContentMetadata");

            migrationBuilder.DropTable(
                name: "HoodForumCategoryJoins");

            migrationBuilder.DropTable(
                name: "HoodForumPosts");

            migrationBuilder.DropTable(
                name: "HoodLogs");

            migrationBuilder.DropTable(
                name: "HoodMedia");

            migrationBuilder.DropTable(
                name: "HoodOptions");

            migrationBuilder.DropTable(
                name: "HoodPropertyFloorplans");

            migrationBuilder.DropTable(
                name: "HoodPropertyMedia");

            migrationBuilder.DropTable(
                name: "HoodPropertyMetadata");

            migrationBuilder.DropTable(
                name: "HoodSubscriptionFeatures");

            migrationBuilder.DropTable(
                name: "HoodUserSubscriptions");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "HoodApiKeys");

            migrationBuilder.DropTable(
                name: "HoodContentCategories");

            migrationBuilder.DropTable(
                name: "HoodContent");

            migrationBuilder.DropTable(
                name: "HoodForumCategories");

            migrationBuilder.DropTable(
                name: "HoodForumTopics");

            migrationBuilder.DropTable(
                name: "HoodMediaDirectories");

            migrationBuilder.DropTable(
                name: "HoodProperties");

            migrationBuilder.DropTable(
                name: "HoodSubscriptions");

            migrationBuilder.DropTable(
                name: "HoodForums");

            migrationBuilder.DropTable(
                name: "HoodSubscriptionGroups");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
