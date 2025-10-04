using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArNir.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddTokenCountsToRagComparisonHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContextTokens",
                table: "RagComparisonHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QueryTokens",
                table: "RagComparisonHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalTokens",
                table: "RagComparisonHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContextTokens",
                table: "RagComparisonHistories");

            migrationBuilder.DropColumn(
                name: "QueryTokens",
                table: "RagComparisonHistories");

            migrationBuilder.DropColumn(
                name: "TotalTokens",
                table: "RagComparisonHistories");
        }
    }
}
