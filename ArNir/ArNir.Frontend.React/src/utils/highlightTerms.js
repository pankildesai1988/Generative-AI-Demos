// Domain-neutral key-term extractor used by HighlightedMessage + chunk-text highlights.
// Three categories: capitalized entities, numeric quantities, inline `code` spans.

const PATTERNS = [
  { category: "entities", regex: /\b[A-Z][a-z]+(?:\s[A-Z][a-z]+){0,2}\b/g },
  // trailing \b dropped — % isn't a word char so \b fails after % between two non-word chars
  { category: "numbers", regex: /\b\d+(?:\.\d+)?(?:%|ms|s|min|kb|mb|gb|MB|GB|KB)?/g },
  { category: "code", regex: /`([^`]+)`/g },
];

export function extractHighlightTerms(text = "") {
  if (!text) return [];

  const matches = [];

  PATTERNS.forEach(({ category, regex }) => {
    for (const m of text.matchAll(regex)) {
      const label = category === "code" ? m[1] : m[0];
      if (label && label.length > 1) {
        matches.push({ label, category });
      }
    }
  });

  return Array.from(
    new Map(matches.map((item) => [`${item.category}:${item.label.toLowerCase()}`, item])).values()
  );
}

export function getHighlightLabels(text = "") {
  return extractHighlightTerms(text).map((t) => t.label);
}
