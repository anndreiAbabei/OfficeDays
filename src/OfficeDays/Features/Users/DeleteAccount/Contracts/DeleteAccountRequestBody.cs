namespace OfficeDays.Features.Users.DeleteAccount.Contracts;

public sealed record DeleteAccountRequestBody
{
    public required string Password { get; init; }
}
