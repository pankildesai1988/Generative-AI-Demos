import { useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import {
  MessageSquare,
  BarChart2,
  Lightbulb,
  BrainCircuit,
  Upload,
  Menu,
  X,
  Sun,
  Moon,
} from "lucide-react";
import { useTheme } from "../../context/ThemeContext";

const NAV = [
  { to: "/", label: "Chat", icon: MessageSquare, end: true },
  { to: "/analytics", label: "Analytics", icon: BarChart2 },
  { to: "/insight", label: "Insights", icon: Lightbulb },
  { to: "/intelligence", label: "Intelligence", icon: BrainCircuit },
  { to: "/intelligence/chat", label: "AI Assistant", icon: MessageSquare },
  { to: "/upload", label: "Upload Docs", icon: Upload },
];

export default function AppLayout() {
  const [open, setOpen] = useState(false);
  const { isDark, toggle } = useTheme();

  const linkClass = ({ isActive }) =>
    `flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
      isActive
        ? "bg-blue-600 text-white"
        : "text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700"
    }`;

  return (
    <div className="flex h-screen bg-gray-50 dark:bg-gray-900 overflow-hidden">
      {/* Mobile overlay */}
      {open && (
        <div
          className="fixed inset-0 bg-black/40 z-20 lg:hidden"
          onClick={() => setOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        className={`fixed lg:static inset-y-0 left-0 z-30 w-60 flex flex-col bg-white dark:bg-gray-800 border-r dark:border-gray-700 shadow-sm transition-transform ${
          open ? "translate-x-0" : "-translate-x-full lg:translate-x-0"
        }`}
      >
        {/* Brand */}
        <div className="flex items-center justify-between px-5 py-4 border-b dark:border-gray-700">
          <span className="text-lg font-bold text-gray-900 dark:text-white">
            ArNir <span className="text-blue-600">AI</span>
          </span>
          <button
            className="lg:hidden text-gray-500 dark:text-gray-400"
            onClick={() => setOpen(false)}
          >
            <X size={20} />
          </button>
        </div>

        {/* Nav links */}
        <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
          {NAV.map(({ to, label, icon: Icon, end }) => (
            <NavLink key={to} to={to} end={end} className={linkClass}>
              <Icon size={18} />
              {label}
            </NavLink>
          ))}
        </nav>

        {/* Dark mode toggle */}
        <div className="px-3 py-4 border-t dark:border-gray-700">
          <button
            onClick={toggle}
            className="flex items-center gap-3 w-full px-4 py-2.5 rounded-lg text-sm font-medium text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
          >
            {isDark ? <Sun size={18} /> : <Moon size={18} />}
            {isDark ? "Light Mode" : "Dark Mode"}
          </button>
        </div>
      </aside>

      {/* Main content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Mobile header */}
        <header className="lg:hidden flex items-center gap-3 px-4 py-3 bg-white dark:bg-gray-800 border-b dark:border-gray-700">
          <button
            onClick={() => setOpen(true)}
            className="text-gray-600 dark:text-gray-300"
          >
            <Menu size={22} />
          </button>
          <span className="font-semibold text-gray-900 dark:text-white">
            ArNir AI
          </span>
        </header>

        <main className="flex-1 overflow-y-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
