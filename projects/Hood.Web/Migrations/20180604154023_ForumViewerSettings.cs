using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Hood.Web.Migrations
{
    public partial class ForumViewerSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Public",
                table: "HoodForumTopics",
                newName: "ViewingRequiresSubscription");

            migrationBuilder.RenameColumn(
                name: "Public",
                table: "HoodForums",
                newName: "ViewingRequiresSubscription");

            migrationBuilder.AddColumn<int>(
                name: "ForumId",
                table: "HoodSubscriptions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TopicId",
                table: "HoodSubscriptions",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PostingRequiresLogin",
                table: "HoodForumTopics",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PostingRequiresSubscription",
                table: "HoodForumTopics",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PostingRoles",
                table: "HoodForumTopics",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostingSubscriptions",
                table: "HoodForumTopics",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ViewingRequiresLogin",
                table: "HoodForumTopics",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ViewingRoles",
                table: "HoodForumTopics",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViewingSubscriptions",
                table: "HoodForumTopics",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PostingRequiresLogin",
                table: "HoodForums",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PostingRequiresSubscription",
                table: "HoodForums",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PostingRoles",
                table: "HoodForums",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostingSubscriptions",
                table: "HoodForums",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostingSusbcriptions",
                table: "HoodForums",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ViewingRequiresLogin",
                table: "HoodForums",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ViewingRoles",
                table: "HoodForums",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViewingSubscriptions",
                table: "HoodForums",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViewingSusbcriptions",
                table: "HoodForums",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ForumId",
                table: "AspNetRoles",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TopicId",
                table: "AspNetRoles",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoodSubscriptions_ForumId",
                table: "HoodSubscriptions",
                column: "ForumId");

            migrationBuilder.CreateIndex(
                name: "IX_HoodSubscriptions_TopicId",
                table: "HoodSubscriptions",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_ForumId",
                table: "AspNetRoles",
                column: "ForumId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_TopicId",
                table: "AspNetRoles",
                column: "TopicId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_HoodForums_ForumId",
                table: "AspNetRoles",
                column: "ForumId",
                principalTable: "HoodForums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_HoodForumTopics_TopicId",
                table: "AspNetRoles",
                column: "TopicId",
                principalTable: "HoodForumTopics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HoodSubscriptions_HoodForums_ForumId",
                table: "HoodSubscriptions",
                column: "ForumId",
                principalTable: "HoodForums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HoodSubscriptions_HoodForumTopics_TopicId",
                table: "HoodSubscriptions",
                column: "TopicId",
                principalTable: "HoodForumTopics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_HoodForums_ForumId",
                table: "AspNetRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_HoodForumTopics_TopicId",
                table: "AspNetRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_HoodSubscriptions_HoodForums_ForumId",
                table: "HoodSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_HoodSubscriptions_HoodForumTopics_TopicId",
                table: "HoodSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_HoodSubscriptions_ForumId",
                table: "HoodSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_HoodSubscriptions_TopicId",
                table: "HoodSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_ForumId",
                table: "AspNetRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_TopicId",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "ForumId",
                table: "HoodSubscriptions");

            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "HoodSubscriptions");

            migrationBuilder.DropColumn(
                name: "PostingRequiresLogin",
                table: "HoodForumTopics");

            migrationBuilder.DropColumn(
                name: "PostingRequiresSubscription",
                table: "HoodForumTopics");

            migrationBuilder.DropColumn(
                name: "PostingRoles",
                table: "HoodForumTopics");

            migrationBuilder.DropColumn(
                name: "PostingSubscriptions",
                table: "HoodForumTopics");

            migrationBuilder.DropColumn(
                name: "ViewingRequiresLogin",
                table: "HoodForumTopics");

            migrationBuilder.DropColumn(
                name: "ViewingRoles",
                table: "HoodForumTopics");

            migrationBuilder.DropColumn(
                name: "ViewingSubscriptions",
                table: "HoodForumTopics");

            migrationBuilder.DropColumn(
                name: "PostingRequiresLogin",
                table: "HoodForums");

            migrationBuilder.DropColumn(
                name: "PostingRequiresSubscription",
                table: "HoodForums");

            migrationBuilder.DropColumn(
                name: "PostingRoles",
                table: "HoodForums");

            migrationBuilder.DropColumn(
                name: "PostingSubscriptions",
                table: "HoodForums");

            migrationBuilder.DropColumn(
                name: "PostingSusbcriptions",
                table: "HoodForums");

            migrationBuilder.DropColumn(
                name: "ViewingRequiresLogin",
                table: "HoodForums");

            migrationBuilder.DropColumn(
                name: "ViewingRoles",
                table: "HoodForums");

            migrationBuilder.DropColumn(
                name: "ViewingSubscriptions",
                table: "HoodForums");

            migrationBuilder.DropColumn(
                name: "ViewingSusbcriptions",
                table: "HoodForums");

            migrationBuilder.DropColumn(
                name: "ForumId",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "AspNetRoles");

            migrationBuilder.RenameColumn(
                name: "ViewingRequiresSubscription",
                table: "HoodForumTopics",
                newName: "Public");

            migrationBuilder.RenameColumn(
                name: "ViewingRequiresSubscription",
                table: "HoodForums",
                newName: "Public");
        }
    }
}
