import { Package, ShoppingBag } from "lucide-react";
import ProductCard from "./ProductCard";
import ComparisonTable from "./ComparisonTable";

export default function RecommendationList({
  products = [],
  comparisonProducts = [],
  selectedComparisonIds = [],
  onToggleCompare,
  onClearComparison,
  onAddToCart,
  onToggleWishlist,
  isWishlisted,
}) {
  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2">
        <Package className="text-primary-600 dark:text-primary-400" size={20} />
        <h3 className="text-lg font-semibold text-gray-800 dark:text-gray-100">
          Recommendations
        </h3>
      </div>

      {comparisonProducts.length === 2 && (
        <ComparisonTable products={comparisonProducts} onClear={onClearComparison} />
      )}

      {products.length === 0 ? (
        <div className="py-12 text-center">
          <ShoppingBag className="mx-auto mb-3 text-gray-300 dark:text-gray-600" size={40} />
          <p className="text-sm text-gray-400 dark:text-gray-500">
            Product recommendations will appear here after you ask a question.
          </p>
          <p className="mt-1 text-xs text-gray-300 dark:text-gray-600">
            Upload product catalogs first to enable AI-powered recommendations.
          </p>
        </div>
      ) : (
        <>
          <div className="rounded-lg border border-primary-200 bg-primary-50 p-3 dark:border-primary-800 dark:bg-primary-900/20">
            <p className="text-xs text-primary-700 dark:text-primary-300">
              <span className="font-semibold">
                {products.length} product{products.length !== 1 ? "s" : ""}
              </span>{" "}
              matched after applying your current filters.
            </p>
          </div>

          <div className="grid gap-3 xl:grid-cols-2">
            {products.map((product) => (
              <ProductCard
                key={product.id}
                product={product}
                compareSelected={selectedComparisonIds.includes(product.id)}
                compareDisabled={
                  selectedComparisonIds.length >= 2 &&
                  !selectedComparisonIds.includes(product.id)
                }
                onToggleCompare={onToggleCompare}
                onAddToCart={onAddToCart}
                onToggleWishlist={onToggleWishlist}
                isWishlisted={isWishlisted(product.id)}
              />
            ))}
          </div>
        </>
      )}
    </div>
  );
}
