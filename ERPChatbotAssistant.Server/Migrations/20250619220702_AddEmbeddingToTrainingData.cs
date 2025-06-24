using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPChatbotAssistant.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbeddingToTrainingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Embedding",
                table: "TrainingData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "TrainingData");
        }
    }
}
