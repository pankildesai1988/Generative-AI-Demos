export default function RiskGauge({ risk }) {
  const score = risk?.score ?? 0;
  const level = risk?.level ?? "Low";
  const circumference = 2 * Math.PI * 42;
  const strokeDashoffset = circumference - (score / 100) * circumference;
  const stroke = score >= 60 ? "#ef4444" : score >= 30 ? "#f59e0b" : "#10b981";

  return (
    <div className="rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-700 dark:bg-gray-900">
      <h4 className="text-sm font-semibold text-gray-900 dark:text-gray-100">
        Risk Score
      </h4>
      <div className="mt-4 flex items-center gap-5">
        <svg width="120" height="120" viewBox="0 0 120 120">
          <circle cx="60" cy="60" r="42" fill="none" stroke="#33415522" strokeWidth="10" />
          <circle
            cx="60"
            cy="60"
            r="42"
            fill="none"
            stroke={stroke}
            strokeWidth="10"
            strokeLinecap="round"
            strokeDasharray={circumference}
            strokeDashoffset={strokeDashoffset}
            transform="rotate(-90 60 60)"
          />
          <text x="60" y="56" textAnchor="middle" className="fill-gray-900 dark:fill-gray-100" fontSize="24" fontWeight="700">
            {score}
          </text>
          <text x="60" y="74" textAnchor="middle" className="fill-gray-500 dark:fill-gray-400" fontSize="12">
            /100
          </text>
        </svg>
        <div>
          <p className="text-lg font-semibold text-gray-900 dark:text-gray-100">{level} Risk</p>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
            Weighted from the latest answer’s risk signals.
          </p>
          <div className="mt-3 flex flex-wrap gap-2">
            {(risk?.factors || []).map((factor) => (
              <span
                key={factor.label}
                className="rounded-full bg-red-50 px-2.5 py-1 text-xs font-medium text-red-700 dark:bg-red-900/20 dark:text-red-300"
              >
                {factor.label}
              </span>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
