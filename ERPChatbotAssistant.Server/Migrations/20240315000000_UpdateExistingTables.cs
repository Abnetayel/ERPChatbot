using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERPChatbotAssistant.Server.Migrations
{
    public partial class UpdateExistingTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing foreign key constraints
            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_Users_UserId",
                table: "ChatSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChatSessions_SessionId",
                table: "Messages");

            // Modify ChatSessions table
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ChatSessions");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ChatSessions",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityAt",
                table: "ChatSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ChatSessions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // Re-add foreign key for Messages
            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChatSessions_SessionId",
                table: "Messages",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the new foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChatSessions_SessionId",
                table: "Messages");

            // Remove new columns
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "LastActivityAt",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ChatSessions");

            // Add back old columns
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ChatSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "ChatSessions",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "ChatSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ChatSessions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Active");

            // Re-add old foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Users_UserId",
                table: "ChatSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChatSessions_SessionId",
                table: "Messages",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
} 