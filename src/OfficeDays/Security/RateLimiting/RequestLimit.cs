 namespace OfficeDays.Security.RateLimiting;

public sealed record RequestLimit(int Permits, int WindowSeconds);
