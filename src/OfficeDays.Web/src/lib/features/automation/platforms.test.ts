import { expect, it } from 'vitest';
import { detectPlatform } from './platforms';

it.each([
  ['iPhone', 5, 'ios'],
  ['iPad', 5, 'ios'],
  ['Macintosh; Intel Mac OS X', 5, 'ios'],
  ['Macintosh; Intel Mac OS X', 0, 'macos'],
  ['Linux; Android 14', 5, 'android'],
  ['Windows NT 10.0', 0, 'windows'],
  ['X11; Linux x86_64', 0, 'linux'],
  ['Unknown', 0, null]
] as const)('Should_DetectPlatform_WhenBrowserReports%sWith%iTouchPoints', (userAgent, maxTouchPoints, expected): void => {
  // Act
  const platform = detectPlatform({ userAgent, maxTouchPoints });

  // Assert
  expect(platform).toBe(expected);
});
