using ArNir.Admin.Controllers;
using ArNir.Core.DTOs.Evaluation;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArNir.Tests.Sprint6;

/// <summary>
/// Unit tests for the Admin <see cref="EvaluationController"/>.
/// </summary>
public class EvaluationControllerAdminTests
{
    private readonly Mock<IEvaluationHistoryService> _historyMock = new();
    private readonly Mock<ILogger<EvaluationController>> _loggerMock = new();

    private EvaluationController CreateSut() => new(_historyMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Index_NoEvaluations_ReturnsViewWithZeroCount()
    {
        _historyMock.Setup(x => x.GetStatsAsync(null, null)).ReturnsAsync(new EvaluationStatsDto());
        _historyMock.Setup(x => x.GetRecentAsync(1, 50, null, null, null, null)).ReturnsAsync(new List<EvaluationResultDto>());
        _historyMock.Setup(x => x.GetTotalCountAsync()).ReturnsAsync(0);

        var result = await CreateSut().Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<EvaluationDashboardViewModel>(viewResult.Model);
        Assert.Equal(0, vm.TotalCount);
    }

    [Fact]
    public async Task Index_WithEvaluations_PopulatesViewModel()
    {
        var stats = new EvaluationStatsDto { AvgRelevance = 0.8, AvgFaithfulness = 0.9, TotalEvaluations = 5 };
        var recent = new List<EvaluationResultDto>
        {
            new() { Id = 1, Question = "Q1", Answer = "A1", RelevanceScore = 0.8, FaithfulnessScore = 0.9, Reasoning = "Good", EvaluatedAt = DateTime.UtcNow }
        };

        _historyMock.Setup(x => x.GetStatsAsync(null, null)).ReturnsAsync(stats);
        _historyMock.Setup(x => x.GetRecentAsync(1, 50, null, null, null, null)).ReturnsAsync(recent);
        _historyMock.Setup(x => x.GetTotalCountAsync()).ReturnsAsync(5);

        var result = await CreateSut().Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<EvaluationDashboardViewModel>(viewResult.Model);
        Assert.Equal(5, vm.TotalCount);
        Assert.Single(vm.Recent);
        Assert.Equal(0.8, vm.Stats.AvgRelevance);
    }

    [Fact]
    public async Task Details_NotFound_Returns404()
    {
        _historyMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((EvaluationResultDto?)null);

        var result = await CreateSut().Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_Found_ReturnsViewWithDto()
    {
        var dto = new EvaluationResultDto { Id = 1, Question = "Q", Answer = "A", RelevanceScore = 0.75, FaithfulnessScore = 0.85, Reasoning = "R", EvaluatedAt = DateTime.UtcNow };
        _historyMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(dto);

        var result = await CreateSut().Details(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EvaluationResultDto>(viewResult.Model);
        Assert.Equal(1, model.Id);
    }
}
