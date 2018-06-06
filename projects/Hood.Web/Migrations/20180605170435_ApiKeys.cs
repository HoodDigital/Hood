using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Hood.Web.Migrations
{
    public partial class ApiKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logs_AspNetUsers_UserId",
                table: "Logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Logs",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "PostingSusbcriptions",
                table: "HoodForums");

            migrationBuilder.DropColumn(
                name: "ViewingSusbcriptions",
                table: "HoodForums");

            migrationBuilder.RenameTable(
                name: "Logs",
                newName: "HoodLogs");

            migrationBuilder.RenameIndex(
                name: "IX_Logs_UserId",
                table: "HoodLogs",
                newName: "IX_HoodLogs_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HoodLogs",
                table: "HoodLogs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "HoodApiKeys",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AccessLevel = table.Column<int>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Key = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoodApiEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Action = table.Column<string>(nullable: true),
                    ApiKeyId = table.Column<string>(nullable: true),
                    IpAddress = table.Column<string>(nullable: true),
                    RequiredAccessLevel = table.Column<int>(nullable: false),
                    RouteDataJson = table.Column<string>(nullable: true),
                    Time = table.Column<DateTime>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoodApiEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoodApiEvents_HoodApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "HoodApiKeys",
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

            migrationBuilder.AddForeignKey(
                name: "FK_HoodLogs_AspNetUsers_UserId",
                table: "HoodLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoodLogs_AspNetUsers_UserId",
                table: "HoodLogs");

            migrationBuilder.DropTable(
                name: "HoodApiEvents");

            migrationBuilder.DropTable(
                name: "HoodApiKeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HoodLogs",
                table: "HoodLogs");

            migrationBuilder.RenameTable(
                name: "HoodLogs",
                newName: "Logs");

            migrationBuilder.RenameIndex(
                name: "IX_HoodLogs_UserId",
                table: "Logs",
                newName: "IX_Logs_UserId");

            migrationBuilder.AddColumn<string>(
                name: "PostingSusbcriptions",
                table: "HoodForums",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViewingSusbcriptions",
                table: "HoodForums",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Logs",
                table: "Logs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Logs_AspNetUsers_UserId",
                table: "Logs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
