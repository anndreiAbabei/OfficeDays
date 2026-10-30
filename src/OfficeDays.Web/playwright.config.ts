import { defineConfig, devices } from '@playwright/test';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { randomUUID } from 'node:crypto';
const testRoot = join(tmpdir(), `officedays-e2e-${randomUUID()}`);
export default defineConfig({
  testDir: './e2e',
  reporter: [['list'], ['json', { outputFile: 'test-results/report.json' }]],
  fullyParallel: false,
  workers: 1,
  timeout: 30_000,
  use: { reducedMotion: 'reduce', baseURL: 'http://127.0.0.1:5187', trace: 'retain-on-failure', screenshot: 'only-on-failure' },
  projects: [
    { name: 'desktop-chromium', use: { ...devices['Desktop Chrome'], viewport: { width: 1440, height: 1000 } } },
    { name: 'iphone-webkit', use: { ...devices['iPhone 13'] } },
    { name: 'android-chromium', use: { ...devices['Pixel 7'] } }
  ],
  webServer: {
    command: 'dotnet run --project ../OfficeDays --no-launch-profile --urls http://127.0.0.1:5187',
    url: 'http://127.0.0.1:5187/health/ready',
    reuseExistingServer: false,
    timeout: 120_000,
    env: {
      ASPNETCORE_ENVIRONMENT: 'Development',
      ConnectionStrings__Default: `Data Source=${testRoot}.db`,
      DataProtection__KeysPath: `${testRoot}-keys`,
      BootstrapAdmin__Password: 'BrowserTestsOnly!123',
      BootstrapAdmin__TimeZoneId: 'UTC',
      BootstrapAdmin__CountryCode: 'RO',
      BootstrapAdmin__CountryName: 'Romania',
      Logging__LogLevel__Default: 'Warning',
      // The suite creates many accounts from one source; HTTP integration tests exercise thresholds.
      RateLimiting__Registration__Permits: '100',
      RateLimiting__Login__Permits: '100',
      RateLimiting__LoginAccount__Permits: '100',
      RateLimiting__Source__Permits: '10000',
      // Guest setup and session checks share the anonymous API bucket across browser projects.
      RateLimiting__Api__Permits: '10000'
    }
  }
});
