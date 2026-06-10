using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArNir.Data.Migrations.SqlServer
{
    /// <summary>
    /// Seeds the remaining runtime-tunable RAG defaults into the <c>PlatformSettings</c> table so
    /// operators can change them from the Admin panel without a redeploy, and corrects the stale
    /// <c>RAG/EmbeddingModel</c> value (was <c>text-embedding-ada-002</c>) to the current default.
    /// <para>
    /// Data-only migration — no schema change. The <c>PlatformSettings</c> table already exists
    /// (Phase 10). These rows are the top config layer; code still falls back to appsettings
    /// (<c>Rag</c> section) and then to <c>ApplicationConstants</c> when a row is absent.
    /// </para>
    /// </summary>
    public partial class AddPlatformSettingsDefaults : Migration
    {
        private static readonly DateTime Seeded = new(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Correct the stale embedding-model value carried over from Phase 10.
            migrationBuilder.UpdateData(
                table: "PlatformSettings",
                keyColumns: new[] { "Module", "Key" },
                keyValues: new object[] { "RAG", "EmbeddingModel" },
                columns: new[] { "Value", "UpdatedAt" },
                values: new object[] { "text-embedding-3-small", Seeded });

            // Insert the previously-hardcoded RAG defaults (Id is identity — omit it).
            migrationBuilder.InsertData(
                table: "PlatformSettings",
                columns: new[] { "Module", "Key", "Value", "Description", "UpdatedAt" },
                values: new object[,]
                {
                    { "RAG", "ScoreThreshold",   "0.45",  "Minimum cosine similarity a chunk must reach to survive the relevance filter", Seeded },
                    { "RAG", "LowTextThreshold", "100",   "Char count below which a parsed document is flagged LowText (image/scanned PDF)", Seeded },
                    { "RAG", "DefaultTopK",      "5",     "Default number of chunks to retrieve when the request omits TopK",            Seeded },
                    { "RAG", "MaxContextTokens", "6000",  "Maximum context tokens before the RAG context block is trimmed",             Seeded },
                    { "RAG", "ConfidenceHigh",   "0.85",  "Top-score cutoff at/above which retrieval confidence is reported as high",    Seeded },
                    { "RAG", "ConfidenceMedium", "0.65",  "Top-score cutoff at/above which retrieval confidence is reported as medium",  Seeded },
                    { "RAG", "EnableVisionOcr",  "false", "Run vision OCR fallback for pages with no extractable text (off by default)", Seeded }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert the embedding-model value.
            migrationBuilder.UpdateData(
                table: "PlatformSettings",
                keyColumns: new[] { "Module", "Key" },
                keyValues: new object[] { "RAG", "EmbeddingModel" },
                columns: new[] { "Value", "UpdatedAt" },
                values: new object[] { "text-embedding-ada-002", Seeded });

            foreach (var key in new[]
                     {
                         "ScoreThreshold", "LowTextThreshold", "DefaultTopK",
                         "MaxContextTokens", "ConfidenceHigh", "ConfidenceMedium", "EnableVisionOcr"
                     })
            {
                migrationBuilder.DeleteData(
                    table: "PlatformSettings",
                    keyColumns: new[] { "Module", "Key" },
                    keyValues: new object[] { "RAG", key });
            }
        }
    }
}
