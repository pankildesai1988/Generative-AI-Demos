export default function ActionButtons({ loading, onAnalyze, onPredict, onReport }) {
  return (
    <div className="flex flex-wrap gap-3">
      <button
        onClick={onAnalyze}
        disabled={loading}
        className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
      >
        {loading ? "Analyzing..." : "Generate Insight"}
      </button>
      <button
        onClick={onPredict}
        disabled={loading}
        className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
      >
        {loading ? "Predicting..." : "Predict Trend"}
      </button>
      <button
        onClick={onReport}
        disabled={loading}
        className="bg-purple-600 text-white px-4 py-2 rounded hover:bg-purple-700"
      >
        {loading ? "Generating..." : "Generate Report"}
      </button>
    </div>
  );
}
