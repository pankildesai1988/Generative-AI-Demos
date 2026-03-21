const amountRegex = /\$[\d,.]+(?:\s*(?:billion|million|B|M|K))?/gi;
const percentageRegex = /([\w\s/&-]+?)[:\s(]+(\d+(?:\.\d+)?)%/gi;

function normalizeAmount(rawAmount) {
  const cleaned = rawAmount.toLowerCase().replace(/[$,\s]/g, "");
  const base = parseFloat(cleaned.replace(/[a-z]/g, ""));

  if (Number.isNaN(base)) {
    return null;
  }

  if (cleaned.includes("billion") || cleaned.endsWith("b")) {
    return base * 1000;
  }
  if (cleaned.includes("million") || cleaned.endsWith("m")) {
    return base;
  }
  if (cleaned.endsWith("k")) {
    return base / 1000;
  }

  return base / 1000000;
}

export function extractChartData(text = "") {
  if (!text) return [];

  const lines = text.split("\n").map((line) => line.trim()).filter(Boolean);
  const rows = [];

  lines.forEach((line, index) => {
    const labelMatch = line.match(/^([A-Za-z][A-Za-z\s/&-]+):\s*(\$[\d,.]+(?:\s*(?:billion|million|B|M|K))?)/i);
    if (labelMatch) {
      const value = normalizeAmount(labelMatch[2]);
      if (value != null) {
        rows.push({
          label: labelMatch[1].trim(),
          value,
          formatted: labelMatch[2],
        });
      }
      return;
    }

    const amountMatch = [...line.matchAll(amountRegex)][0];
    if (amountMatch) {
      const value = normalizeAmount(amountMatch[0]);
      if (value != null) {
        rows.push({
          label: `Metric ${rows.length + 1 || index + 1}`,
          value,
          formatted: amountMatch[0],
        });
      }
    }
  });

  return rows.slice(0, 6);
}

export function extractPercentSeries(text = "") {
  const rows = [];
  for (const match of text.matchAll(percentageRegex)) {
    rows.push({
      label: match[1].trim().replace(/\s+/g, " ").slice(-28),
      value: Number(match[2]),
    });
  }
  return rows.slice(0, 6);
}
