import { test, expect } from "@playwright/test";

test.describe("StatusMessage component", () => {
  const preview = ".showcase-preview-area";

  test.beforeEach(async ({ page }) => {
    await page.goto("/components-showcase/status-message");
    await page.waitForLoadState("networkidle");
  });

  test("renders default 404 preset with icon, title, message, and button", async ({
    page,
  }) => {
    const area = page.locator(preview);
    await expect(area.locator("i.bi-question-circle")).toBeVisible();
    await expect(area.locator("h2")).toHaveText("Not Found");
    await expect(area.locator("p")).toHaveText(
      "The page you're looking for doesn't exist."
    );
    await expect(area.locator("a.btn-primary")).toHaveText("Back to Dashboard");
  });

  test("switching preset to 401 updates icon, title, and message", async ({
    page,
  }) => {
    await page.locator("select").first().selectOption("401");
    const area = page.locator(preview);
    await expect(area.locator("i.bi-box-arrow-in-right")).toBeVisible();
    await expect(area.locator("h2")).toHaveText("Unauthorized");
    await expect(area.locator("p")).toHaveText(
      "You need to log in to access this page."
    );
    await expect(area.locator("a.btn-primary")).toHaveText("Go to Login");
  });

  test("switching preset to 403 updates icon, title, and message", async ({
    page,
  }) => {
    await page.locator("select").first().selectOption("403");
    const area = page.locator(preview);
    await expect(area.locator("i.bi-shield-lock")).toBeVisible();
    await expect(area.locator("h2")).toHaveText("Access Denied");
    await expect(area.locator("p")).toHaveText(
      "You don't have permission to view this page."
    );
    await expect(area.locator("a.btn-primary")).toHaveText("Back to Dashboard");
  });

  test("unchecking show button hides the action button", async ({ page }) => {
    await page.locator("#showButton").uncheck();
    const area = page.locator(preview);
    await expect(area.locator("a.btn-primary")).toHaveCount(0);
    await expect(area.locator("h2")).toBeVisible();
  });

  test("re-checking show button restores the action button", async ({
    page,
  }) => {
    await page.locator("#showButton").uncheck();
    await expect(page.locator(`${preview} a.btn-primary`)).toHaveCount(0);
    await page.locator("#showButton").check();
    await expect(page.locator(`${preview} a.btn-primary`)).toBeVisible();
  });

  test("custom preset shows icon dropdown, title, and message inputs", async ({
    page,
  }) => {
    await page.locator("select").first().selectOption("custom");
    const iconSelect = page.locator("select.form-select-sm").nth(1);
    await expect(iconSelect).toBeVisible();
    await expect(
      page.locator("input.form-control-sm").first()
    ).toBeVisible();
    await expect(page.locator("input.form-control-sm").nth(1)).toBeVisible();
  });

  test("custom preset updates title and message from inputs", async ({
    page,
  }) => {
    await page.locator("select").first().selectOption("custom");
    const area = page.locator(preview);

    const titleInput = page.locator("input.form-control-sm").first();
    const messageInput = page.locator("input.form-control-sm").nth(1);

    await titleInput.fill("Maintenance");
    await titleInput.press("Tab");
    await expect(area.locator("h2")).toHaveText("Maintenance");

    await messageInput.fill("The system is under maintenance.");
    await messageInput.press("Tab");
    await expect(area.locator("p")).toHaveText(
      "The system is under maintenance."
    );
  });

  test("custom preset icon dropdown changes the displayed icon", async ({
    page,
  }) => {
    await page.locator("select").first().selectOption("custom");
    const iconSelect = page.locator("select.form-select-sm").nth(1);
    await iconSelect.selectOption("gear");
    const area = page.locator(preview);
    await expect(area.locator("i.bi-gear")).toBeVisible();
  });

  test("code block is rendered", async ({ page }) => {
    const codeBlock = page.locator("pre code");
    await expect(codeBlock).toBeVisible();
  });
});
