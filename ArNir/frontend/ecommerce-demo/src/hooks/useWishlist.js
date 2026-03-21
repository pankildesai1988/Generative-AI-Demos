import { useEffect, useState } from "react";

const STORAGE_KEY = "arnir-ecommerce-wishlist";

function readStoredWishlist() {
  if (typeof window === "undefined") {
    return [];
  }

  try {
    return JSON.parse(window.localStorage.getItem(STORAGE_KEY) || "[]");
  } catch {
    return [];
  }
}

export default function useWishlist() {
  const [items, setItems] = useState(readStoredWishlist);

  useEffect(() => {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
  }, [items]);

  const toggleItem = (product) => {
    setItems((current) =>
      current.some((item) => item.id === product.id)
        ? current.filter((item) => item.id !== product.id)
        : [...current, product]
    );
  };

  const isWishlisted = (productId) =>
    items.some((item) => item.id === productId);

  return {
    items,
    toggleItem,
    isWishlisted,
  };
}
