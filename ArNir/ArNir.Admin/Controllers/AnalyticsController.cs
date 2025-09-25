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
            var data = await _ragService.GetAverageLatenciesAsync(startDate, endDate, slaStatus, promptStyle);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetSlaCompliance(DateTime? startDate, DateTime? endDate, string? slaStatus, string? promptStyle)
        {
            var data = await _ragService.GetSlaComplianceAsync(startDate, endDate, slaStatus, promptStyle);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetPromptStyleUsage(DateTime? startDate, DateTime? endDate, string? slaStatus, string? promptStyle)
        {
            var data = await _ragService.GetPromptStyleUsageAsync(startDate, endDate, slaStatus, promptStyle);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetTrends(DateTime? startDate, DateTime? endDate, string? slaStatus, string? promptStyle)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var data = await _ragService.GetTrendsAsync(start, end, slaStatus, promptStyle);
            return Json(data);
        }
    }
}
