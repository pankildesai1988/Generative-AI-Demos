using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace ArNir.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddChatEmbeddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatEmbeddings",
                columns: table => new
                {
                    EmbeddingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatMemoryId = table.Column<int>(type: "integer", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    Vector = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEmbeddings", x => x.EmbeddingId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatEmbeddings");
        }
    }
}
