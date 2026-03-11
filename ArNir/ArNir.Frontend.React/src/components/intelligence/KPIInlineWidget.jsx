export default function KPIInlineWidget({ chart }) {
  if (!chart || !chart.data) return null;
  const numericPoints = chart.data.filter((d) => typeof d.value === "number");
  if (numericPoints.length === 0) return null;

  const top3 = numericPoints.slice(0, 3);

  return (
    <div className="grid grid-cols-3 gap-3 mt-3">
      {top3.map((item, i) => (
        <div
          key={i}
          className="bg-white border rounded-lg shadow-sm p-3 text-center"
        >
          <h4 className="text-sm text-gray-500 truncate">{item.label}</h4>
          <p className="text-lg font-semibold text-blue-600">
            {item.value.toLocaleString()}
          </p>
          {item.description && (
            <p className="text-xs text-gray-400 mt-1">{item.description}</p>
          )}
        </div>
      ))}
    </div>
  );
}
