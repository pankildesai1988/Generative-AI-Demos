using ArNir.Core.DTOs.Documents;
using ArNir.Core.DTOs.Embeddings;

namespace ArNir.Admin.ViewModel
{
    public class RetrievalComparisonViewModel
    {
        public string Query { get; set; } = string.Empty;
        public int TopK { get; set; }
        public bool ShowMetadata { get; set; } = false; // ✅ New toggle
        public List<ChunkResultDto> SemanticResults { get; set; } = new();
        public List<ChunkResultDto> HybridResults { get; set; } = new();

        // ✅ New properties for timings
        public RetrievalTimingDto SemanticTiming { get; set; } = new();
        public RetrievalTimingDto HybridTiming { get; set; } = new();
    }
}
