import { Tag, Star } from "lucide-react";

/**
 * Displays a product recommendation derived from a RAG chunk.
 * Parses chunk text for product-like information.
 */
export default function ProductCard({ chunk, index }) {
  const text = chunk.chunkText || chunk.text || chunk.content || "";

  // Try to extract price-like patterns ($xxx)
  const priceMatch = text.match(/\$[\d,]+(?:\.\d{2})?/);
  const price = priceMatch ? priceMatch[0] : null;

  // Use first line as title, rest as description
  const lines = text.split("\n").filter((l) => l.trim());
  const title = lines[0]?.substring(0, 80) || `Product ${index + 1}`;
  const description = lines.slice(1).join(" ").substring(0, 150);

  return (
    <div className="border border-gray-200 rounded-xl p-4 hover:shadow-md hover:border-primary-300 transition bg-white">
      <div className="flex items-start justify-between">
        <div className="flex-1 min-w-0">
          <h4 className="font-semibold text-sm text-gray-900 truncate">
            {title}
          </h4>
          {price && (
            <p className="text-primary-600 font-bold text-lg mt-1">{price}</p>
          )}
        </div>
        <div className="flex items-center gap-0.5 text-accent-500 ml-2">
          <Star size={12} className="fill-current" />
          <span className="text-xs font-medium">
            {chunk.rank ? `#${chunk.rank}` : "Top"}
          </span>
        </div>
      </div>

      {description && (
        <p className="text-xs text-gray-500 mt-2 line-clamp-3">
          {description}
        </p>
      )}

      <div className="flex items-center gap-2 mt-3">
        <Tag size={12} className="text-gray-400" />
        <span className="text-xs text-gray-400">
          {chunk.documentTitle || "Product Catalog"}
        </span>
      </div>
    </div>
  );
}
