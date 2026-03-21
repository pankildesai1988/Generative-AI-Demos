import InsightsPanel from "./InsightsPanel";
import FinanceChart from "./FinanceChart";
import RiskGauge from "./RiskGauge";

export default function ComparisonDashboard({ leftEntry, rightEntry }) {
  if (!leftEntry || !rightEntry) {
    return (
      <div className="rounded-2xl border border-dashed border-gray-300 bg-white p-8 text-center text-sm text-gray-500 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-400">
        Select two saved analyses to compare.
      </div>
    );
  }

  const cards = [leftEntry, rightEntry];

  return (
    <div className="grid gap-4 xl:grid-cols-2">
      {cards.map((entry) => (
        <div key={entry.id} className="space-y-4 rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-700 dark:bg-gray-900">
          <div>
            <p className="text-xs uppercase tracking-wide text-gray-500 dark:text-gray-400">
              {new Date(entry.createdAt).toLocaleString()}
            </p>
            <h3 className="mt-1 text-lg font-semibold text-gray-900 dark:text-gray-100">
              {entry.query}
            </h3>
          </div>
          <RiskGauge risk={entry.risk} />
          <FinanceChart data={entry.chartData} />
          <InsightsPanel answer={entry.answer} hasData />
        </div>
      ))}
    </div>
  );
}
