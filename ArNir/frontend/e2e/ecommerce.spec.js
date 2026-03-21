import { expect, test } from "@playwright/test";

test("ecommerce demo renders advisor shell", async ({ page }) => {
  await page.goto("/");

  await expect(page.getByText("Ecommerce")).toBeVisible();
  await expect(page.getByRole("heading", { name: "Guided Product Discovery" })).toBeVisible();
  await expect(page.getByText("Upload Catalog")).toBeVisible();
  await expect(page.getByPlaceholder("What kind of laptop are you looking for? Budget? Use case?")).toBeVisible();
});

test("ecommerce upload route renders catalog upload", async ({ page }) => {
  await page.goto("/upload");

  await expect(page.getByRole("heading", { name: "Upload Product Catalog" })).toBeVisible();
  await expect(page.getByText("Drag and drop product catalogs here, or click to browse")).toBeVisible();
});
