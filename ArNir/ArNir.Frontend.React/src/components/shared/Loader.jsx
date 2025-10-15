export default function Loader({ message = "Loading..." }) {
  return (
    <div className="flex items-center justify-center p-6">
      <div className="flex items-center space-x-3">
        <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-gray-800"></div>
        <span className="text-gray-700 font-medium">{message}</span>
      </div>
    </div>
  );
}
