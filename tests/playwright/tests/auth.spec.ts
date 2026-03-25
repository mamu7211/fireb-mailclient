import { test, expect } from "@playwright/test";

const ADMIN_USERNAME = "admin";
const ADMIN_PASSWORD = "TestPassword123!";

test.describe("Login and logout flow", () => {
  test.beforeEach(async ({ page }) => {
    // Ensure setup is complete before running auth tests
    const statusResponse = await page.request.get("/api/setup/status");
    const status = await statusResponse.json();
    test.skip(!status.isComplete, "Setup not completed — run setup.spec.ts first");
  });

  test("logs in, reaches dashboard, and logs out", async ({ page }) => {
    // Navigate to login page
    await page.goto("/login");
    await expect(page.locator("#username")).toBeVisible();

    // Fill login form
    await page.locator("#username").fill(ADMIN_USERNAME);
    await page.locator("#password").fill(ADMIN_PASSWORD);

    // Submit login
    await page.getByRole("button", { name: "Log In" }).click();

    // Verify redirect to dashboard
    await page.waitForURL("/");
    await expect(page.getByText("Dashboard")).toBeVisible();

    // Click logout button in the nav menu
    await page.getByRole("button", { name: "Logout" }).click();

    // Verify redirect to login page
    await page.waitForURL("**/login");
    await expect(page.locator("#username")).toBeVisible();
  });

  test("shows error for invalid credentials", async ({ page }) => {
    await page.goto("/login");

    await page.locator("#username").fill("wronguser");
    await page.locator("#password").fill("wrongpassword");
    await page.getByRole("button", { name: "Log In" }).click();

    // Verify error message appears
    await expect(page.locator(".alert-danger")).toBeVisible();
  });
});
