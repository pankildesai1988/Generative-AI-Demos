import { DollarSign, AlertTriangle, TrendingUp, Lightbulb } from "lucide-react";
import MetricsCard from "./MetricsCard";

/**
 * Extracts financial insights from RAG answer text.
 * Parses for: dollar amounts, percentages, risk keywords, growth indicators.
 */
function extractInsights(text) {
  if (!text) return { amounts: [], risks: [], highlights: [] };

  // Extract dollar amounts
  const amountRegex = /\$[\d,.]+(?:\s*(?:billion|million|B|M|K))?/gi;
  const amounts = [...new Set(text.match(amountRegex) || [])].slice(0, 5);

  // Extract percentage figures
  const pctRegex = /[\d.]+%/g;
  const percentages = [...new Set(text.match(pctRegex) || [])].slice(0, 5);

  // Detect risk keywords
  const riskKeywords = [
    "risk", "decline", "loss", "decrease", "threat", "volatility",
    "uncertainty", "downgrade", "warning", "concern", "debt",
  ];
  const risks = riskKeywords.filter((kw) =>
    text.toLowerCase().includes(kw)
  );

  // Detect growth/positive keywords
  const growthKeywords = [
    "growth", "increase", "profit", "revenue", "gain", "expansion",
    "improvement", "outperform", "beat", "exceed", "strong",
  ];
  const highlights = growthKeywords.filter((kw) =>
    text.toLowerCase().includes(kw)
  );

  return { amounts, percentages, risks, highlights };
}

export default function InsightsPanel({ answer = "", hasData = false }) {
  const insights = extractInsights(answer);
  const hasInsights =
    insights.amounts.length > 0 ||
    insights.risks.length > 0 ||
    insights.highlights.length > 0;

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center gap-2">
        <Lightbulb className="text-accent-500" size={20} />
        <h3 className="text-lg font-semibold text-gray-800">
          Key Insights
        </h3>
      </div>

      {!hasData ? (
        <div className="text-center py-12">
          <TrendingUp className="mx-auto text-gray-300 mb-3" size={40} />
          <p className="text-sm text-gray-400">
            Financial insights will appear here after you ask a question.
          </p>
          <p className="text-xs text-gray-300 mt-1">
            Upload financial documents first to enable AI analysis.
          </p>
        </div>
      ) : !hasInsights ? (
        <div className="bg-gray-50 border rounded-lg p-4 text-center">
          <p className="text-sm text-gray-500">
            No specific financial metrics detected in the latest response.
          </p>
        </div>
      ) : (
        <div className="space-y-3">
          {/* Financial Amounts */}
          {insights.amounts.length > 0 && (
            <div>
              <h4 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
                Financial Figures
              </h4>
              <div className="space-y-2">
                {insights.amounts.map((amt, i) => (
                  <MetricsCard
                    key={i}
                    icon={DollarSign}
                    label={`Amount ${i + 1}`}
                    value={amt}
                    type="amount"
                  />
                ))}
              </div>
            </div>
          )}

          {/* Percentages */}
          {insights.percentages?.length > 0 && (
            <div>
              <h4 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
                Key Percentages
              </h4>
              <div className="flex flex-wrap gap-2">
                {insights.percentages.map((pct, i) => (
                  <span
                    key={i}
                    className="bg-accent-50 text-accent-700 px-3 py-1 rounded-full text-sm font-medium"
                  >
                    {pct}
                  </span>
                ))}
              </div>
            </div>
          )}

          {/* Risk Flags */}
          {insights.risks.length > 0 && (
            <div>
              <h4 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
                Risk Flags
              </h4>
              <div className="space-y-1.5">
                {insights.risks.map((risk, i) => (
                  <div
                    key={i}
                    className="flex items-center gap-2 text-sm text-red-600 bg-red-50 px-3 py-1.5 rounded-lg"
                  >
                    <AlertTriangle size={14} />
                    <span className="capitalize">{risk} detected</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Growth Highlights */}
          {insights.highlights.length > 0 && (
            <div>
              <h4 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
                Positive Indicators
              </h4>
              <div className="flex flex-wrap gap-2">
                {insights.highlights.map((h, i) => (
                  <span
                    key={i}
                    className="bg-green-50 text-green-700 px-3 py-1 rounded-full text-sm font-medium capitalize"
                  >
                    {h}
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
