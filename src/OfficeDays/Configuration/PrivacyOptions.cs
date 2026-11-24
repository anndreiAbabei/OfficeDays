namespace OfficeDays.Configuration;

public sealed class PrivacyOptions
{
    public bool Enabled { get; init; }
    public string OperatorName { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string OperatorNotice { get; init; } = string.Empty;
}
