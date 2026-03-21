import { expect, test } from "@playwright/test";

test("healthcare demo renders core navigation and chat surface", async ({ page }) => {
  await page.route("**/api/documents", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([]),
    });
  });

  await page.goto("/");

  await expect(page.getByRole("heading", { name: "Medical Knowledge Assistant" }).first()).toBeVisible();
  await expect(page.getByText("Upload Documents")).toBeVisible();
  await expect(page.getByText("Upload Documents")).toBeVisible();
  await expect(page.getByPlaceholder("Ask about symptoms, treatments, drug interactions...")).toBeVisible();
});

test("healthcare upload route renders file upload", async ({ page }) => {
  await page.goto("/upload");

  await expect(page.getByRole("heading", { name: "Upload Medical Documents" })).toBeVisible();
  await expect(page.getByText("Drag and drop medical documents here, or click to browse")).toBeVisible();
});
