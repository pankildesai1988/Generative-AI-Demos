using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArNir.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class InitSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RagComparisonHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaselineAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RagAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RetrievedChunksJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RetrievalLatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    LlmLatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    TotalLatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    IsWithinSla = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RagComparisonHistories", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RagComparisonHistories");
        }
    }
}
