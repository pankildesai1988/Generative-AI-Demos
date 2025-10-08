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

        //[HttpPost]
        //public async Task<IActionResult> Run(string query, int topK = 5, bool useHybrid = true)
        //{
        //    if (string.IsNullOrWhiteSpace(query))
        //        return BadRequest("Query is required.");

        //    var result = await _ragService.RunRagAsync(query, topK, useHybrid);
        //    return Json(result);
        //}

        [HttpPost]
        public async Task<IActionResult> Run(string query, string promptStyle = "rag", string provider = "OpenAI", string model = "gpt-4o-mini")
        {
            var result = await _ragService.RunRagAsync(query, 3, true, promptStyle, true, provider, model);
            return Json(new
            {
                baselineAnswer = result.BaselineAnswer,
                ragAnswer = result.RagAnswer,
                provider = result.Provider,
                model = result.Model,
                retrievedChunks = result.RetrievedChunks,
                retrievalLatencyMs = result.RetrievalLatencyMs,
                llmLatencyMs = result.LlmLatencyMs,
                totalLatencyMs = result.TotalLatencyMs,
                isWithinSla = result.IsWithinSla
            });
        }

    }
}
