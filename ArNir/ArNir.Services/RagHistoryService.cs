using ArNir.Core.DTOs.RAG;
using ArNir.Data.Repositories;
using ArNir.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Services
{
    public class RagHistoryService : IRagHistoryService
    {
        private readonly IRagHistoryRepository _repository;

        public RagHistoryService(IRagHistoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<RagHistoryListDto>> GetHistoryListAsync(
            string? slaStatus, DateTime? startDate, DateTime? endDate, string? queryText, string? promptStyle)
        {
            var histories = await _repository.FilterAsync(slaStatus, startDate, endDate, queryText, promptStyle);

            return histories.Select(h => new RagHistoryListDto
            {
                Id = h.Id,
                Query = h.UserQuery,
                CreatedAt = h.CreatedAt,
                IsWithinSla = h.IsWithinSla,
                RetrievalLatencyMs = h.RetrievalLatencyMs,
                LlmLatencyMs = h.LlmLatencyMs,
                TotalLatencyMs = h.TotalLatencyMs,
                PromptStyle = h.PromptStyle   // ✅ add this
            }).ToList();
        }

        public async Task<RagHistoryDetailDto?> GetHistoryDetailsAsync(int id)
        {
            var history = await _repository.GetByIdAsync(id);
            if (history == null) return null;

            return new RagHistoryDetailDto
            {
                Id = history.Id,
                Query = history.UserQuery,
                BaselineAnswer = history.BaselineAnswer,
                RagAnswer = history.RagAnswer,
                RetrievedChunksJson = history.RetrievedChunksJson,
                RetrievalLatencyMs = history.RetrievalLatencyMs,
                LlmLatencyMs = history.LlmLatencyMs,
                TotalLatencyMs = history.TotalLatencyMs,
                IsWithinSla = history.IsWithinSla,
                CreatedAt = history.CreatedAt,
                PromptStyle = history.PromptStyle   // ✅ add this
            };
        }
    }
}
