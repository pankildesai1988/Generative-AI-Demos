using Microsoft.AspNetCore.Mvc;
using ArNir.Services.AI;
using ArNir.Core.DTOs.AI;

[ApiController]
[Route("api/[controller]")]
public class InsightsController : ControllerBase
{
    private readonly InsightEngineService _insightEngine;
    private readonly AnomalyDetectionService _anomalyDetection;
    private readonly PredictiveModelService _predictiveModelService;
    private readonly NarrativeReportService _narrativeReportService;

    public InsightsController(
        InsightEngineService insightEngine,
        AnomalyDetectionService anomalyDetection,
        PredictiveModelService predictiveModelService,
        NarrativeReportService narrativeReportService)
    {
        _insightEngine = insightEngine;
        _anomalyDetection = anomalyDetection;
        _predictiveModelService = predictiveModelService;
        _narrativeReportService = narrativeReportService;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] InsightRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.DataJson))
            return BadRequest("DataJson is required for analysis.");

        var result = await _insightEngine.GenerateInsightsAsync(request);
        return Ok(result);
    }

    [HttpPost("anomalies")]
    public IActionResult DetectAnomalies([FromBody] object payload)
    {
        string json = payload?.ToString() ?? string.Empty;
        var results = _anomalyDetection.AnalyzeJson(json);
        return Ok(new { Count = results.Count, Details = results });
    }

    [HttpPost("predict")]
    public async Task<IActionResult> Predict([FromBody] PredictRequestDto request)
    {
        var result = await _predictiveModelService.GeneratePredictionAsync(request);
        return Ok(result);
    }

    [HttpPost("report")]
    public async Task<IActionResult> GenerateReport([FromBody] NarrativeReportRequestDto dto)
    {
        var result = await _narrativeReportService.GenerateAsync(dto);
        return Ok(result);
    }
}
