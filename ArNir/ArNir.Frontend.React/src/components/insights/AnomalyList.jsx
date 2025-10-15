export default function AnomalyList({ anomalies }) {
  if (!anomalies || anomalies.length === 0) return null;

  return (
    <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded">
      <h4 className="font-semibold text-red-600 mb-2">Anomalies Detected:</h4>
      <ul className="list-disc pl-5 text-red-700">
        {anomalies.map((a, i) => (
          <li key={i}>{a}</li>
        ))}
      </ul>
    </div>
  );
}
