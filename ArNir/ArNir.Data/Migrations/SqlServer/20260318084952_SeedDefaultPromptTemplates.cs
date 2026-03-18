using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArNir.Data.Migrations.SqlServer
{
    /// <summary>
    /// Seeds 5 default prompt templates (one per built-in style) so that the
    /// LayeredPromptResolver has DB-layer entries to serve immediately after deployment.
    /// Templates use {{QUERY}} and {{CONTEXT}} placeholders which the resolver substitutes
    /// at build time.
    /// </summary>
    public partial class SeedDefaultPromptTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = new DateTime(2026, 3, 18, 0, 0, 0, DateTimeKind.Utc);

            migrationBuilder.InsertData(
                table: "PromptTemplates",
                columns: new[] { "Id", "Style", "Name", "TemplateText", "Version", "IsActive", "Source", "CreatedAt" },
                values: new object[,]
                {
                    {
                        Guid.NewGuid(), "zero-shot", "Zero-Shot Default",
                        "You are a helpful AI assistant.\n\nQuestion: {{QUERY}}\n\nAnswer:",
                        1, true, "Database", now
                    },
                    {
                        Guid.NewGuid(), "few-shot", "Few-Shot Default",
                        "Below are examples that demonstrate the expected answer style.\n\nExamples:\n{{CONTEXT}}\n\nQuestion: {{QUERY}}\n\nAnswer:",
                        1, true, "Database", now
                    },
                    {
                        Guid.NewGuid(), "role", "Role-Based Default",
                        "You are an expert consultant specialising in the domain relevant to the question below.\n\nQuestion: {{QUERY}}\n\nAnswer:",
                        1, true, "Database", now
                    },
                    {
                        Guid.NewGuid(), "rag", "RAG Default",
                        "You are a helpful assistant. Use ONLY the context below to answer the question. If the context does not contain enough information, say so.\n\nContext:\n{{CONTEXT}}\n\nQuestion: {{QUERY}}\n\nAnswer:",
                        1, true, "Database", now
                    },
                    {
                        Guid.NewGuid(), "hybrid", "Hybrid Default",
                        "You are an expert assistant.\n\nRelevant context:\n{{CONTEXT}}\n\nQuestion: {{QUERY}}\n\nProvide a thorough answer drawing on both the context and your general knowledge:\n",
                        1, true, "Database", now
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM PromptTemplates WHERE Source = 'Database' AND Version = 1");
        }
    }
}
