import { useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { Heart, MessageSquare, Upload, Activity, Sun, Moon, Menu, X } from "lucide-react";
import { useTheme } from "@arnir/shared";

export default function HealthcareLayout({ children }) {
  const location = useLocation();
  const { mode, toggleMode } = useTheme();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const isActive = (path) => location.pathname === path;

  const navLinks = [
    { path: "/", icon: MessageSquare, label: "Ask a Question" },
    { path: "/upload", icon: Upload, label: "Upload Documents" },
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
        <div className="p-5 border-b dark:border-gray-800 bg-gradient-to-r from-primary-600 to-accent-600">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-white/20 rounded-xl flex items-center justify-center">
              <Heart className="text-white" size={22} />
            </div>
            <div>
              <h1 className="text-white font-bold text-lg leading-tight">Healthcare</h1>
              <p className="text-white/70 text-xs">Knowledge Assistant</p>
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

        {/* Footer */}
        <div className="p-4 border-t dark:border-gray-800 space-y-3">
          <button
            onClick={toggleMode}
            className="flex items-center gap-2 w-full px-3 py-2 rounded-lg text-sm text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition"
          >
            {mode === "dark" ? <Sun size={16} /> : <Moon size={16} />}
            {mode === "dark" ? "Light Mode" : "Dark Mode"}
          </button>
          <div className="flex items-center gap-2 text-xs text-gray-400 dark:text-gray-500">
            <Activity size={14} />
            <span>Powered by ArNir AI Platform</span>
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Mobile header */}
        <div className="md:hidden flex items-center gap-3 p-3 border-b dark:border-gray-800 bg-white dark:bg-gray-900">
          <button onClick={() => setSidebarOpen(true)} className="p-1.5 text-gray-600 dark:text-gray-400">
            <Menu size={22} />
          </button>
          <span className="font-semibold text-primary-700 dark:text-primary-400">Healthcare Assistant</span>
        </div>
        <main className="flex-1 overflow-hidden">{children}</main>
      </div>
    </div>
  );
}
