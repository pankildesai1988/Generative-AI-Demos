import { useMemo, useState } from "react";

export default function useComparison() {
  const [selectedIds, setSelectedIds] = useState([]);

  const toggleProduct = (productId) => {
    setSelectedIds((current) => {
      if (current.includes(productId)) {
        return current.filter((id) => id !== productId);
      }

      if (current.length >= 2) {
        return current;
      }

      return [...current, productId];
    });
  };

  const clearComparison = () => {
    setSelectedIds([]);
  };

  return {
    selectedIds,
    toggleProduct,
    clearComparison,
    canCompare: useMemo(() => selectedIds.length === 2, [selectedIds]),
  };
}
