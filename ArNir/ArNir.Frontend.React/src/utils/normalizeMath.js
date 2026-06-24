/**
 * Coerces the loose math notation LLMs commonly emit into the `$$…$$` / `$…$` delimiters
 * that remark-math recognises, without disturbing already-delimited or non-math text:
 * - converts `\[ … \]` / `\( … \)` to `$$…$$` / `$…$`
 * - wraps a parenthesised line that is clearly a LaTeX formula (contains \frac, \sum,
 *   \text, _ or ^ groups, etc.) in `$$…$$`
 * Lines with no LaTeX markers are returned untouched.
 *
 * @param {string} text Raw assistant answer text.
 * @returns {string} Text with math coerced to KaTeX-compatible delimiters.
 */
export function normalizeMath(text) {
  if (!text) return text;

  let out = text
    .replace(/\\\[([\s\S]+?)\\\]/g, (_m, body) => `$$${body.trim()}$$`)
    .replace(/\\\(([\s\S]+?)\\\)/g, (_m, body) => `$${body.trim()}$`);

  const hasLatex = /\\(frac|sum|sqrt|text|alpha|beta|sigma|mu|theta|cdot|times|partial|int)\b|[_^]\{/;

  out = out
    .split("\n")
    .map((line) => {
      const trimmed = line.trim();
      if (/^\(.+\)$/.test(trimmed) && hasLatex.test(trimmed) && !trimmed.includes("$")) {
        return `$$${trimmed.slice(1, -1).trim()}$$`;
      }
      return line;
    })
    .join("\n");

  return out;
}
