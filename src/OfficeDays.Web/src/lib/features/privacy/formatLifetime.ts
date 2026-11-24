export function formatLifetime(seconds: number): string {
  for (const [unit, divisor] of [['day', 86400], ['hour', 3600], ['minute', 60]] as const) {
    if (seconds >= divisor && seconds % divisor === 0) {
      const value = seconds / divisor;
      return `${value} ${unit}${value === 1 ? '' : 's'}`;
    }
  }
  return `${seconds} second${seconds === 1 ? '' : 's'}`;
}
