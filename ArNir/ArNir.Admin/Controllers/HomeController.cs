using ArNir.Admin.Models;
using ArNir.Data;
using ArNir.RAG.InProcess;
using ArNir.RAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ArNir.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDbContextFactory<ArNirDbContext> _sqlFactory;
        private readonly IDbContextFactory<VectorDbContext> _pgFactory;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;

        public HomeController(
            ILogger<HomeController> logger,
            IDbContextFactory<ArNirDbContext> sqlFactory,
            IDbContextFactory<VectorDbContext> pgFactory,
            IConfiguration config,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _sqlFactory = sqlFactory;
            _pgFactory = pgFactory;
            _config = config;
            _serviceProvider = serviceProvider;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new PlatformHealthViewModel();

            // SQL Server queries
            await using var sqlCtx = await _sqlFactory.CreateDbContextAsync();
            vm.TotalDocuments = await sqlCtx.Documents.CountAsync();
            vm.TotalChunks = await sqlCtx.DocumentChunks.CountAsync();

            // PostgreSQL queries
            try
            {
                await using var pgCtx = await _pgFactory.CreateDbContextAsync();
                vm.TotalEmbeddings = await pgCtx.Embeddings.LongCountAsync();
                vm.PostgresConnected = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not connect to PostgreSQL for health check.");
                vm.PostgresConnected = false;
            }

            // OpenAI key configured?
            var apiKey = _config["OpenAI:ApiKey"];
            vm.OpenAiKeyConfigured = !string.IsNullOrEmpty(apiKey) && apiKey != "sk-your-openai-key";

            // Embedder type check
            using var scope = _serviceProvider.CreateScope();
            var embedder = scope.ServiceProvider.GetService<IDocumentEmbedder>();
            vm.EmbedderIsReal = embedder is not NullDocumentEmbedder;

            // MetricEvents last 24h
            var since = DateTime.UtcNow.AddHours(-24);
            var recentEvents = await sqlCtx.MetricEvents
                .Where(e => e.OccurredAt >= since)
                .ToListAsync();

            vm.MetricEventsLast24h = recentEvents.Count;
            vm.AvgLatencyLast24hMs = recentEvents.Count > 0
                ? recentEvents.Average(e => (double)e.LatencyMs)
                : 0.0;
            vm.SlaComplianceLast24hPct = recentEvents.Count > 0
                ? recentEvents.Count(e => e.IsWithinSla) * 100.0 / recentEvents.Count
                : 0.0;

            // Recent agent runs (last 5)
            vm.RecentAgentRuns = await sqlCtx.AgentRunLogs
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new RecentRunSummary
                {
                    Id = r.Id,
                    Query = r.OriginalQuery,
                    Status = r.Status,
                    StartedAt = r.CreatedAt
                })
                .ToListAsync();

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
