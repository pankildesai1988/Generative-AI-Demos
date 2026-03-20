import { Link, useLocation } from "react-router-dom";
import { ShoppingCart, MessageSquare, Upload, Zap } from "lucide-react";

export default function EcommerceLayout({ children }) {
  const location = useLocation();
  const isActive = (path) => location.pathname === path;

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      <aside className="w-64 bg-white border-r border-gray-200 flex flex-col">
        {/* Brand */}
        <div className="p-5 border-b bg-gradient-to-r from-primary-500 to-accent-500">
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
          <Link
            to="/"
            className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition ${
              isActive("/")
                ? "bg-primary-50 text-primary-700"
                : "text-gray-600 hover:bg-gray-100"
            }`}
          >
            <MessageSquare size={18} />
            Product Advisor
          </Link>
          <Link
            to="/upload"
            className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition ${
              isActive("/upload")
                ? "bg-primary-50 text-primary-700"
                : "text-gray-600 hover:bg-gray-100"
            }`}
          >
            <Upload size={18} />
            Upload Catalog
          </Link>
        </nav>

        {/* Footer */}
        <div className="p-4 border-t">
          <div className="flex items-center gap-2 text-xs text-gray-400">
            <Zap size={14} />
            <span>Powered by ArNir AI Platform</span>
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 overflow-hidden">{children}</main>
    </div>
  );
}
