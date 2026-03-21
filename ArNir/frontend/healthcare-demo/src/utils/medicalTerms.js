const TERM_GROUPS = {
  conditions: [
    "hypertension",
    "diabetes",
    "hypoglycemia",
    "heart failure",
    "asthma",
    "infection",
    "renal impairment",
  ],
  drugs: [
    "metformin",
    "insulin",
    "warfarin",
    "statins",
    "ace inhibitors",
    "arb",
    "glp-1",
    "ssri",
  ],
  dosages: [
    "\\d+\\s?mg",
    "\\d+\\s?mcg",
    "\\d+\\s?units?",
    "\\d+\\s?ml",
    "\\d+%",
  ],
};

const escapeTerm = (term) =>
  term.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");

const patterns = Object.entries(TERM_GROUPS).map(([category, values]) => ({
  category,
  regex: new RegExp(`\\b(?:${values.map(escapeTerm).join("|")})\\b`, "gi"),
}));

export function extractMedicalTerms(text = "") {
  const matches = [];

  patterns.forEach(({ category, regex }) => {
    for (const match of text.matchAll(regex)) {
      matches.push({
        label: match[0],
        category,
      });
    }
  });

  return Array.from(
    new Map(matches.map((item) => [`${item.category}:${item.label.toLowerCase()}`, item])).values()
  );
}

export function getHighlightTerms(text = "") {
  return extractMedicalTerms(text).map((term) => term.label);
}
