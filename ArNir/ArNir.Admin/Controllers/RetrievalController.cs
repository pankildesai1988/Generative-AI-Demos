using ArNir.Services.Interfaces;
using ArNir.Core.DTOs.Documents;
using ArNir.Admin.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ArNir.Admin.Controllers
{
    public class RetrievalController : Controller
    {
        private readonly IRetrievalService _retrievalService;

        public RetrievalController(IRetrievalService retrievalService)
        {
            _retrievalService = retrievalService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string query, int topK = 5)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ModelState.AddModelError("", "Query is required.");
                return View();
            }

            // Run both Semantic & Hybrid
            var semantic = await _retrievalService.SearchAsync(query, topK, false);
            var hybrid = await _retrievalService.SearchAsync(query, topK, true);

            var model = new RetrievalComparisonViewModel
            {
                Query = query,
                TopK = topK,
                SemanticResults = semantic,
                HybridResults = hybrid
            };

            return View(model);
        }

    }
}
