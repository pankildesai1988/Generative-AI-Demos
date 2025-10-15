export default function TrendSummaryBox({ summary }) {
  if (!summary) return null;
  return (
    <div className="mt-4 bg-green-50 border border-green-200 p-4 rounded">
      <h4 className="font-semibold text-green-700 mb-1">Trend Summary</h4>
      <p className="text-green-800 whitespace-pre-wrap">{summary}</p>
    </div>
  );
}
