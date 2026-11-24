import { expect, it } from 'vitest';
import { formatLifetime } from './formatLifetime';

it.each([
  [1728000, '20 days'], [86400, '1 day'], [43200, '12 hours'],
  [60, '1 minute'], [121, '121 seconds']
] as const)('Should_DescribeConfiguredLifetime_WhenDurationIs%sSeconds', (seconds, expected): void => {
  // Act
  const actual = formatLifetime(seconds);

  // Assert
  expect(actual).toBe(expected);
});
