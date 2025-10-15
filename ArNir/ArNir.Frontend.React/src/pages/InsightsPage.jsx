import { useState } from "react";
import { getInsights, getPredictions, getReport } from "../api/client";
import { DataInputBox, ActionButtons, InsightSummary, AnomalyList, PredictionChart, TrendSummaryBox, ReportPreview } from "../components/insights";


export default function InsightsPage() {
  const [data, setData] = useState("");
  const [insight, setInsight] = useState("");
  const [anomalies, setAnomalies] = useState([]);
  const [predictions, setPredictions] = useState([]);
  const [trendSummary, setTrendSummary] = useState("");
  const [chartData, setChartData] = useState([]);
  const [report, setReport] = useState("");
  const [loading, setLoading] = useState(false);

  const analyze = async () => {
    setLoading(true);
    try {
      const res = await getInsights({ provider: "OpenAI", model: "gpt-4o-mini", metricType: "Latency", dataJson: data });
      setInsight(res.data.summary || "");
      setAnomalies(res.data.anomalies || []);
    } finally {
      setLoading(false);
    }
  };

  const predict = async () => {
    setLoading(true);
    try {
      const parsed = JSON.parse(data || "[]");
      const values = parsed.map((x) => x.avgLatency || x.value || 0);
      const res = await getPredictions({ provider: "OpenAI", metricType: "Latency", values, forecastPoints: 3, useGPT: true });
      setPredictions(res.data.predictedValues || []);
      setTrendSummary(res.data.trendSummary || "");
      const combined = values.map((v, i) => ({ index: i + 1, actual: v }));
      res.data.predictedValues?.forEach((p, i) => combined.push({ index: values.length + i + 1, predicted: p }));
      setChartData(combined);
    } finally {
      setLoading(false);
    }
  };

  const reportGenerate = async () => {
    setLoading(true);
    try {
      const res = await getReport({ provider: "OpenAI", metricType: "Latency", insights: insight, anomalies, predictions, trendSummary });
      setReport(res.data.reportMarkdown || "");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6 bg-gray-50 min-h-screen">
      <h1 className="text-2xl font-bold mb-4">AI Insights Dashboard</h1>

      <DataInputBox value={data} onChange={setData} />
      <ActionButtons loading={loading} onAnalyze={analyze} onPredict={predict} onReport={reportGenerate} />

      <InsightSummary insight={insight} />
      <AnomalyList anomalies={anomalies} />
      <TrendSummaryBox summary={trendSummary} />
      <PredictionChart chartData={chartData} />
      <ReportPreview report={report} />
    </div>
  );
}
