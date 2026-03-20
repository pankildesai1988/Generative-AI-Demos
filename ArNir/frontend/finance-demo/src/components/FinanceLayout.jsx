import { Link, useLocation } from "react-router-dom";
import { BarChart3, MessageSquare, Upload, TrendingUp } from "lucide-react";

export default function FinanceLayout({ children }) {
  const location = useLocation();
  const isActive = (path) => location.pathname === path;

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar — Dark theme for finance */}
      <aside className="w-64 bg-primary-900 border-r border-primary-800 flex flex-col">
        {/* Brand */}
        <div className="p-5 border-b border-primary-700">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-accent-500/20 rounded-xl flex items-center justify-center">
              <BarChart3 className="text-accent-400" size={22} />
            </div>
            <div>
              <h1 className="text-white font-bold text-lg leading-tight">Financial</h1>
              <p className="text-gray-400 text-xs">Document Analyzer</p>
            </div>
          </div>
        </div>

        {/* Navigation */}
        <nav className="flex-1 p-4 space-y-1">
          <Link
            to="/"
            className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition ${
              isActive("/")
                ? "bg-primary-700 text-accent-400"
                : "text-gray-400 hover:bg-primary-800 hover:text-gray-200"
            }`}
          >
            <MessageSquare size={18} />
            Analyze Documents
          </Link>
          <Link
            to="/upload"
            className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition ${
              isActive("/upload")
                ? "bg-primary-700 text-accent-400"
                : "text-gray-400 hover:bg-primary-800 hover:text-gray-200"
            }`}
          >
            <Upload size={18} />
            Upload Reports
          </Link>
        </nav>

        {/* Footer */}
        <div className="p-4 border-t border-primary-700">
          <div className="flex items-center gap-2 text-xs text-gray-500">
            <TrendingUp size={14} />
            <span>Powered by ArNir AI Platform</span>
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 overflow-hidden">{children}</main>
    </div>
  );
}
