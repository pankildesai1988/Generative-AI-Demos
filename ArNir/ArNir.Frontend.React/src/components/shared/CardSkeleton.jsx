import Skeleton from "./Skeleton";

export default function CardSkeleton({ count = 4 }) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      {Array.from({ length: count }, (_, i) => (
        <div key={i} className="p-4 border rounded-xl space-y-2">
          <Skeleton variant="text" className="w-1/2" />
          <Skeleton variant="card" />
        </div>
      ))}
    </div>
  );
}
