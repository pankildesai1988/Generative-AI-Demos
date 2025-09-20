using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ArNir.Admin.Controllers
{
    public class RagComparisonController : Controller
    {
        private readonly IRagService _ragService;

        public RagComparisonController(IRagService ragService)
        {
            _ragService = ragService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Run(string query, int topK = 5, bool useHybrid = true)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query is required.");

            var result = await _ragService.RunRagAsync(query, topK, useHybrid);
            return Json(result);
        }
    }
}
