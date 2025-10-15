using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArNir.Services.AI
{
    public class AnomalyDetectionService
    {
        /// <summary>
        /// Analyzes numeric values from a JSON dataset and detects anomalies.
        /// Uses a dynamic threshold multiplier: smaller datasets get higher sensitivity.
        /// </summary>
        public List<string> AnalyzeJson(string jsonData, double thresholdMultiplier = 2.0)
        {
            if (string.IsNullOrWhiteSpace(jsonData))
                return new List<string> { "⚠️ No data provided for anomaly detection." };

            var numericValues = ExtractNumericValues(jsonData);
            if (numericValues.Count == 0)
                return new List<string> { "ℹ️ No numeric fields found for anomaly analysis." };

            // Automatically increase sensitivity for small datasets
            if (numericValues.Count < 5)
                thresholdMultiplier = 1.0;
            if (numericValues.Count < 3)
                return new List<string> { $"⚠️ Not enough numeric data ({numericValues.Count} points) to detect anomalies." };

            return DetectAnomalies(numericValues, thresholdMultiplier);
        }

        /// <summary>
        /// Extracts all numeric fields (case-insensitive) from JSON objects.
        /// </summary>
        private List<double> ExtractNumericValues(string jsonData)
        {
            var numericValues = new List<double>();
            var possibleKeys = new[] { "avgLatency", "slaCompliance", "avgRating", "latency", "sla", "value" };

            try
            {
                var array = JsonConvert.DeserializeObject<List<object>>(jsonData);
                foreach (var obj in array)
                {
                    if (obj is JObject jObj)
                    {
                        foreach (var prop in jObj.Properties())
                        {
                            string key = prop.Name.ToLowerInvariant();
                            string? match = possibleKeys.FirstOrDefault(k => key.Contains(k.ToLowerInvariant()));
                            if (match != null && double.TryParse(prop.Value.ToString(), out double val))
                                numericValues.Add(val);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ JSON parsing error in AnomalyDetectionService: {ex.Message}");
            }

            return numericValues;
        }

        /// <summary>
        /// Detects anomalies using mean ± (multiplier × std deviation).
        /// For small datasets, thresholdMultiplier is automatically reduced.
        /// </summary>
        public List<string> DetectAnomalies(List<double> values, double thresholdMultiplier = 2.0)
        {
            if (values == null || values.Count < 3)
                return new List<string> { "⚠️ Not enough data points to detect anomalies." };

            double mean = values.Average();
            double stdDev = Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / values.Count);
            double upper = mean + thresholdMultiplier * stdDev;
            double lower = mean - thresholdMultiplier * stdDev;

            var anomalies = new List<string>();
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] > upper || values[i] < lower)
                    anomalies.Add($"⚠️ Index {i}: {values[i]} (outside range {lower:F2}–{upper:F2})");
            }

            // Always include context info for debugging or analytics dashboards
            anomalies.Add($"📊 Mean = {mean:F2}, StdDev = {stdDev:F2}, Range = [{lower:F2}, {upper:F2}], Threshold = {thresholdMultiplier:F1}σ");

            if (anomalies.Count == 1) // Only the context line → no true anomalies
                anomalies.Insert(0, "✅ No anomalies detected.");

            return anomalies;
        }
    }
}
