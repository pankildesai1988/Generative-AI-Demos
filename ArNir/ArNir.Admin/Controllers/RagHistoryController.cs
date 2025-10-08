using ArNir.Services;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Admin.Controllers
{
    public class RagHistoryController : Controller
    {
        private readonly IRagHistoryService _service;

        public RagHistoryController(IRagHistoryService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(string? slaStatus, DateTime? startDate, DateTime? endDate, string? queryText, string? promptStyle)
        {
            var history = await _service.GetHistoryListAsync(slaStatus, startDate, endDate, queryText, promptStyle);
            return Json(new { data = history });
        }


        [HttpGet]
        public async Task<IActionResult> GetDetails(int id)
        {
            var details = await _service.GetHistoryDetailsAsync(id);
            if (details == null) return NotFound();
            return Json(details);
        }
    }
}
