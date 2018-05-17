using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Hood.Web.Migrations
{
    public partial class ModerationSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "HoodForumTopics",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "HoodForumTopics",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedTime",
                table: "HoodForumTopics",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequirePostModeration",
                table: "HoodForums",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireTopicModeration",
                table: "HoodForums",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "HoodForumTopics");

            migrationBuilder.DropColumn(
                name: "ApprovedTime",
                table: "HoodForumTopics");

            migrationBuilder.DropColumn(
                name: "RequirePostModeration",
                table: "HoodForums");

            migrationBuilder.DropColumn(
                name: "RequireTopicModeration",
                table: "HoodForums");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "HoodForumTopics",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
