using _2_OpenAIChatDemo.Data;
using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.LLMProviders;
using _2_OpenAIChatDemo.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace _2_OpenAIChatDemo.Services
{
    public class ComparisonService : IComparisonService
    {
        private readonly ChatDbContext _dbContext;
        private readonly IEnumerable<ILlmProvider> _providers;
        private readonly ILogger<ComparisonService> _logger;

        public ComparisonService(ChatDbContext dbContext, IEnumerable<ILlmProvider> providers, ILogger<ComparisonService> logger)
        {
            _dbContext = dbContext;
            _providers = providers;
            _logger = logger;
        }

        public async Task<ComparisonResultDto> RunComparisonAsync(ComparisonRequestDto request)
        {
            var results = new List<ComparisonResult>();
            var dtoResults = new List<ModelOutputDto>();

            // ✅ Deduplicate model requests
            foreach (var modelName in request.ModelNames.Distinct())
            {
                var parts = modelName.Split(':');
                if (parts.Length != 2) continue;

                var providerName = parts[0];
                var model = parts[1];

                var provider = _providers.FirstOrDefault(p =>
                    p.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));

                if (provider == null)
                {
                    var errorResult = new ComparisonResult
                    {
                        Provider = providerName,
                        ModelName = model,
                        ResponseText = null,
                        RawResponse = null,
                        ErrorCode = "ComparisonError",
                        ErrorMessage = $"No provider found for {providerName}:{model}",
                        LatencyMs = null,
                        CreatedAt = DateTime.UtcNow
                    };

                    results.Add(errorResult);

                    dtoResults.Add(new ModelOutputDto
                    {
                        Provider = providerName,
                        ModelName = model,
                        IsError = true,
                        ErrorCode = "ComparisonError",
                        ErrorMessage = errorResult.ErrorMessage
                    });

                    continue;
                }

                try
                {
                    var sw = Stopwatch.StartNew();
                    var responseText = await provider.GetResponseAsync(model, request.InputText);
                    sw.Stop();

                    var successResult = new ComparisonResult
                    {
                        Provider = providerName,
                        ModelName = model,
                        ResponseText = responseText,
                        RawResponse = responseText,
                        ErrorCode = null,
                        ErrorMessage = null,
                        LatencyMs = sw.Elapsed.TotalMilliseconds,
                        CreatedAt = DateTime.UtcNow
                    };

                    results.Add(successResult);

                    dtoResults.Add(new ModelOutputDto
                    {
                        Provider = providerName,
                        ModelName = model,
                        ResponseText = responseText,
                        RawResponse = responseText,
                        LatencyMs = successResult.LatencyMs,
                        IsError = false
                    });
                }
                catch (Exception ex)
                {
                    var errorResult = new ComparisonResult
                    {
                        Provider = providerName,
                        ModelName = model,
                        ResponseText = null,
                        RawResponse = null,
                        ErrorCode = "ProviderError",
                        ErrorMessage = ex.Message,
                        LatencyMs = null,
                        CreatedAt = DateTime.UtcNow
                    };

                    results.Add(errorResult);

                    dtoResults.Add(new ModelOutputDto
                    {
                        Provider = providerName,
                        ModelName = model,
                        IsError = true,
                        ErrorCode = "ProviderError",
                        ErrorMessage = ex.Message
                    });
                }
            }

            // ✅ Save session comparison with results
            var sessionComparison = new SessionComparison
            {
                InputText = request.InputText,
                CreatedAt = DateTime.UtcNow,
                Results = results
            };

            _dbContext.SessionComparisons.Add(sessionComparison);
            await _dbContext.SaveChangesAsync();

            return new ComparisonResultDto
            {
                ComparisonId = sessionComparison.Id,
                InputText = request.InputText,
                Results = dtoResults
            };
        }


        public async Task<ComparisonResultDto> GetComparisonAsync(int id)
        {
            var comparison = await _dbContext.SessionComparisons
                .Include(c => c.Results)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comparison == null) return null;

            return new ComparisonResultDto
            {
                ComparisonId = comparison.Id,
                InputText = comparison.InputText,
                Results = comparison.Results
                    .Where(r => r != null)
                    .Select(r => new ModelOutputDto
                    {
                        Provider = r.Provider,
                        ModelName = r.ModelName,
                        ResponseText = r.ResponseText,
                        RawResponse = r.RawResponse,
                        LatencyMs = r.LatencyMs,
                        IsError = !string.IsNullOrEmpty(r.ErrorCode),
                        ErrorCode = r.ErrorCode,
                        ErrorMessage = r.ErrorMessage
                    }).ToList()
            };
        }

        public async Task<List<ComparisonHistoryDto>> GetHistoryAsync(int take = 50)
        {
            var history = await _dbContext.SessionComparisons
                .Include(c => c.Results)
                .OrderByDescending(c => c.CreatedAt)
                .Take(take)
                .ToListAsync();

            return history.Select(c => new ComparisonHistoryDto
            {
                Id = c.Id,
                InputText = c.InputText,
                CreatedAt = c.CreatedAt,
                Results = c.Results.Select(r => new ComparisonHistoryResultDto
                {
                    Provider = r.Provider,
                    ModelName = r.ModelName,
                    IsError = !string.IsNullOrEmpty(r.ErrorCode),
                    ErrorCode = r.ErrorCode,
                    ErrorMessage = r.ErrorMessage
                }).ToList()
            }).ToList();
        }
        public async Task<ComparisonHistoryDto?> GetHistoryByIdAsync(int id)
        {
            var comparison = await _dbContext.SessionComparisons
                .Include(c => c.Results)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comparison == null) return null;

            return new ComparisonHistoryDto
            {
                Id = comparison.Id,
                InputText = comparison.InputText,
                CreatedAt = comparison.CreatedAt,
                Results = comparison.Results.Select(r => new ComparisonHistoryResultDto
                {
                    Provider = r.Provider,
                    ModelName = r.ModelName,
                    IsError = !string.IsNullOrEmpty(r.ErrorCode),
                    ErrorCode = r.ErrorCode,
                    ErrorMessage = r.ErrorMessage,
                    ResponseText = r.ResponseText   // ✅ add this
                }).ToList()
            };
        }

    }
}
