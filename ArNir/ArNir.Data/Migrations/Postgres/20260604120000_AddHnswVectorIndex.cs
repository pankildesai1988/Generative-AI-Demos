using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArNir.Data.Migrations.Postgres
{
    /// <summary>
    /// Adds an HNSW (Hierarchical Navigable Small World) approximate-nearest-neighbour index
    /// on <c>Embeddings."Vector"</c> using the <c>vector_cosine_ops</c> operator class.
    /// <para>
    /// The operator class MUST be <c>vector_cosine_ops</c> to match the cosine distance operator
    /// (<c>&lt;=&gt;</c>) used by <c>RetrievalService</c> and <c>PgvectorDocumentVectorStore</c>;
    /// a mismatched operator class causes the query planner to ignore the index and fall back to a
    /// sequential scan. Without this index, similarity search was an exact brute-force scan (O(n)).
    /// </para>
    /// <para>
    /// Tuning: <c>m = 16</c> (graph connectivity) and <c>ef_construction = 64</c> (build-time
    /// candidate list) are pgvector's balanced defaults for recall vs build cost. Requires
    /// pgvector &gt;= 0.5.0.
    /// </para>
    /// </summary>
    public partial class AddHnswVectorIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ix_embeddings_vector_hnsw
                ON ""Embeddings"" USING hnsw (""Vector"" vector_cosine_ops)
                WITH (m = 16, ef_construction = 64);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_embeddings_vector_hnsw;");
        }
    }
}
