export default function InsightSummary({ insight }) {
  if (!insight) return null;
  return (
    <div className="mt-4 bg-gray-100 p-4 rounded shadow">
      <h4 className="font-semibold text-gray-800">Insight Summary</h4>
      <p className="mt-2 whitespace-pre-wrap text-gray-700">{insight}</p>
    </div>
  );
}
