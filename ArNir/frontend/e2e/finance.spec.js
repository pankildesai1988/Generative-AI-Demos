import { expect, test } from "@playwright/test";

test("finance demo renders analyzer shell", async ({ page }) => {
  await page.goto("/");

  await expect(page.getByRole("heading", { name: "Financial", exact: true })).toBeVisible();
  await expect(page.getByRole("heading", { name: "Financial Document Analyzer" }).first()).toBeVisible();
  await expect(page.getByText("Analyze Documents")).toBeVisible();
  await expect(page.getByPlaceholder("Ask about revenue, risk factors, market trends...")).toBeVisible();
});

test("finance upload route renders financial upload", async ({ page }) => {
  await page.goto("/upload");

  await expect(page.getByRole("heading", { name: "Upload Financial Reports" })).toBeVisible();
  await expect(page.getByText("Drag and drop financial documents here, or click to browse")).toBeVisible();
});
