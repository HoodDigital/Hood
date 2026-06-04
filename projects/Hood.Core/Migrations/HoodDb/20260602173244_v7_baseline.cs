using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hood.Core.Migrations.HoodDb
{
    /// <inheritdoc />
    public partial class v7_baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HoodLogs",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodLogs", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "HoodMediaDirectories",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodMediaDirectories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodMediaDirectories_HoodMediaDirectories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "HoodMediaDirectories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "HoodOptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodOptions", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "HoodMedia",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DirectoryId = table.Column<int>(type: "int", nullable: true),
                    Directory = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    GenericFileType = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodMedia_HoodMediaDirectories_DirectoryId",
                        column: x => x.DirectoryId,
                        principalTable: "HoodMediaDirectories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_HoodMedia_DirectoryId",
                table: "HoodMedia",
                column: "DirectoryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_HoodMediaDirectories_ParentId",
                table: "HoodMediaDirectories",
                column: "ParentId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "HoodLogs");

            migrationBuilder.DropTable(name: "HoodMedia");

            migrationBuilder.DropTable(name: "HoodOptions");

            migrationBuilder.DropTable(name: "HoodMediaDirectories");
        }
    }
}
