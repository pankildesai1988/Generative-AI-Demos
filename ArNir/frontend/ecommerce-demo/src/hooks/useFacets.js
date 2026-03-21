import { useMemo, useState } from "react";

export default function useFacets(products = []) {
  const [selectedCategories, setSelectedCategories] = useState([]);
  const [selectedPriceBands, setSelectedPriceBands] = useState([]);

  const availableCategories = useMemo(() => {
    const counts = new Map();
    products.forEach((product) => {
      counts.set(product.category, (counts.get(product.category) || 0) + 1);
    });
    return Array.from(counts.entries()).map(([value, count]) => ({ value, count }));
  }, [products]);

  const availablePriceBands = useMemo(() => {
    const counts = new Map();
    products.forEach((product) => {
      counts.set(product.priceBand, (counts.get(product.priceBand) || 0) + 1);
    });
    return Array.from(counts.entries()).map(([value, count]) => ({ value, count }));
  }, [products]);

  const toggleCategory = (category) => {
    setSelectedCategories((current) =>
      current.includes(category)
        ? current.filter((item) => item !== category)
        : [...current, category]
    );
  };

  const togglePriceBand = (band) => {
    setSelectedPriceBands((current) =>
      current.includes(band)
        ? current.filter((item) => item !== band)
        : [...current, band]
    );
  };

  const clearFacets = () => {
    setSelectedCategories([]);
    setSelectedPriceBands([]);
  };

  const filteredProducts = useMemo(
    () =>
      products.filter((product) => {
        const categoryMatch =
          selectedCategories.length === 0 ||
          selectedCategories.includes(product.category);
        const priceMatch =
          selectedPriceBands.length === 0 ||
          selectedPriceBands.includes(product.priceBand);

        return categoryMatch && priceMatch;
      }),
    [products, selectedCategories, selectedPriceBands]
  );

  return {
    selectedCategories,
    selectedPriceBands,
    availableCategories,
    availablePriceBands,
    filteredProducts,
    toggleCategory,
    togglePriceBand,
    clearFacets,
  };
}
