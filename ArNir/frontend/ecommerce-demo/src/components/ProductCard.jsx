import { ImageOff, ShoppingCart, Star, Tag } from "lucide-react";
import WishlistButton from "./WishlistButton";

export default function ProductCard({
  product,
  compareSelected = false,
  compareDisabled = false,
  onToggleCompare,
  onAddToCart,
  onToggleWishlist,
  isWishlisted = false,
}) {
  const showImage = Boolean(product.imageUrl);

  return (
    <div className="rounded-2xl border border-gray-200 bg-white p-4 transition hover:border-primary-300 hover:shadow-md dark:border-gray-700 dark:bg-gray-900">
      <div className="mb-4 overflow-hidden rounded-2xl border border-gray-100 bg-gradient-to-br from-orange-50 to-amber-50 dark:border-gray-800 dark:from-gray-800 dark:to-gray-900">
        {showImage ? (
          <img
            src={product.imageUrl}
            alt={product.title}
            onError={(event) => {
              event.currentTarget.style.display = "none";
              event.currentTarget.nextSibling.style.display = "flex";
            }}
            className="h-40 w-full object-cover"
          />
        ) : null}
        <div
          className={`h-40 items-center justify-center ${showImage ? "hidden" : "flex"}`}
        >
          <div className="text-center text-gray-400 dark:text-gray-500">
            <ImageOff className="mx-auto mb-2" size={24} />
            <p className="text-xs">No product image metadata</p>
          </div>
        </div>
      </div>

      <div className="flex items-start justify-between">
        <div className="min-w-0 flex-1">
          <h4 className="truncate text-sm font-semibold text-gray-900 dark:text-gray-100">
            {product.title}
          </h4>
          {product.priceLabel && (
            <p className="mt-1 text-lg font-bold text-primary-600 dark:text-primary-400">
              {product.priceLabel}
            </p>
          )}
        </div>
        <div className="ml-2 flex items-center gap-0.5 text-accent-500">
          <Star size={12} className="fill-current" />
          <span className="text-xs font-medium">
            {product.rank ? `#${product.rank}` : "Top"}
          </span>
        </div>
      </div>

      {product.description && (
        <p className="mt-2 line-clamp-3 text-xs text-gray-500 dark:text-gray-400">
          {product.description}
        </p>
      )}

      <div className="mt-3 flex items-center gap-2">
        <Tag size={12} className="text-gray-400 dark:text-gray-500" />
        <span className="text-xs text-gray-400 dark:text-gray-500">
          {product.documentTitle}
        </span>
      </div>

      <div className="mt-3 flex flex-wrap gap-2">
        <span className="rounded-full bg-orange-50 px-2.5 py-1 text-[11px] font-medium text-orange-700 dark:bg-orange-900/20 dark:text-orange-300">
          {product.category}
        </span>
        <span className="rounded-full bg-amber-50 px-2.5 py-1 text-[11px] font-medium text-amber-700 dark:bg-amber-900/20 dark:text-amber-300">
          {product.priceBand}
        </span>
      </div>

      <div className="mt-4 grid grid-cols-[1fr_auto] gap-2">
        <button
          type="button"
          onClick={() => onAddToCart(product)}
          className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-primary-700"
        >
          <ShoppingCart size={16} />
          Add to Cart
        </button>
        <WishlistButton
          active={isWishlisted}
          onToggle={() => onToggleWishlist(product)}
        />
      </div>

      <label className="mt-3 flex items-center gap-2 text-xs font-medium text-gray-600 dark:text-gray-300">
        <input
          type="checkbox"
          checked={compareSelected}
          disabled={compareDisabled}
          onChange={() => onToggleCompare(product.id)}
          className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
        />
        Compare this product
      </label>
    </div>
  );
}
