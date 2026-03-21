import { useMemo, useState } from "react";
import { useChat, ChatWindow } from "@arnir/shared";
import { Sparkles } from "lucide-react";
import { useCommerce } from "../context/CommerceContext";
import { buildProductsFromChunks } from "../utils/productData";
import useFacets from "../hooks/useFacets";
import PriceFilter from "./PriceFilter";
import FacetPanel from "./FacetPanel";
import RecommendationList from "./RecommendationList";

export default function ProductAdvisorPage() {
  const [minPrice, setMinPrice] = useState("");
  const [maxPrice, setMaxPrice] = useState("");
  const chat = useChat({
    provider: "OpenAI",
    model: "gpt-4o-mini",
    promptStyle: "rag",
    topK: 5,
  });
  const { cart, wishlist, comparison } = useCommerce();
  const products = useMemo(() => buildProductsFromChunks(chat.chunks), [chat.chunks]);
  const facets = useFacets(products);

  const comparisonProducts = useMemo(
    () => products.filter((product) => comparison.selectedIds.includes(product.id)),
    [products, comparison.selectedIds]
  );

  const handleSendMessage = (query) => {
    const budgetParts = [];
    if (minPrice) budgetParts.push(`minimum budget $${minPrice}`);
    if (maxPrice) budgetParts.push(`maximum budget $${maxPrice}`);

    const enrichedQuery =
      budgetParts.length > 0
        ? `Budget constraint: ${budgetParts.join(", ")}. Shopper request: ${query}`
        : query;

    return chat.sendMessage(enrichedQuery);
  };

  const resetBudget = () => {
    setMinPrice("");
    setMaxPrice("");
  };

  return (
    <div className="h-full overflow-y-auto bg-gradient-to-br from-orange-50 via-amber-50/50 to-white dark:from-gray-950 dark:via-gray-950 dark:to-gray-900">
      <div className="grid min-h-full gap-4 p-4 xl:grid-cols-[280px_minmax(0,1fr)_420px]">
        <div className="space-y-4">
          <PriceFilter
            minPrice={minPrice}
            maxPrice={maxPrice}
            onMinChange={setMinPrice}
            onMaxChange={setMaxPrice}
            onReset={resetBudget}
          />
          <FacetPanel
            availableCategories={facets.availableCategories}
            availablePriceBands={facets.availablePriceBands}
            selectedCategories={facets.selectedCategories}
            selectedPriceBands={facets.selectedPriceBands}
            onToggleCategory={facets.toggleCategory}
            onTogglePriceBand={facets.togglePriceBand}
            onClear={facets.clearFacets}
          />
        </div>

        <div className="flex min-h-[60vh] flex-col rounded-[28px] border border-orange-100 bg-white/90 p-4 shadow-sm backdrop-blur dark:border-gray-800 dark:bg-gray-900/90">
          <div className="mb-4 flex items-start justify-between gap-3 rounded-2xl border border-orange-100 bg-orange-50/80 px-4 py-3 dark:border-gray-800 dark:bg-gray-800/70">
            <div>
              <div className="flex items-center gap-2">
                <Sparkles className="text-primary-600 dark:text-primary-400" size={18} />
                <h2 className="text-base font-semibold text-gray-900 dark:text-gray-100">
                  Guided Product Discovery
                </h2>
              </div>
              <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
                Use budget and facets to narrow the catalog before or after asking the advisor.
              </p>
            </div>
          </div>

        <ChatWindow
          messages={chat.messages}
          onSend={handleSendMessage}
          loading={chat.loading}
          lastHistoryId={chat.lastHistoryId}
          onClear={chat.clearChat}
          title="Product Advisor"
          placeholder="What kind of laptop are you looking for? Budget? Use case?"
        />
        </div>

        <div className="min-h-[60vh] rounded-[28px] border border-orange-100 bg-white/95 p-4 shadow-sm dark:border-gray-800 dark:bg-gray-900/95">
          <RecommendationList
            products={facets.filteredProducts}
            comparisonProducts={comparisonProducts}
            selectedComparisonIds={comparison.selectedIds}
            onToggleCompare={comparison.toggleProduct}
            onClearComparison={comparison.clearComparison}
            onAddToCart={cart.addItem}
            onToggleWishlist={wishlist.toggleItem}
            isWishlisted={wishlist.isWishlisted}
          />
        </div>
      </div>
    </div>
  );
}
