import { test, expect } from "@playwright/test";

const ADMIN_USERNAME = "admin";
const ADMIN_EMAIL = "admin@feirb.local";
const ADMIN_PASSWORD = "TestPassword123!";

test.describe("Setup flow", () => {
  test("creates admin account and redirects to login", async ({ page }) => {
    // Reset the database state by calling setup status first
    const statusResponse = await page.request.get("/api/setup/status");
    const status = await statusResponse.json();

    // If setup is already complete, skip this test
    test.skip(status.isComplete, "Setup already completed — run against a fresh database");

    // Navigate to the app — SetupGuard should redirect to /setup
    await page.goto("/");
    await page.waitForURL("**/setup");

    // Step 1: Fill admin account details
    await page.locator("#username").fill(ADMIN_USERNAME);
    await page.locator("#email").fill(ADMIN_EMAIL);
    await page.locator("#password").fill(ADMIN_PASSWORD);
    await page.locator("#confirmPassword").fill(ADMIN_PASSWORD);

    // Click Next to proceed to SMTP configuration
    await page.getByRole("button", { name: "Next" }).click();

    // Step 2: Fill SMTP configuration (use GreenMail dev defaults)
    await page.locator("#smtpHost").fill("localhost");
    await page.locator("#smtpPort").fill("3025");
    await page.locator("#smtpUsername").fill("test@feirb.local");
    await page.locator("#smtpPassword").fill("changeit");

    // Uncheck TLS and auth since GreenMail dev server doesn't require them
    const useTlsCheckbox = page.locator("#smtpUseTls");
    if (await useTlsCheckbox.isChecked()) {
      await useTlsCheckbox.uncheck();
    }

    const requiresAuthCheckbox = page.locator("#smtpRequiresAuth");
    if (await requiresAuthCheckbox.isChecked()) {
      await requiresAuthCheckbox.uncheck();
    }

    // Click Complete Setup
    await page.getByRole("button", { name: "Complete Setup" }).click();

    // Step 3: Verify completion page
    await expect(page.getByText("Setup Complete!")).toBeVisible();

    // Click Go to Login
    await page.getByRole("link", { name: "Go to Login" }).click();

    // Verify redirect to login page with setup=true parameter
    await page.waitForURL("**/login?setup=true");
    await expect(page.locator("#username")).toBeVisible();
  });
});
