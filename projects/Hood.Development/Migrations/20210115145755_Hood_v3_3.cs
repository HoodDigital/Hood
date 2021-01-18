using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hood.Development.Migrations
{
    public partial class Hood_v3_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoodApiEvents");

            migrationBuilder.DropTable(
                name: "HoodForumCategoryJoins");

            migrationBuilder.DropTable(
                name: "HoodForumPosts");

            migrationBuilder.DropTable(
                name: "HoodSubscriptionFeatures");

            migrationBuilder.DropTable(
                name: "HoodUserSubscriptions");

            migrationBuilder.DropTable(
                name: "HoodApiKeys");

            migrationBuilder.DropTable(
                name: "HoodForumCategories");

            migrationBuilder.DropTable(
                name: "HoodForumTopics");

            migrationBuilder.DropTable(
                name: "HoodSubscriptions");

            migrationBuilder.DropTable(
                name: "HoodForums");

            migrationBuilder.DropTable(
                name: "HoodSubscriptionGroups");

            migrationBuilder.DropColumn(
                name: "StripeId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "HoodAddresses",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Address1",
                table: "HoodAddresses",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "HoodAddresses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address1",
                table: "HoodAddresses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "StripeId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HoodApiKeys",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessLevel = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
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
                name: "HoodForumCategories",
                columns: table => new
                {
                    ForumCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "HoodForums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Featured = table.Column<bool>(type: "bit", nullable: false),
                    FeaturedImageJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastEditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastEditedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastPostId = table.Column<long>(type: "bigint", nullable: true),
                    LastPosted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastTopicId = table.Column<int>(type: "int", nullable: true),
                    LastUserDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModeratedPostCount = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumPosts = table.Column<int>(type: "int", nullable: false),
                    NumTopics = table.Column<int>(type: "int", nullable: false),
                    PostingRequiresLogin = table.Column<bool>(type: "bit", nullable: false),
                    PostingRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostingSubscriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    RequirePostModeration = table.Column<bool>(type: "bit", nullable: false),
                    RequireTopicModeration = table.Column<bool>(type: "bit", nullable: false),
                    ShareCount = table.Column<int>(type: "int", nullable: false),
                    ShareImageJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserVars = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewingRequiresLogin = table.Column<bool>(type: "bit", nullable: false),
                    ViewingRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewingSubscriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Views = table.Column<int>(type: "int", nullable: false)
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
                name: "HoodSubscriptionGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeaturedImageJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeaturesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastEditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastEditedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Public = table.Column<bool>(type: "bit", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StripeId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodSubscriptionGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoodApiEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiKeyId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiredAccessLevel = table.Column<int>(type: "int", nullable: false),
                    RouteDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "HoodForumCategoryJoins",
                columns: table => new
                {
                    ForumId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllowReplies = table.Column<bool>(type: "bit", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthorDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Featured = table.Column<bool>(type: "bit", nullable: false),
                    FeaturedImageJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForumId = table.Column<int>(type: "int", nullable: false),
                    LastEditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastEditedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastPostId = table.Column<long>(type: "bigint", nullable: true),
                    LastPosted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastTopicId = table.Column<int>(type: "int", nullable: true),
                    LastUserDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModeratedPostCount = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumPosts = table.Column<int>(type: "int", nullable: false),
                    PostingRequiresLogin = table.Column<bool>(type: "bit", nullable: false),
                    PostingRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostingSubscriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    ShareCount = table.Column<int>(type: "int", nullable: false),
                    ShareImageJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserVars = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewingRequiresLogin = table.Column<bool>(type: "bit", nullable: false),
                    ViewingRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewingSubscriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Views = table.Column<int>(type: "int", nullable: false)
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
                name: "HoodSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Addon = table.Column<bool>(type: "bit", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Colour = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeaturedImageJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeaturesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Interval = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntervalCount = table.Column<int>(type: "int", nullable: false),
                    LastEditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastEditedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    LiveMode = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberAllowed = table.Column<int>(type: "int", nullable: false),
                    Public = table.Column<bool>(type: "bit", nullable: false),
                    StatementDescriptor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StripeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubscriptionProductId = table.Column<int>(type: "int", nullable: true),
                    TrialPeriodDays = table.Column<int>(type: "int", nullable: true)
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
                name: "HoodForumPosts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthorDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AuthorIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeleteReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DeletedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EditReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Edited = table.Column<bool>(type: "bit", nullable: false),
                    EditedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EditedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReplyId = table.Column<long>(type: "bigint", nullable: true),
                    Signature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TopicId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "HoodSubscriptionFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    UserSubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CancelAtPeriodEnd = table.Column<bool>(type: "bit", nullable: false),
                    CanceledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Confirmed = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    Start = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StripeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    TaxPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TrialEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrialStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_HoodApiEvents_ApiKeyId",
                table: "HoodApiEvents",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodApiKeys_UserId",
                table: "HoodApiKeys",
                column: "UserId");

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
    }
}
