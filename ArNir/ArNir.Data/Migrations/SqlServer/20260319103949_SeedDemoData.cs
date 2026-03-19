using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArNir.Data.Migrations.SqlServer
{
    /// <summary>
    /// Seeds realistic demo data for consulting presentations:
    /// - 3 RagComparisonHistories (varied providers, latencies, prompt styles)
    /// - 3 EvaluationResults linked to the histories
    /// - 5 MetricEvents (observability dashboard)
    /// - 2 Feedbacks (star ratings)
    /// Uses high IDs (9001+) to avoid conflicts with production data.
    /// </summary>
    public partial class SeedDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── RAG Comparison Histories ──
            migrationBuilder.Sql(@"
SET IDENTITY_INSERT [RagComparisonHistories] ON;

INSERT INTO [RagComparisonHistories]
    ([Id],[UserQuery],[BaselineAnswer],[RagAnswer],[RetrievedChunksJson],
     [RetrievalLatencyMs],[LlmLatencyMs],[TotalLatencyMs],[IsWithinSla],
     [PromptStyle],[Provider],[Model],[QueryTokens],[ContextTokens],[TotalTokens],[CreatedAt])
VALUES
    (9001, 'What is cloud computing?',
     'Cloud computing is the delivery of computing services over the internet.',
     'Cloud computing refers to the on-demand delivery of IT resources via the internet with pay-as-you-go pricing. According to the uploaded documentation, it includes IaaS, PaaS, and SaaS models.',
     '[]', 120, 1800, 1920, 1, 'rag', 'OpenAI', 'gpt-4o-mini', 8, 450, 458,
     '2026-03-17 10:00:00'),

    (9002, 'Explain neural network backpropagation',
     'Backpropagation is a training algorithm for neural networks.',
     'Backpropagation calculates the gradient of the loss function with respect to each weight by applying the chain rule. The retrieved context from the ML fundamentals document confirms this is used with gradient descent optimizers like Adam and SGD.',
     '[]', 95, 2200, 2295, 1, 'hybrid', 'OpenAI', 'gpt-4o-mini', 12, 620, 632,
     '2026-03-18 14:30:00'),

    (9003, 'What are the benefits of containerization?',
     'Containers package applications with their dependencies for consistent deployment.',
     'Containerization provides portability, resource efficiency, and rapid scaling. Docker containers share the host OS kernel, making them lighter than VMs. The ingested DevOps guide highlights that Kubernetes orchestrates containers at scale.',
     '[]', 110, 3100, 3210, 1, 'rag', 'Gemini', 'gemini-pro', 10, 380, 390,
     '2026-03-19 09:00:00');

SET IDENTITY_INSERT [RagComparisonHistories] OFF;
");

            // ── Evaluation Results ──
            migrationBuilder.Sql(@"
SET IDENTITY_INSERT [EvaluationResults] ON;

INSERT INTO [EvaluationResults]
    ([Id],[Question],[Answer],[Context],[RelevanceScore],[FaithfulnessScore],
     [Reasoning],[EvaluatedAt],[RelatedHistoryId])
VALUES
    (9001, 'What is cloud computing?',
     'Cloud computing refers to the on-demand delivery of IT resources via the internet with pay-as-you-go pricing.',
     'Cloud computing context from uploaded documents.',
     0.92, 0.88,
     'The answer directly addresses the question about cloud computing and accurately reflects the context provided. Minor point: IaaS/PaaS/SaaS are mentioned but not fully explained.',
     '2026-03-17 10:01:00', 9001),

    (9002, 'Explain neural network backpropagation',
     'Backpropagation calculates the gradient of the loss function with respect to each weight by applying the chain rule.',
     'ML fundamentals context about training algorithms.',
     0.95, 0.91,
     'Excellent relevance — the answer precisely explains backpropagation. Faithfulness is high as the chain rule and optimizer references match the provided context.',
     '2026-03-18 14:31:00', 9002),

    (9003, 'What are the benefits of containerization?',
     'Containerization provides portability, resource efficiency, and rapid scaling.',
     'DevOps guide context about Docker and Kubernetes.',
     0.85, 0.78,
     'Good relevance to the question. Faithfulness slightly lower because the Kubernetes mention extends beyond what was strictly in the retrieved chunks.',
     '2026-03-19 09:01:00', 9003);

SET IDENTITY_INSERT [EvaluationResults] OFF;
");

            // ── Metric Events (Observability) ──
            migrationBuilder.Sql(@"
INSERT INTO [MetricEvents]
    ([EventType],[Provider],[Model],[LatencyMs],[TokensUsed],[IsWithinSla],[OccurredAt])
VALUES
    ('RAG_Query', 'OpenAI', 'gpt-4o-mini', 1920, 458, 1, '2026-03-17 10:00:00'),
    ('RAG_Query', 'OpenAI', 'gpt-4o-mini', 2295, 632, 1, '2026-03-18 14:30:00'),
    ('RAG_Query', 'Gemini', 'gemini-pro',  3210, 390, 1, '2026-03-19 09:00:00'),
    ('Embedding', 'OpenAI', 'text-embedding-3-small', 450, 120, 1, '2026-03-17 09:00:00'),
    ('RAG_Query', 'OpenAI', 'gpt-4o-mini', 5200, 890, 0, '2026-03-18 16:00:00');
");

            // ── Feedbacks ──
            migrationBuilder.Sql(@"
INSERT INTO [Feedbacks]
    ([HistoryId],[Rating],[Comments],[CreatedAt])
VALUES
    (9001, 5, 'Excellent answer with good context usage.', '2026-03-17 10:05:00'),
    (9002, 4, 'Very detailed, could be more concise.', '2026-03-18 14:35:00');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [Feedbacks] WHERE [HistoryId] IN (9001, 9002)");
            migrationBuilder.Sql("DELETE FROM [EvaluationResults] WHERE [Id] IN (9001, 9002, 9003)");
            migrationBuilder.Sql("DELETE FROM [MetricEvents] WHERE [OccurredAt] >= '2026-03-17' AND [OccurredAt] <= '2026-03-19 23:59:59'");
            migrationBuilder.Sql("DELETE FROM [RagComparisonHistories] WHERE [Id] IN (9001, 9002, 9003)");
        }
    }
}
