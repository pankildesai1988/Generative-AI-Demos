const path = require("node:path");
const { defineConfig } = require("@playwright/test");

const rootDir = path.resolve(__dirname, "..");

module.exports = defineConfig({
  testDir: "./e2e",
  fullyParallel: true,
  timeout: 30_000,
  expect: {
    timeout: 5_000,
  },
  reporter: "list",
  use: {
    headless: true,
    trace: "on-first-retry",
  },
  projects: [
    {
      name: "healthcare",
      testMatch: /healthcare\.spec\.js/,
      use: {
        baseURL: "http://127.0.0.1:3001",
      },
    },
    {
      name: "ecommerce",
      testMatch: /ecommerce\.spec\.js/,
      use: {
        baseURL: "http://127.0.0.1:3002",
      },
    },
    {
      name: "finance",
      testMatch: /finance\.spec\.js/,
      use: {
        baseURL: "http://127.0.0.1:3003",
      },
    },
  ],
  webServer: [
    {
      command: "npm run dev --workspace=@arnir/healthcare-demo -- --host 127.0.0.1 --strictPort",
      url: "http://127.0.0.1:3001",
      cwd: rootDir,
      reuseExistingServer: !process.env.CI,
      timeout: 120_000,
    },
    {
      command: "npm run dev --workspace=@arnir/ecommerce-demo -- --host 127.0.0.1 --strictPort",
      url: "http://127.0.0.1:3002",
      cwd: rootDir,
      reuseExistingServer: !process.env.CI,
      timeout: 120_000,
    },
    {
      command: "npm run dev --workspace=@arnir/finance-demo -- --host 127.0.0.1 --strictPort",
      url: "http://127.0.0.1:3003",
      cwd: rootDir,
      reuseExistingServer: !process.env.CI,
      timeout: 120_000,
    },
  ],
});
