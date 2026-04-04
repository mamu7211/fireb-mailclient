import { test, expect } from "@playwright/test";

const ADMIN_USERNAME = process.env.ADMIN_USERNAME ?? "admin-playwright";
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD ?? "TestPassword123!";

test.describe("SMTP Require Authentication toggle hides credentials", () => {
  test.describe("Setup wizard (step 2)", () => {
    test("credentials are hidden when Require Authentication is unchecked", async ({
      page,
    }) => {
      const statusResponse = await page.request.get("/api/setup/status");
      const status = await statusResponse.json();

      if (status.isComplete) {
        test.skip();
        return;
      }

      await page.goto("/");
      await page.waitForURL("**/setup");

      // Step 1: Fill admin account to reach step 2
      await page.locator("#username").fill("setup-toggle-test");
      await page.locator("#email").fill("toggle@feirb.local");
      await page.locator("#password").fill("TestPassword123!");
      await page.locator("#confirmPassword").fill("TestPassword123!");
      await page.getByRole("button", { name: "Next" }).click();

      // Step 2: SMTP — RequiresAuth is checked by default
      const requiresAuth = page.locator("#smtpRequiresAuth");
      const usernameField = page.locator("#smtpUsername");
      const passwordField = page.locator("#smtpPassword");

      await expect(requiresAuth).toBeChecked();
      await expect(usernameField).toBeVisible();
      await expect(passwordField).toBeVisible();

      // Uncheck — credentials should disappear
      await requiresAuth.uncheck();
      await expect(usernameField).toBeHidden();
      await expect(passwordField).toBeHidden();

      // Re-check — credentials should reappear
      await requiresAuth.check();
      await expect(usernameField).toBeVisible();
      await expect(passwordField).toBeVisible();
    });
  });

  test.describe("Admin Outgoing SMTP settings", () => {
    test.beforeEach(async ({ page }) => {
      await page.goto("/");
      await page.waitForLoadState("networkidle");

      if (page.url().includes("/login") || page.url().includes("/setup")) {
        await page.goto("/login");
        await expect(page.locator("#username")).toBeVisible({ timeout: 10000 });
        await page.locator("#username").fill(ADMIN_USERNAME);
        await page.locator("#password").fill(ADMIN_PASSWORD);
        await page.getByRole("button", { name: "Log In" }).click();
        await expect(
          page.getByLabel("breadcrumb").getByText("Dashboard"),
        ).toBeVisible({ timeout: 15000 });
      }
    });

    test("credentials are visible when Require Authentication is checked", async ({
      page,
    }) => {
      await page.goto("/admin/outgoing-smtp");
      await expect(page.locator(".spinner-border")).toBeHidden({
        timeout: 10000,
      });

      const requiresAuth = page.locator("#smtpRequiresAuth");

      // Ensure RequiresAuth is checked
      if (!(await requiresAuth.isChecked())) {
        await requiresAuth.check();
      }

      await expect(page.locator("#smtpUsername")).toBeVisible();
      await expect(page.locator("#smtpPassword")).toBeVisible();
    });

    test("credentials are hidden when Require Authentication is unchecked", async ({
      page,
    }) => {
      await page.goto("/admin/outgoing-smtp");
      await expect(page.locator(".spinner-border")).toBeHidden({
        timeout: 10000,
      });

      const requiresAuth = page.locator("#smtpRequiresAuth");

      // Ensure RequiresAuth is checked first so fields are visible
      if (!(await requiresAuth.isChecked())) {
        await requiresAuth.check();
      }
      await expect(page.locator("#smtpUsername")).toBeVisible();
      await expect(page.locator("#smtpPassword")).toBeVisible();

      // Uncheck — credentials should disappear
      await requiresAuth.uncheck();
      await expect(page.locator("#smtpUsername")).toBeHidden();
      await expect(page.locator("#smtpPassword")).toBeHidden();
    });

    test("toggling Require Authentication back on restores credential fields", async ({
      page,
    }) => {
      await page.goto("/admin/outgoing-smtp");
      await expect(page.locator(".spinner-border")).toBeHidden({
        timeout: 10000,
      });

      const requiresAuth = page.locator("#smtpRequiresAuth");

      // Uncheck first
      if (await requiresAuth.isChecked()) {
        await requiresAuth.uncheck();
      }
      await expect(page.locator("#smtpUsername")).toBeHidden();
      await expect(page.locator("#smtpPassword")).toBeHidden();

      // Re-check — fields should reappear
      await requiresAuth.check();
      await expect(page.locator("#smtpUsername")).toBeVisible();
      await expect(page.locator("#smtpPassword")).toBeVisible();
    });
  });
});
