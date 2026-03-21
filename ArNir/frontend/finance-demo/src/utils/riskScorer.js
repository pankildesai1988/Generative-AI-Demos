const weightedKeywords = [
  { label: "debt", weight: 14 },
  { label: "risk", weight: 12 },
  { label: "volatility", weight: 12 },
  { label: "uncertainty", weight: 10 },
  { label: "decline", weight: 10 },
  { label: "loss", weight: 10 },
  { label: "regulatory", weight: 8 },
  { label: "threat", weight: 8 },
  { label: "downgrade", weight: 8 },
  { label: "warning", weight: 8 },
];

export function scoreRisk(text = "") {
  const normalized = text.toLowerCase();
  const hits = weightedKeywords
    .filter((item) => normalized.includes(item.label))
    .map((item) => ({
      label: item.label,
      weight: item.weight,
    }));

  const rawScore = hits.reduce((sum, item) => sum + item.weight, 0);
  const score = Math.min(100, rawScore);

  let level = "Low";
  if (score >= 60) level = "High";
  else if (score >= 30) level = "Moderate";

  return {
    score,
    level,
    factors: hits,
  };
}
