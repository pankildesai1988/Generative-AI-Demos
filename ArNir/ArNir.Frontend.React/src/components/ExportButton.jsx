import { CSVLink } from "react-csv";

export default function ExportButton({ data }) {
  if (!data || data.length === 0) return null;

  return (
    <CSVLink
      data={data}
      filename={`provider-analytics-${new Date().toISOString()}.csv`}
      className="bg-green-600 text-white px-4 py-2 rounded-xl hover:bg-green-700 transition"
    >
      Export CSV
    </CSVLink>
  );
}
