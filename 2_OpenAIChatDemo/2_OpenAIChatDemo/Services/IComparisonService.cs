using _2_OpenAIChatDemo.DTOs;

namespace _2_OpenAIChatDemo.Services
{
    public interface IComparisonService
    {
        Task<ComparisonResultDto> RunComparisonAsync(ComparisonRequestDto request);
        Task<ComparisonResultDto> GetComparisonAsync(int id);
        Task<List<ComparisonHistoryDto>> GetHistoryAsync(int take = 50);
        Task<ComparisonHistoryDto?> GetHistoryByIdAsync(int id);
    }

}
