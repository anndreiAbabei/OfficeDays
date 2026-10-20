namespace OfficeDays.Security.RateLimiting;

public sealed record SensitiveRequestLimit(string Policy, string? AccountField = null);
