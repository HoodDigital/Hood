using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hood.Core.Migrations.Content
{
    public partial class v61 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HoodContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Excerpt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    PublishDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastEditedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Views = table.Column<int>(type: "int", nullable: false),
                    ShareCount = table.Column<int>(type: "int", nullable: false),
                    AllowComments = table.Column<bool>(type: "bit", nullable: false),
                    Public = table.Column<bool>(type: "bit", nullable: false),
                    Featured = table.Column<bool>(type: "bit", nullable: false),
                    AuthorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeaturedImageJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShareImageJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoodContentCategories",
                columns: table => new
                {
                    ContentCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodContentCategories", x => x.ContentCategoryId);
                    table.ForeignKey(
                        name: "FK_HoodContentCategories_HoodContentCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "HoodContentCategories",
                        principalColumn: "ContentCategoryId");
                });

            migrationBuilder.CreateTable(
                name: "HoodContentMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContentId = table.Column<int>(type: "int", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Filename = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BlobReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThumbUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmallUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediumUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LargeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UniqueId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Directory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GenericFileType = table.Column<int>(type: "int", nullable: false)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContentId = table.Column<int>(type: "int", nullable: false),
                    BaseValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "HoodContentCategoryJoins",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ContentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodContentCategoryJoins", x => new { x.ContentId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_HoodContentCategoryJoins_HoodContent_ContentId",
                        column: x => x.ContentId,
                        principalTable: "HoodContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoodContentCategoryJoins_HoodContentCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "HoodContentCategories",
                        principalColumn: "ContentCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoodContentCategoryJoins");

            migrationBuilder.DropTable(
                name: "HoodContentMedia");

            migrationBuilder.DropTable(
                name: "HoodContentMetadata");

            migrationBuilder.DropTable(
                name: "HoodContentCategories");

            migrationBuilder.DropTable(
                name: "HoodContent");
        }
    }
}
