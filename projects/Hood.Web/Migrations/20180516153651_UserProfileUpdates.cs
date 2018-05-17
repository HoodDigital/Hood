using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Hood.Web.Migrations
{
    public partial class UserProfileUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VATNumber",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "EmailOptin",
                table: "AspNetUsers",
                newName: "Anonymous");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Anonymous",
                table: "AspNetUsers",
                newName: "EmailOptin");

            migrationBuilder.AddColumn<string>(
                name: "ClientCode",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VATNumber",
                table: "AspNetUsers",
                nullable: true);
        }
    }
}
