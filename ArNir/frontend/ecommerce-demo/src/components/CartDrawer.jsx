import { X, ShoppingCart } from "lucide-react";
import { formatCurrency } from "../utils/productData";

export default function CartDrawer({ open, items = [], subtotal = 0, onClose, onRemove, onClear }) {
  return (
    <>
      {open && <div className="fixed inset-0 z-40 bg-black/40" onClick={onClose} />}
      <aside
        className={`fixed right-0 top-0 z-50 flex h-full w-full max-w-md flex-col border-l border-gray-200 bg-white shadow-2xl transition-transform dark:border-gray-700 dark:bg-gray-900 ${
          open ? "translate-x-0" : "translate-x-full"
        }`}
      >
        <div className="flex items-center justify-between border-b border-gray-200 px-5 py-4 dark:border-gray-700">
          <div className="flex items-center gap-2">
            <ShoppingCart className="text-primary-600 dark:text-primary-400" size={18} />
            <h2 className="font-semibold text-gray-900 dark:text-gray-100">Your Cart</h2>
          </div>
          <button type="button" onClick={onClose} className="rounded-lg p-2 text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800">
            <X size={18} />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-5">
          {items.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-gray-300 p-8 text-center text-sm text-gray-500 dark:border-gray-700 dark:text-gray-400">
              Add products from the recommendation list to build your cart.
            </div>
          ) : (
            <div className="space-y-3">
              {items.map((item) => (
                <div key={item.id} className="rounded-2xl border border-gray-200 p-4 dark:border-gray-700">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="font-medium text-gray-900 dark:text-gray-100">{item.title}</p>
                      <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
                        {formatCurrency(item.priceValue)} x {item.quantity}
                      </p>
                    </div>
                    <button
                      type="button"
                      onClick={() => onRemove(item.id)}
                      className="text-xs font-medium text-rose-600 dark:text-rose-400"
                    >
                      Remove
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="border-t border-gray-200 px-5 py-4 dark:border-gray-700">
          <div className="flex items-center justify-between text-sm text-gray-600 dark:text-gray-300">
            <span>Subtotal</span>
            <span className="font-semibold text-gray-900 dark:text-gray-100">
              {formatCurrency(subtotal)}
            </span>
          </div>
          <button
            type="button"
            onClick={onClear}
            disabled={items.length === 0}
            className="mt-4 w-full rounded-xl bg-primary-600 px-4 py-2.5 text-sm font-medium text-white transition hover:bg-primary-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            Clear Cart
          </button>
        </div>
      </aside>
    </>
  );
}
