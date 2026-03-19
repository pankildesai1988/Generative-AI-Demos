namespace ArNir.Core.DTOs.Evaluation;

/// <summary>Aggregate evaluation statistics with daily trend points.</summary>
public class EvaluationStatsDto
{
    /// <summary>Average relevance score across all evaluations.</summary>
    public double AvgRelevance { get; set; }

    /// <summary>Average faithfulness score across all evaluations.</summary>
    public double AvgFaithfulness { get; set; }

    /// <summary>Total number of evaluations in the period.</summary>
    public int TotalEvaluations { get; set; }

    /// <summary>Daily trend data points.</summary>
    public List<EvaluationTrendPoint> Trends { get; set; } = new();
}

/// <summary>A single daily data point in the evaluation trend chart.</summary>
public class EvaluationTrendPoint
{
    /// <summary>The date (day) this data point represents.</summary>
    public DateTime Date { get; set; }

    /// <summary>Average relevance score for this day.</summary>
    public double AvgRelevance { get; set; }

    /// <summary>Average faithfulness score for this day.</summary>
    public double AvgFaithfulness { get; set; }

    /// <summary>Number of evaluations on this day.</summary>
    public int Count { get; set; }
}
