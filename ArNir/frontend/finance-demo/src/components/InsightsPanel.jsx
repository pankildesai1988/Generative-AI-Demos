import { DollarSign, AlertTriangle, TrendingUp, Lightbulb } from "lucide-react";
import MetricsCard from "./MetricsCard";
import FinanceChart from "./FinanceChart";
import RiskGauge from "./RiskGauge";
import ExportMenu from "./ExportMenu";
import { extractChartData, extractPercentSeries } from "../utils/chartDataExtractor";
import { extractMarkdownTable } from "../utils/tableExtractor";
import { scoreRisk } from "../utils/riskScorer";

export function extractInsights(text) {
  if (!text) return { amounts: [], risks: [], highlights: [] };

  const amountRegex = /\$[\d,.]+(?:\s*(?:billion|million|B|M|K))?/gi;
  const amounts = [...new Set(text.match(amountRegex) || [])].slice(0, 5);

  const growthKeywords = [
    "growth", "increase", "profit", "revenue", "gain", "expansion",
    "improvement", "outperform", "beat", "exceed", "strong",
  ];
  const highlights = growthKeywords.filter((kw) =>
    text.toLowerCase().includes(kw)
  );

  return { amounts, highlights };
}

export default function InsightsPanel({ answer = "", hasData = false }) {
  const insights = extractInsights(answer);
  const chartData = extractChartData(answer);
  const percentages = extractPercentSeries(answer);
  const risk = scoreRisk(answer);
  const table = extractMarkdownTable(answer);
  const hasInsights =
    insights.amounts.length > 0 ||
    insights.highlights.length > 0 ||
    chartData.length > 0 ||
    percentages.length > 0 ||
    risk.factors.length > 0;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <Lightbulb className="text-accent-500" size={20} />
          <h3 className="text-lg font-semibold text-gray-800 dark:text-gray-100">
            Key Insights
          </h3>
        </div>
        {hasData && (
          <ExportMenu answer={answer} chartData={chartData} risk={risk} table={table} />
        )}
      </div>

      {!hasData ? (
        <div className="py-12 text-center">
          <TrendingUp className="mx-auto mb-3 text-gray-300 dark:text-gray-600" size={40} />
          <p className="text-sm text-gray-400 dark:text-gray-500">
            Financial insights will appear here after you ask a question.
          </p>
          <p className="mt-1 text-xs text-gray-300 dark:text-gray-600">
            Upload financial documents first to enable AI analysis.
          </p>
        </div>
      ) : !hasInsights && !table ? (
        <div className="rounded-lg border bg-gray-50 p-4 text-center dark:border-gray-700 dark:bg-gray-900">
          <p className="text-sm text-gray-500 dark:text-gray-400">
            No specific financial metrics detected in the latest response.
          </p>
        </div>
      ) : (
        <div className="space-y-4">
          <RiskGauge risk={risk} />
          <FinanceChart data={chartData} />

          {insights.amounts.length > 0 && (
            <div>
              <h4 className="mb-2 text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
                Financial Figures
              </h4>
              <div className="space-y-2">
                {insights.amounts.map((amt, index) => (
                  <MetricsCard
                    key={amt}
                    icon={DollarSign}
                    label={`Amount ${index + 1}`}
                    value={amt}
                    type="amount"
                  />
                ))}
              </div>
            </div>
          )}

          {percentages.length > 0 && (
            <div>
              <h4 className="mb-2 text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
                Key Percentages
              </h4>
              <div className="flex flex-wrap gap-2">
                {percentages.map((item) => (
                  <span
                    key={`${item.label}-${item.value}`}
                    className="rounded-full bg-accent-50 px-3 py-1 text-sm font-medium text-accent-700 dark:bg-accent-900/20 dark:text-accent-300"
                  >
                    {item.value}%
                  </span>
                ))}
              </div>
            </div>
          )}

          {risk.factors.length > 0 && (
            <div>
              <h4 className="mb-2 text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
                Risk Flags
              </h4>
              <div className="space-y-1.5">
                {risk.factors.map((factor) => (
                  <div
                    key={factor.label}
                    className="flex items-center gap-2 rounded-lg bg-red-50 px-3 py-1.5 text-sm text-red-600 dark:bg-red-900/20 dark:text-red-300"
                  >
                    <AlertTriangle size={14} />
                    <span className="capitalize">{factor.label} detected</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {insights.highlights.length > 0 && (
            <div>
              <h4 className="mb-2 text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
                Positive Indicators
              </h4>
              <div className="flex flex-wrap gap-2">
                {insights.highlights.map((item) => (
                  <span
                    key={item}
                    className="rounded-full bg-green-50 px-3 py-1 text-sm font-medium capitalize text-green-700 dark:bg-green-900/20 dark:text-green-300"
                  >
                    {item}
                  </span>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
