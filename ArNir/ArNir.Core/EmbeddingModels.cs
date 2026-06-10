namespace ArNir.Core
{
    /// <summary>
    /// Central source of truth for the OpenAI embedding model used across the platform.
    /// <para>
    /// Both the ingestion path (document embedding) and the query path (search embedding)
    /// MUST use the same model — comparing vectors produced by different models yields
    /// meaningless cosine similarity. Reference <see cref="Default"/> everywhere instead of
    /// hard-coding a model string so the two paths can never drift apart.
    /// </para>
    /// <para>
    /// <b>Note:</b> <c>text-embedding-3-small</c> and the legacy <c>text-embedding-ada-002</c>
    /// are both 1536-dimensional, so the <c>vector(1536)</c> column needs no schema change when
    /// switching. Existing rows embedded with the old model MUST be re-embedded after a change.
    /// </para>
    /// </summary>
    public static class EmbeddingModels
    {
        /// <summary>
        /// Default embedding model for all ingest and query operations.
        /// 1536-dimensional; drop-in compatible with the existing pgvector column.
        /// </summary>
        public const string Default = "text-embedding-3-small";

        /// <summary>
        /// Legacy OpenAI embedding model (2022). Retained only for identifying/clearing
        /// historical embeddings during a re-embed migration. Do not use for new work.
        /// </summary>
        public const string LegacyAda002 = "text-embedding-ada-002";
    }
}
