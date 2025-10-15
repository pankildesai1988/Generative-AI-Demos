export default function KPIWidget({ title, value, trend }) {
  return (
    <div className="bg-white border rounded p-4 shadow-sm flex flex-col">
      <h4 className="text-sm text-gray-500">{title}</h4>
      <span className="text-2xl font-semibold text-gray-800 mt-1">{value}</span>
      {trend && (
        <span
          className={`text-sm mt-1 ${
            trend.includes("+") ? "text-green-600" : "text-red-600"
          }`}
        >
          {trend}
        </span>
      )}
    </div>
  );
}
