import { createContext, useContext } from "react";
import useCart from "../hooks/useCart";
import useWishlist from "../hooks/useWishlist";
import useComparison from "../hooks/useComparison";

const CommerceContext = createContext(null);

export function CommerceProvider({ children }) {
  const cart = useCart();
  const wishlist = useWishlist();
  const comparison = useComparison();

  return (
    <CommerceContext.Provider value={{ cart, wishlist, comparison }}>
      {children}
    </CommerceContext.Provider>
  );
}

export function useCommerce() {
  const value = useContext(CommerceContext);
  if (!value) {
    throw new Error("useCommerce must be used within CommerceProvider");
  }
  return value;
}
