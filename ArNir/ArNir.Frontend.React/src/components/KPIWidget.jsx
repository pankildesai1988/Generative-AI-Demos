export default function KPIWidget({ title, value, icon }) {
  return (
    <div className="flex items-center justify-between p-5 bg-white rounded-2xl shadow-md w-full">
      <div>
        <p className="text-gray-500 text-sm">{title}</p>
        <p className="text-2xl font-bold">{value}</p>
      </div>
      <div className="text-blue-500 text-3xl">{icon}</div>
    </div>
  );
}
