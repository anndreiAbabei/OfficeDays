import { expect, test } from '@playwright/test';

for (const available of [true, false]) {
  test(`Should_ShowInstalledVersion_WhenUpdateIs${available ? 'Available' : 'Unavailable'}`, async ({ page }) => {
    // Arrange
    const version = '2.4.0+local';
    const latestVersion = '2.5.0';
    const expectedTooltip = `${latestVersion} version is available`;
    await page.route('**/api/version', route => route.fulfill({
      json: { version, latestVersion, newVersionAvailable: available, author: 'Andrei', gitHubRepo: 'https://github.com/anndreiAbabei/OfficeDays' }
    }));

    // Act
    await page.goto('/login');

    // Assert
    const footer = page.getByRole('contentinfo');
    await expect(footer).toContainText(`v${version}`);
    const update = footer.getByTitle(expectedTooltip);
    if (available) {
      await expect(update).toHaveText(`v${version}`);
      await expect(update).toHaveCSS('font-weight', '700');
    } else {
      await expect(update).toHaveCount(0);
    }
  });
}
