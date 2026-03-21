import { Heart } from "lucide-react";

export default function WishlistButton({ active = false, onToggle }) {
  return (
    <button
      type="button"
      onClick={onToggle}
      className={`inline-flex items-center justify-center rounded-xl border px-3 py-2 text-sm font-medium transition ${
        active
          ? "border-rose-200 bg-rose-50 text-rose-600 dark:border-rose-800 dark:bg-rose-900/20 dark:text-rose-300"
          : "border-gray-200 bg-white text-gray-600 hover:border-rose-200 hover:text-rose-600 dark:border-gray-700 dark:bg-gray-950 dark:text-gray-300"
      }`}
      aria-label={active ? "Remove from wishlist" : "Add to wishlist"}
    >
      <Heart size={16} className={active ? "fill-current" : ""} />
    </button>
  );
}
