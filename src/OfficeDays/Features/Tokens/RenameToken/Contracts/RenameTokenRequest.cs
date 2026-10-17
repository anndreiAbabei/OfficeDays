using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Tokens.RenameToken.Contracts;

public sealed record RenameTokenRequest : IRequest
{
    [FromRoute(Name = "id")]
    public Guid Id { get; init; }

    [FromBody]
    public required RenameTokenRequestBody Body { get; init; }
}
