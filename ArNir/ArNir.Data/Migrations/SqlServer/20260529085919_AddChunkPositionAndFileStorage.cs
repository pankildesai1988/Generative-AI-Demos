using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArNir.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddChunkPositionAndFileStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                table: "Documents",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "BboxX1",
                table: "DocumentChunks",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "BboxX2",
                table: "DocumentChunks",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "BboxY1",
                table: "DocumentChunks",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "BboxY2",
                table: "DocumentChunks",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChunkType",
                table: "DocumentChunks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageNumber",
                table: "DocumentChunks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "BboxX1",
                table: "DocumentChunks");

            migrationBuilder.DropColumn(
                name: "BboxX2",
                table: "DocumentChunks");

            migrationBuilder.DropColumn(
                name: "BboxY1",
                table: "DocumentChunks");

            migrationBuilder.DropColumn(
                name: "BboxY2",
                table: "DocumentChunks");

            migrationBuilder.DropColumn(
                name: "ChunkType",
                table: "DocumentChunks");

            migrationBuilder.DropColumn(
                name: "PageNumber",
                table: "DocumentChunks");
        }
    }
}
