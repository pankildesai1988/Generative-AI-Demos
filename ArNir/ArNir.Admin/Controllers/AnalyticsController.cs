using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArNir.Admin.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly IRagService _ragService;

        public AnalyticsController(IRagService ragService)
        {
            _ragService = ragService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAverageLatencies(DateTime? startDate, DateTime? endDate, string? slaStatus, string? promptStyle)
        {
            if (string.IsNullOrWhiteSpace(slaStatus)) slaStatus = null;
            var response = await _ragService.GetAverageLatenciesAsync(startDate, endDate, slaStatus, promptStyle);
            return Json(response); // ✅ return AnalyticsResponse directly
        }

        [HttpGet]
        public async Task<IActionResult> GetSlaCompliance(DateTime? startDate, DateTime? endDate, string? slaStatus, string? promptStyle)
        {
            if (string.IsNullOrWhiteSpace(slaStatus)) slaStatus = null;
            var response = await _ragService.GetSlaComplianceAsync(startDate, endDate, slaStatus, promptStyle);
            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetPromptStyleUsage(DateTime? startDate, DateTime? endDate, string? slaStatus, string? promptStyle)
        {
            if (string.IsNullOrWhiteSpace(slaStatus)) slaStatus = null;
            var response = await _ragService.GetPromptStyleUsageAsync(startDate, endDate, slaStatus, promptStyle);
            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetTrends(DateTime? startDate, DateTime? endDate, string? slaStatus, string? promptStyle)
        {
            if (string.IsNullOrWhiteSpace(slaStatus)) slaStatus = null;

            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var response = await _ragService.GetTrendsAsync(start, end, slaStatus, promptStyle);
            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetProviderAnalytics(DateTime? startDate, DateTime? endDate, string? promptStyle)
        {
            var response = await _ragService.GetProviderAnalyticsAsync(startDate, endDate, promptStyle);
            return Json(response);
        }
    }
}
