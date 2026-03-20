import { Package, ShoppingBag } from "lucide-react";
import ProductCard from "./ProductCard";

export default function RecommendationList({ chunks = [] }) {
  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center gap-2">
        <Package className="text-primary-600" size={20} />
        <h3 className="text-lg font-semibold text-gray-800">
          Recommendations
        </h3>
      </div>

      {chunks.length === 0 ? (
        <div className="text-center py-12">
          <ShoppingBag className="mx-auto text-gray-300 mb-3" size={40} />
          <p className="text-sm text-gray-400">
            Product recommendations will appear here after you ask a question.
          </p>
          <p className="text-xs text-gray-300 mt-1">
            Upload product catalogs first to enable AI-powered recommendations.
          </p>
        </div>
      ) : (
        <>
          <div className="bg-primary-50 border border-primary-200 rounded-lg p-3">
            <p className="text-xs text-primary-700">
              <span className="font-semibold">
                {chunks.length} product{chunks.length !== 1 ? "s" : ""}
              </span>{" "}
              matched from your catalog.
            </p>
          </div>
          <div className="space-y-3">
            {chunks.map((chunk, i) => (
              <ProductCard key={i} chunk={chunk} index={i} />
            ))}
          </div>
        </>
      )}
    </div>
  );
}
