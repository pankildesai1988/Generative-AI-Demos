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

export function parseProductChunk(chunk, index = 0) {
  const text = chunk.chunkText || chunk.text || chunk.content || "";
  const lines = text.split("\n").map((line) => line.trim()).filter(Boolean);
  const title = lines[0]?.substring(0, 120) || `Product ${index + 1}`;
  const category = readField(text, "Category") || "General";
  const bestFor = readField(text, "Best for");
  const features = readField(text, "Features");
  const cpu = readField(text, "CPU");
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
  return chunks.map((chunk, index) => parseProductChunk(chunk, index));
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
