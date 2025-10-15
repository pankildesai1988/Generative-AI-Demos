export default function DataInputBox({ value, onChange }) {
  return (
    <textarea
      className="w-full border rounded p-3 mb-4 text-sm font-mono"
      rows="8"
      placeholder="Paste analytics JSON here..."
      value={value}
      onChange={(e) => onChange(e.target.value)}
    />
  );
}
