import { useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { BarChart3, MessageSquare, Upload, TrendingUp, Sun, Moon, Menu } from "lucide-react";
import { useTheme } from "@arnir/shared";

export default function FinanceLayout({ children }) {
  const location = useLocation();
  const { mode, toggleMode } = useTheme();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const isActive = (path) => location.pathname === path;

  const navLinks = [
    { path: "/", icon: MessageSquare, label: "Analyze Documents" },
    { path: "/upload", icon: Upload, label: "Upload Reports" },
  ];

  return (
    <div className="flex h-screen bg-gray-50 dark:bg-gray-950">
      {/* Mobile overlay */}
      {sidebarOpen && (
        <div className="fixed inset-0 bg-black/40 z-30 md:hidden" onClick={() => setSidebarOpen(false)} />
      )}

      {/* Sidebar — Dark theme for finance */}
      <aside className={`fixed md:static inset-y-0 left-0 z-40 w-64 bg-primary-900 dark:bg-gray-900 border-r border-primary-800 dark:border-gray-800 flex flex-col transform transition-transform md:translate-x-0 ${sidebarOpen ? "translate-x-0" : "-translate-x-full"}`}>
        {/* Brand */}
        <div className="p-5 border-b border-primary-700 dark:border-gray-800">
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
          {navLinks.map(({ path, icon: Icon, label }) => (
            <Link
              key={path}
              to={path}
              onClick={() => setSidebarOpen(false)}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition ${
                isActive(path)
                  ? "bg-primary-700 dark:bg-gray-800 text-accent-400"
                  : "text-gray-400 hover:bg-primary-800 dark:hover:bg-gray-800 hover:text-gray-200"
              }`}
            >
              <Icon size={18} />
              {label}
            </Link>
          ))}
        </nav>

        {/* Footer */}
        <div className="p-4 border-t border-primary-700 dark:border-gray-800 space-y-3">
          <button
            onClick={toggleMode}
            className="flex items-center gap-2 w-full px-3 py-2 rounded-lg text-sm text-gray-400 hover:bg-primary-800 dark:hover:bg-gray-800 transition"
          >
            {mode === "dark" ? <Sun size={16} /> : <Moon size={16} />}
            {mode === "dark" ? "Light Mode" : "Dark Mode"}
          </button>
          <div className="flex items-center gap-2 text-xs text-gray-500">
            <TrendingUp size={14} />
            <span>Powered by ArNir AI Platform</span>
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Mobile header */}
        <div className="md:hidden flex items-center gap-3 p-3 border-b border-primary-800 dark:border-gray-800 bg-primary-900 dark:bg-gray-900">
          <button onClick={() => setSidebarOpen(true)} className="p-1.5 text-gray-400">
            <Menu size={22} />
          </button>
          <span className="font-semibold text-accent-400">Financial Analyzer</span>
        </div>
        <main className="flex-1 overflow-hidden">{children}</main>
      </div>
    </div>
  );
}
