using System.Diagnostics.CodeAnalysis;

namespace OfficeDays.Domain;

public sealed class ApiToken
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global", Justification = "Can be set (uses EF functions)")]
    public required string Name { get; set; }
    [SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength", Justification = "Behind validator")]
    public string? Purpose { get; init; }
    public required string TokenHash { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public User User { get; init; } = null!;
}
