function slugify(value = "") {
  return value
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "");
}

function readField(text, label) {
  const regex = new RegExp(`^${label}:\\s*(.+)$`, "im");
  return text.match(regex)?.[1]?.trim() || "";
}

function parsePrice(text) {
  const match = text.match(/\$[\d,]+(?:\.\d{2})?/);
  if (!match) {
    return { label: "", value: null };
  }

  return {
    label: match[0],
    value: Number(match[0].replace(/[$,]/g, "")),
  };
}

function getPriceBand(priceValue) {
  if (priceValue == null) return "Unknown";
  if (priceValue < 500) return "Under $500";
  if (priceValue < 1000) return "$500 - $999";
  if (priceValue < 1500) return "$1,000 - $1,499";
  return "$1,500+";
}

function getImageUrl(chunk, text) {
  return (
    chunk.imageUrl ||
    chunk.metadata?.imageUrl ||
    chunk.metadata?.image_url ||
    readField(text, "Image URL") ||
    readField(text, "Image")
  );
}

// Returns true for separator lines like "======" or "------"
function isSeparator(line) {
  return /^[=\-]{3,}$/.test(line);
}

// Matches any line that begins with a known product spec field label followed by ":"
// Used to prevent spec lines from being mistaken for a product title when a chunk
// starts mid-product (e.g., the chunk begins at the "Image URL:" or "Category:" line).
const FIELD_LABEL_RE = /^(Image URL|Image|Category|Price|CPU|Processor|RAM|Storage|Display|GPU|Battery|Weight|Features|Best for|Type|Compatibility|Rating|Camera|5G)\s*:/i;

function isFieldLine(line) {
  return FIELD_LABEL_RE.test(line);
}

/**
 * Normalize chunk text that may have been stored/transmitted without newlines.
 * The RAG backend may join product lines with spaces rather than preserving \n,
 * which breaks readField() (uses ^Label: anchors) and title extraction.
 * This function inserts \n before known field labels in single-line chunks.
 */
function normalizeChunkText(rawText) {
  if (!rawText) return "";
  // Already has enough newlines — return as-is
  const newlineCount = (rawText.match(/\n/g) || []).length;
  if (newlineCount > 2) return rawText;

  // Insert \n before each known product field label
  const fieldLabels = [
    "Category",
    "Image URL",
    "Image",
    "Price",
    "CPU",
    "Processor",
    "RAM",
    "Storage",
    "Display",
    "GPU",
    "Battery",
    "Weight",
    "Features",
    "Best for",
    "Type",
    "Compatibility",
    "Rating",
    "Camera",
    "5G",
  ];

  let text = rawText;
  for (const label of fieldLabels) {
    text = text.replace(new RegExp(`\\s+(${label}:)`, "g"), "\n$1");
  }
  return text;
}

/**
 * Split a normalised chunk text into individual product sub-texts.
 * Handles the common case where one RAG chunk spans multiple product entries.
 * Product boundaries are detected by numbered lines: "1. ProductName", "2. ProductName", …
 */
function splitOnProductBoundaries(text) {
  const lines = text.split("\n");
  const productStarts = [];

  lines.forEach((line, i) => {
    if (/^\d+\.\s+\S/.test(line.trim())) {
      productStarts.push(i);
    }
  });

  // 0 or 1 product boundary found — treat the whole text as one entry
  if (productStarts.length <= 1) return [text];

  return productStarts.map((start, idx) => {
    const end = productStarts[idx + 1] ?? lines.length;
    return lines.slice(start, end).join("\n");
  });
}

export function parseProductChunk(chunk, index = 0) {
  const rawText = chunk.chunkText || chunk.text || chunk.content || "";
  // Normalise first so all field parsing works regardless of how the backend serialised the chunk
  const text = normalizeChunkText(rawText);

  const lines = text.split("\n").map((line) => line.trim()).filter((l) => l && !isSeparator(l));

  // Prefer a numbered-list product name line: "1. ProductName" → "ProductName"
  const numberedLine = lines.find((l) => /^\d+\.\s+\S/.test(l));
  // Fallback 1: first short line with no colon — product names never have colons; spec fields always do
  const nonSpecLine = lines.find((l) => !l.includes(":") && l.length >= 3 && l.length < 80);
  // Fallback 2: first line that is NOT a known field label AND has no colon.
  // The colon guard catches partial field labels like "mage URL:" (chunk started mid-word at a
  // field line) that isFieldLine() cannot detect because the label prefix is truncated.
  const fallbackLine = lines.find((l) => !isFieldLine(l) && !l.includes(":")) || "";
  const rawTitle = numberedLine
    ? numberedLine.replace(/^\d+\.\s+/, "")
    : nonSpecLine || fallbackLine;
  const title = rawTitle.substring(0, 120) || `Product ${index + 1}`;
  const category = readField(text, "Category") || readField(text, "Type") || "General";
  const bestFor = readField(text, "Best for");
  const features = readField(text, "Features");
  const cpu = readField(text, "CPU") || readField(text, "Processor");
  const ram = readField(text, "RAM");
  const storage = readField(text, "Storage");
  const display = readField(text, "Display");
  const gpu = readField(text, "GPU");
  const battery = readField(text, "Battery");
  const weight = readField(text, "Weight");
  const price = parsePrice(text);
  const description =
    bestFor ||
    lines
      .filter((line) => !/^(price|category|cpu|ram|storage|display|gpu|battery|weight|features|best for|image url|image):/i.test(line))
      .slice(1)
      .join(" ")
      .substring(0, 180);

  return {
    id: chunk.documentId ? `${chunk.documentId}-${index}-${slugify(title)}` : `${index}-${slugify(title)}`,
    title,
    description,
    category,
    priceLabel: price.label,
    priceValue: price.value,
    priceBand: getPriceBand(price.value),
    bestFor,
    features,
    cpu,
    ram,
    storage,
    display,
    gpu,
    battery,
    weight,
    imageUrl: getImageUrl(chunk, text),
    documentTitle: chunk.documentTitle || "Product Catalog",
    rank: chunk.rank,
    rawChunk: chunk,
  };
}

export function buildProductsFromChunks(chunks = []) {
  const all = [];
  const seen = new Set(); // deduplicate by slugified title (same product can appear in multiple chunks)

  for (const chunk of chunks) {
    const rawText = chunk.chunkText || chunk.text || chunk.content || "";
    const normalised = normalizeChunkText(rawText);

    // Split chunks that span multiple product entries into individual sub-texts
    const subTexts = splitOnProductBoundaries(normalised);

    for (const subText of subTexts) {
      if (subText.trim().length < 10) continue; // skip blank/header-only fragments

      const subChunk = { ...chunk, chunkText: subText, text: subText, content: subText };
      const product = parseProductChunk(subChunk, all.length);

      // Deduplicate — skip if we already have this product from another chunk
      const key = slugify(product.title);
      if (!key || key.length < 2 || seen.has(key)) continue;

      seen.add(key);
      all.push(product);
    }
  }

  return all;
}

export function formatCurrency(value) {
  if (value == null || Number.isNaN(value)) {
    return "N/A";
  }

  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    maximumFractionDigits: 0,
  }).format(value);
}
