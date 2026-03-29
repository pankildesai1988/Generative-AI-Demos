/**
 * Generic utilities for extracting structured information from LLM RAG answer text.
 * Works with any demo — ecommerce product names, healthcare drug/condition names,
 * finance company/metric names, etc.
 */

/**
 * Extracts names formatted as **bold** markdown from a RAG answer string.
 * Strips common trailing suffixes like prices (" - $29", " ($1,399)") so the
 * returned values are clean display names.
 *
 * Examples:
 *   "**SmartPlug WiFi 4-Pack** - $29"   → "SmartPlug WiFi 4-Pack"
 *   "**GameStorm X17**"                  → "GameStorm X17"
 *   "**iPhone 16 Pro Max** ($1,399)"     → "iPhone 16 Pro Max"
 *   "**Metformin** is first-line..."     → "Metformin"
 */
export function extractBoldNames(answer: string): string[] {
  if (!answer) return [];

  const names: string[] = [];
  const boldRegex = /\*\*([^*\n]{3,100})\*\*/g;
  let match: RegExpExecArray | null;

  while ((match = boldRegex.exec(answer)) !== null) {
    const raw = match[1].trim();
    // Strip trailing price or parenthetical suffix: " - $29", " ($1,399)", " — $89"
    const name = raw.replace(/\s*[-–—(]\s*\$[\d,]+.*$/, "").trim();
    if (name.length >= 3 && name.length <= 100) {
      names.push(name);
    }
  }

  return names;
}
