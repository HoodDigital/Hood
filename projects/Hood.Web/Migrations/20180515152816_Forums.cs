using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Hood.Web.Migrations
{
    public partial class Forums : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ForumSignature",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HoodForumCategories",
                columns: table => new
                {
                    ForumCategoryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DisplayName = table.Column<string>(nullable: true),
                    ParentCategoryId = table.Column<int>(nullable: true),
                    Slug = table.Column<string>(nullable: true)
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AuthorDisplayName = table.Column<string>(nullable: true),
                    AuthorId = table.Column<string>(nullable: true),
                    AuthorName = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Featured = table.Column<bool>(nullable: false),
                    FeaturedImageJson = table.Column<string>(nullable: true),
                    LastEditedBy = table.Column<string>(nullable: true),
                    LastEditedOn = table.Column<DateTime>(nullable: false),
                    LastPostId = table.Column<long>(nullable: true),
                    LastPosted = table.Column<DateTime>(nullable: true),
                    LastTopicId = table.Column<int>(nullable: true),
                    LastUserDisplayName = table.Column<string>(nullable: true),
                    LastUserId = table.Column<string>(nullable: true),
                    LastUserName = table.Column<string>(nullable: true),
                    ModeratedPostCount = table.Column<int>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    NumPosts = table.Column<int>(nullable: false),
                    NumTopics = table.Column<int>(nullable: false),
                    Public = table.Column<bool>(nullable: false),
                    Published = table.Column<bool>(nullable: false),
                    ShareCount = table.Column<int>(nullable: false),
                    ShareImageJson = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    SystemNotes = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: false),
                    UserVars = table.Column<string>(nullable: true),
                    Views = table.Column<int>(nullable: false)
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
                name: "HoodForumCategoryJoins",
                columns: table => new
                {
                    ForumId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false)
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
                    AllowReplies = table.Column<bool>(nullable: false),
                    AuthorDisplayName = table.Column<string>(nullable: true),
                    AuthorId = table.Column<string>(nullable: true),
                    AuthorName = table.Column<string>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Featured = table.Column<bool>(nullable: false),
                    FeaturedImageJson = table.Column<string>(nullable: true),
                    ForumId = table.Column<int>(nullable: false),
                    LastEditedBy = table.Column<string>(nullable: true),
                    LastEditedOn = table.Column<DateTime>(nullable: false),
                    LastPostId = table.Column<long>(nullable: true),
                    LastPosted = table.Column<DateTime>(nullable: true),
                    LastTopicId = table.Column<int>(nullable: true),
                    LastUserDisplayName = table.Column<string>(nullable: true),
                    LastUserId = table.Column<string>(nullable: true),
                    LastUserName = table.Column<string>(nullable: true),
                    ModeratedPostCount = table.Column<int>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    NumPosts = table.Column<int>(nullable: false),
                    Public = table.Column<bool>(nullable: false),
                    Published = table.Column<bool>(nullable: false),
                    ShareCount = table.Column<int>(nullable: false),
                    ShareImageJson = table.Column<string>(nullable: true),
                    SystemNotes = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: false),
                    UserVars = table.Column<string>(nullable: true),
                    Views = table.Column<int>(nullable: false)
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
                name: "HoodForumPosts",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Approved = table.Column<bool>(nullable: false),
                    ApprovedTime = table.Column<DateTime>(nullable: true),
                    AuthorDisplayName = table.Column<string>(nullable: true),
                    AuthorId = table.Column<string>(nullable: true),
                    AuthorIp = table.Column<string>(nullable: true),
                    AuthorName = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    DeleteReason = table.Column<string>(nullable: true),
                    Deleted = table.Column<bool>(nullable: false),
                    DeletedById = table.Column<string>(nullable: true),
                    DeletedTime = table.Column<DateTime>(nullable: true),
                    EditReason = table.Column<string>(nullable: true),
                    Edited = table.Column<bool>(nullable: false),
                    EditedById = table.Column<string>(nullable: true),
                    EditedTime = table.Column<DateTime>(nullable: true),
                    PostedTime = table.Column<DateTime>(nullable: false),
                    ReplyId = table.Column<long>(nullable: true),
                    Signature = table.Column<string>(nullable: true),
                    TopicId = table.Column<int>(nullable: false)
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoodForumCategoryJoins");

            migrationBuilder.DropTable(
                name: "HoodForumPosts");

            migrationBuilder.DropTable(
                name: "HoodForumCategories");

            migrationBuilder.DropTable(
                name: "HoodForumTopics");

            migrationBuilder.DropTable(
                name: "HoodForums");

            migrationBuilder.DropColumn(
                name: "ForumSignature",
                table: "AspNetUsers");
        }
    }
}
