using ArNir.Core.DTOs.Analytics;
using ArNir.Services.AI.Interfaces;
using ArNir.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ArNir.Services.AI
{
    /// <summary>
    /// Handles intelligent backend actions such as model comparison,
    /// SLA analytics, and trend exploration triggered from chat context.
    /// </summary>
    public class ActionEngineService : IActionEngineService
    {
        private readonly IRagService _ragService;
        private readonly ILogger<ActionEngineService> _logger;

        public ActionEngineService(IRagService ragService, ILogger<ActionEngineService> logger)
        {
            _ragService = ragService;
            _logger = logger;
        }

        /// <summary>
        /// Executes contextual backend analytics actions mapped from AI-detected intents.
        /// </summary>
        /// <param name="intent">Action intent name (e.g., 'compare_models', 'view_trends').</param>
        /// <param name="parameter">Optional parameter (e.g., provider name).</param>
        public async Task<object?> ExecuteActionAsync(string intent, string? parameter = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(intent))
                    return new { Message = "⚠️ No action intent specified." };

                _logger.LogInformation("Executing contextual action: {Intent} (param: {Param})", intent, parameter);

                switch (intent.ToLowerInvariant())
                {
                    case "compare_models":
                    case "compare":
                        // ✅ Model comparison analytics
                        return await _ragService.GetProviderAnalyticsAsync(null, null);

                    case "sla_summary":
                    case "sla":
                        // ✅ SLA metrics overview
                        return await _ragService.GetProviderAnalyticsAsync(
                            DateTime.UtcNow.AddDays(-30),
                            DateTime.UtcNow);

                    case "view_trends":
                    case "trends":
                        // ✅ Historical RAG results (last 30 days)
                        return await _ragService.GetRagHistoryAsync(
                            DateTime.UtcNow.AddDays(-30),
                            DateTime.UtcNow);

                    default:
                        _logger.LogWarning("No handler found for action: {Intent}", intent);
                        return new { Message = $"⚠️ No backend handler found for action: '{intent}'" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error executing contextual action: {Intent}", intent);
                return new { Message = $"❌ Failed to execute '{intent}': {ex.Message}" };
            }
        }
    }
}
