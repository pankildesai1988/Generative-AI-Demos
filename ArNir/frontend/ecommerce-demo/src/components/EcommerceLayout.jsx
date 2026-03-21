import { useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { ShoppingCart, MessageSquare, Upload, Zap, Sun, Moon, Menu, Heart } from "lucide-react";
import { useTheme } from "@arnir/shared";
import CartDrawer from "./CartDrawer";
import { useCommerce } from "../context/CommerceContext";

export default function EcommerceLayout({ children }) {
  const location = useLocation();
  const { mode, toggleMode } = useTheme();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [cartOpen, setCartOpen] = useState(false);
  const {
    cart: { items, totalItems, subtotal, removeItem, clearCart },
    wishlist: { items: wishlistItems },
  } = useCommerce();
  const isActive = (path) => location.pathname === path;

  const navLinks = [
    { path: "/", icon: MessageSquare, label: "Product Advisor" },
    { path: "/upload", icon: Upload, label: "Upload Catalog" },
  ];

  return (
    <div className="flex h-screen bg-gray-50 dark:bg-gray-950">
      {/* Mobile overlay */}
      {sidebarOpen && (
        <div className="fixed inset-0 bg-black/40 z-30 md:hidden" onClick={() => setSidebarOpen(false)} />
      )}

      {/* Sidebar */}
      <aside className={`fixed md:static inset-y-0 left-0 z-40 w-64 bg-white dark:bg-gray-900 border-r border-gray-200 dark:border-gray-800 flex flex-col transform transition-transform md:translate-x-0 ${sidebarOpen ? "translate-x-0" : "-translate-x-full"}`}>
        {/* Brand */}
        <div className="p-5 border-b dark:border-gray-800 bg-gradient-to-r from-primary-500 to-accent-500">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-white/20 rounded-xl flex items-center justify-center">
              <ShoppingCart className="text-white" size={22} />
            </div>
            <div>
              <h1 className="text-white font-bold text-lg leading-tight">Ecommerce</h1>
              <p className="text-white/70 text-xs">Product Advisor</p>
            </div>
          </div>
        </div>

        {/* Navigation */}
        <nav className="flex-1 p-4 space-y-1">
          {navLinks.map(({ path, icon: Icon, label }) => (
            <Link
              key={path}
              to={path}
              onClick={() => setSidebarOpen(false)}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition ${
                isActive(path)
                  ? "bg-primary-50 dark:bg-primary-900/30 text-primary-700 dark:text-primary-400"
                  : "text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800"
              }`}
            >
              <Icon size={18} />
              {label}
            </Link>
          ))}
        </nav>

        <div className="p-4 border-t dark:border-gray-800 space-y-3">
          <button
            onClick={() => setCartOpen(true)}
            className="flex items-center justify-between w-full px-3 py-2 rounded-lg text-sm text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition"
          >
            <span className="flex items-center gap-2">
              <ShoppingCart size={16} />
              Cart
            </span>
            <span className="rounded-full bg-primary-100 px-2 py-0.5 text-xs font-semibold text-primary-700 dark:bg-primary-900/30 dark:text-primary-300">
              {totalItems}
            </span>
          </button>
          <div className="flex items-center justify-between px-3 py-2 rounded-lg text-sm text-gray-600 dark:text-gray-400 bg-gray-50 dark:bg-gray-800/60">
            <span className="flex items-center gap-2">
              <Heart size={16} />
              Wishlist
            </span>
            <span className="text-xs font-semibold">{wishlistItems.length}</span>
          </div>
          <button
            onClick={toggleMode}
            className="flex items-center gap-2 w-full px-3 py-2 rounded-lg text-sm text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition"
          >
            {mode === "dark" ? <Sun size={16} /> : <Moon size={16} />}
            {mode === "dark" ? "Light Mode" : "Dark Mode"}
          </button>
          <div className="flex items-center gap-2 text-xs text-gray-400 dark:text-gray-500">
            <Zap size={14} />
            <span>Powered by ArNir AI Platform</span>
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        <div className="md:hidden flex items-center gap-3 p-3 border-b dark:border-gray-800 bg-white dark:bg-gray-900">
          <button onClick={() => setSidebarOpen(true)} className="p-1.5 text-gray-600 dark:text-gray-400">
            <Menu size={22} />
          </button>
          <span className="font-semibold text-primary-700 dark:text-primary-400">Product Advisor</span>
          <button
            onClick={() => setCartOpen(true)}
            className="ml-auto inline-flex items-center gap-2 rounded-lg border border-gray-200 px-3 py-1.5 text-sm text-gray-700 dark:border-gray-700 dark:text-gray-200"
          >
            <ShoppingCart size={16} />
            {totalItems}
          </button>
        </div>
        <main className="flex-1 overflow-hidden">{children}</main>
      </div>

      <CartDrawer
        open={cartOpen}
        items={items}
        subtotal={subtotal}
        onClose={() => setCartOpen(false)}
        onRemove={removeItem}
        onClear={clearCart}
      />
    </div>
  );
}
