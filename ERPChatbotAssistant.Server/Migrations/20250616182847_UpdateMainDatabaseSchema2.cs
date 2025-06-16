using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERPChatbotAssistant.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMainDatabaseSchema2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old tables if they exist
            migrationBuilder.Sql("IF OBJECT_ID('Messages', 'U') IS NOT NULL DROP TABLE Messages;");
            migrationBuilder.Sql("IF OBJECT_ID('ChatSession', 'U') IS NOT NULL DROP TABLE ChatSession;");
            migrationBuilder.Sql("IF OBJECT_ID('ConversationHistories', 'U') IS NOT NULL DROP TABLE ConversationHistories;");
            migrationBuilder.Sql("IF OBJECT_ID('TrainingData', 'U') IS NOT NULL DROP TABLE TrainingData;");

            // Create ConversationHistories table
            migrationBuilder.CreateTable(
                name: "ConversationHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BotResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Context = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationHistories", x => x.Id);
                });

            // Create TrainingData table
            migrationBuilder.CreateTable(
                name: "TrainingData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingData", x => x.Id);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_ConversationHistories_SessionId",
                table: "ConversationHistories",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingData_Keywords",
                table: "TrainingData",
                column: "Keywords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationHistories");

            migrationBuilder.DropTable(
                name: "TrainingData");
        }
    }
}
