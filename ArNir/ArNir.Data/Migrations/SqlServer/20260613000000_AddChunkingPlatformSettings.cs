using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArNir.Data.Migrations.SqlServer
{
    /// <summary>
    /// Seeds the chunking knobs into the <c>PlatformSettings</c> table (module <c>RAG</c>:
    /// <c>ChunkSize=600</c>, <c>ChunkOverlap=100</c>, <c>ChunkingStrategy=sentence</c>) and bumps
    /// <c>RAG/DefaultTopK</c> from 5 to 10 — multi-row comparison questions silently fail when
    /// top-K is smaller than the expected answer set.
    /// <para>
    /// Data-only migration — no schema change. Inserts are guarded with <c>IF NOT EXISTS</c>
    /// because operators may have already created these rows from the Admin Platform Settings
    /// panel even though they were never seeded; <c>DefaultTopK</c> is updated only while it
    /// still holds the original seeded value <c>5</c>, preserving operator overrides.
    /// </para>
    /// </summary>
    public partial class AddChunkingPlatformSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM PlatformSettings WHERE Module = 'RAG' AND [Key] = 'ChunkSize')
    INSERT INTO PlatformSettings (Module, [Key], Value, Description, UpdatedAt)
    VALUES ('RAG', 'ChunkSize', '600', 'Target character length of each document chunk produced during ingestion', '2026-06-13T00:00:00');

IF NOT EXISTS (SELECT 1 FROM PlatformSettings WHERE Module = 'RAG' AND [Key] = 'ChunkOverlap')
    INSERT INTO PlatformSettings (Module, [Key], Value, Description, UpdatedAt)
    VALUES ('RAG', 'ChunkOverlap', '100', 'Characters of context carried over between consecutive chunks (sliding window)', '2026-06-13T00:00:00');

IF NOT EXISTS (SELECT 1 FROM PlatformSettings WHERE Module = 'RAG' AND [Key] = 'ChunkingStrategy')
    INSERT INTO PlatformSettings (Module, [Key], Value, Description, UpdatedAt)
    VALUES ('RAG', 'ChunkingStrategy', 'sentence', 'Document chunking strategy: sentence (boundary-aware packing) or sliding (fixed window)', '2026-06-13T00:00:00');

-- Bump only while the row still holds the original seeded default (preserves operator overrides).
UPDATE PlatformSettings
SET Value = '10', UpdatedAt = '2026-06-13T00:00:00'
WHERE Module = 'RAG' AND [Key] = 'DefaultTopK' AND Value = '5';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM PlatformSettings WHERE Module = 'RAG' AND [Key] = 'ChunkSize'        AND Value = '600';
DELETE FROM PlatformSettings WHERE Module = 'RAG' AND [Key] = 'ChunkOverlap'     AND Value = '100';
DELETE FROM PlatformSettings WHERE Module = 'RAG' AND [Key] = 'ChunkingStrategy' AND Value = 'sentence';

UPDATE PlatformSettings
SET Value = '5', UpdatedAt = '2026-06-13T00:00:00'
WHERE Module = 'RAG' AND [Key] = 'DefaultTopK' AND Value = '10';
");
        }
    }
}
