import { test, expect } from "@playwright/test";

test("chat page renders 3-panel enterprise layout", async ({ page }) => {
  await page.goto("/");
  await expect(page.getByPlaceholder(/ask about anything/i)).toBeVisible();
  await expect(page.getByText("Document Scope")).toBeVisible();
  await expect(page.getByText("Source Documents")).toBeVisible();
});

test("upload page shows dropzone", async ({ page }) => {
  await page.goto("/upload");
  await expect(page.getByText(/upload documents/i)).toBeVisible();
  await expect(page.getByText(/drag & drop/i)).toBeVisible();
});

test("dark mode toggle persists across reload", async ({ page }) => {
  await page.goto("/");
  await page.getByRole("button", { name: /dark mode/i }).click();
  await expect(page.locator("html")).toHaveClass(/dark/);
  await page.reload();
  await expect(page.locator("html")).toHaveClass(/dark/);
});
