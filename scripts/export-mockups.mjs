import { chromium } from "playwright";
import path from "path";
import { fileURLToPath } from "url";
import fs from "fs";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, "..");
const mockupsDir = path.join(repoRoot, "mockups");
const pngDir = path.join(mockupsDir, "png");
const htmlPath = path.join(mockupsDir, "studio.html");

const screens = [
  "home-default", "home-loading", "home-empty", "home-error",
  "search-empty", "search-loading", "search-results", "search-no-results",
  "detail-series", "detail-series-seasons", "detail-film", "detail-loading", "detail-not-found",
  "stats-default", "stats-loading",
  "sync-pending", "sync-synced", "sync-offline", "sync-syncing", "sync-loading", "sync-resolved", "sync-diagnostic",
];

fs.mkdirSync(pngDir, { recursive: true });

const htmlUrl = "file:///" + htmlPath.replace(/\\/g, "/");

const browser = await chromium.launch();
const context = await browser.newContext({
  viewport: { width: 430, height: 932 },
  deviceScaleFactor: 3,
});
const page = await context.newPage();
await page.goto(htmlUrl, { waitUntil: "networkidle" });
await page.waitForTimeout(500);

for (const screen of screens) {
  const ok = await page.evaluate((name) => window.CINETRACK_EXPORT_SCREEN(name), screen);
  if (!ok) {
    await browser.close();
    throw new Error(`Screen not found: ${screen}`);
  }
  await page.waitForTimeout(200);
  const target = page.locator(".export-stage .phone-frame.is-export-target");
  await target.waitFor({ state: "visible", timeout: 5000 });
  const out = path.join(pngDir, `${screen}.png`);
  await target.screenshot({ path: out });
  console.log(`Exported ${screen} -> ${out}`);
}

await browser.close();
