using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Users.DeleteAccount.Contracts;

public sealed record DeleteAccountRequest : IRequest
{
    [FromBody]
    public required DeleteAccountRequestBody Body { get; init; }
}
