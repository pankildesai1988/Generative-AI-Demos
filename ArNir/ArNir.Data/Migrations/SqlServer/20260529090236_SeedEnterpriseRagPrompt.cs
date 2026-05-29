using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArNir.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class SeedEnterpriseRagPrompt : Migration
    {
        private const string TemplateText =
            "You are ArNir, an enterprise document assistant.\r\n\r\n" +
            "Answer ONLY from the document chunks provided below. Do NOT use general knowledge. Do NOT guess.\r\n\r\n" +
            "Document chunks:\r\n{{CONTEXT}}\r\n\r\n" +
            "RULES:\r\n" +
            "- If the answer is not in the chunks, say: \"Not found in uploaded documents.\"\r\n" +
            "- Never hallucinate dates, numbers, names, or figures.\r\n" +
            "- Always cite: source filename and page number when available.\r\n" +
            "- If the answer involves a chart or table, say: \"See [chart/table] on page X of [filename].\"\r\n\r\n" +
            "TONE: Professional. Concise. Consultant-level.\r\n\r\n" +
            "Question: {{QUERY}}";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Deactivate all existing "rag" style templates
            migrationBuilder.Sql(
                "UPDATE PromptTemplates SET IsActive = 0 WHERE Style = 'rag'");

            // Insert new enterprise rag v2 template
            migrationBuilder.Sql($@"
                INSERT INTO PromptTemplates (Id, Style, Name, TemplateText, Version, IsActive, Source, CreatedAt)
                VALUES (
                    NEWID(),
                    'rag',
                    'Enterprise RAG v2',
                    N'{TemplateText.Replace("'", "''")}',
                    2,
                    1,
                    'Database',
                    GETUTCDATE()
                )");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM PromptTemplates WHERE Style = 'rag' AND Name = 'Enterprise RAG v2'");

            migrationBuilder.Sql(
                "UPDATE PromptTemplates SET IsActive = 1 WHERE Style = 'rag' AND Version = 1");
        }
    }
}
