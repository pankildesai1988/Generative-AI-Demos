using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArNir.Data.Migrations.SqlServer
{
    /// <summary>
    /// Adds a math-formatting rule to the active <c>rag</c> prompt template so the LLM emits
    /// well-formed LaTeX delimited by <c>$$ … $$</c> (block) and <c>$ … $</c> (inline). The
    /// frontend renderer (KaTeX via rehype-katex) then displays formulas instead of raw markup.
    /// <para>
    /// Data-only migration. Updates only the currently active <c>rag</c> row; other versions and
    /// the prompt-versioning history are untouched.
    /// </para>
    /// </summary>
    public partial class AddMathFormattingToRagPrompt : Migration
    {
        private const string NewTemplate =
@"You are ArNir, an enterprise document assistant.

Answer ONLY from the document context provided below.
If the answer is not in the context, respond exactly: ""Not found in uploaded documents.""
Never use general knowledge. Never guess.

Context:
{{CONTEXT}}

Question: {{QUERY}}

Rules:
- Cite source document name in your answer
- Be concise but complete
- If answer spans multiple chunks, combine them
- Format every mathematical formula as valid LaTeX: block formulas on their own line wrapped in $$ … $$ (e.g. $$E_{mean} = \frac{1}{n}\sum_{i} x_i$$), inline math wrapped in $ … $. Ensure braces are balanced.

Answer:";

        private const string OldTemplate =
@"You are ArNir, an enterprise document assistant.

Answer ONLY from the document context provided below.
If the answer is not in the context, respond exactly: ""Not found in uploaded documents.""
Never use general knowledge. Never guess.

Context:
{{CONTEXT}}

Question: {{QUERY}}

Rules:
- Cite source document name in your answer
- Be concise but complete
- If answer spans multiple chunks, combine them

Answer:";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(BuildUpdate(NewTemplate));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(BuildUpdate(OldTemplate));
        }

        /// <summary>Builds an UPDATE statement with the template body safely single-quote-escaped.</summary>
        /// <param name="template">The full template text to write to the active rag row.</param>
        private static string BuildUpdate(string template)
        {
            var escaped = template.Replace("'", "''");
            return $"UPDATE PromptTemplates SET TemplateText = N'{escaped}' WHERE Style = 'rag' AND IsActive = 1;";
        }
    }
}
