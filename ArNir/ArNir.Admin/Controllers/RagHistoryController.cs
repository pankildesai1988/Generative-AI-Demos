using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.Services;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArNir.Admin.Controllers
{
    /// <summary>
    /// Admin controller for browsing RAG history and submitting retrieval quality feedback.
    /// </summary>
    [Authorize]
    public class RagHistoryController : Controller
    {
        private readonly IRagHistoryService _service;
        private readonly IDbContextFactory<ArNirDbContext> _dbFactory;

        /// <summary>Initialises the controller with required services.</summary>
        public RagHistoryController(
            IRagHistoryService service,
            IDbContextFactory<ArNirDbContext> dbFactory)
        {
            _service   = service;
            _dbFactory = dbFactory;
        }

        /// <summary>Renders the RAG history index page.</summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>Returns filtered RAG history entries as JSON for DataTables.</summary>
        [HttpGet]
        public async Task<IActionResult> GetHistory(string? slaStatus, DateTime? startDate, DateTime? endDate, string? queryText, string? promptStyle, string? provider, string? model)
        {
            var histories = await _service.GetHistoryAsync(slaStatus, startDate, endDate, queryText, promptStyle, provider, model);
            return Json(new { data = histories });
        }

        /// <summary>Returns full details of a single RAG history entry as JSON.</summary>
        [HttpGet]
        public async Task<IActionResult> GetDetails(int id)
        {
            var details = await _service.GetHistoryDetailsAsync(id);
            if (details == null) return NotFound();
            return Json(details);
        }

        /// <summary>
        /// Upserts a star rating (1–5) for a RAG history entry.
        /// If a <see cref="Feedback"/> row already exists for <paramref name="historyId"/>, its rating
        /// and comments are updated; otherwise a new row is inserted.
        /// </summary>
        /// <param name="historyId">The RAG history entry being rated.</param>
        /// <param name="rating">Star rating between 1 and 5 (inclusive).</param>
        /// <param name="comments">Optional free-text comments.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(int historyId, int rating, string? comments)
        {
            if (rating < 1 || rating > 5)
                return BadRequest("Rating must be between 1 and 5.");

            await using var db = await _dbFactory.CreateDbContextAsync();

            var existing = await db.Feedbacks
                .FirstOrDefaultAsync(f => f.HistoryId == historyId);

            if (existing != null)
            {
                existing.Rating   = rating;
                existing.Comments = comments;
            }
            else
            {
                db.Feedbacks.Add(new Feedback
                {
                    HistoryId  = historyId,
                    Rating     = rating,
                    Comments   = comments,
                    CreatedAt  = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Feedback saved." });
        }
    }
}
