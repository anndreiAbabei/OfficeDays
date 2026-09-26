using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Tokens.RevokeToken.Contracts;

public sealed record RevokeTokenRequest : IRequest
{
    [FromRoute(Name = "id")]
    public Guid Id { get; init; }
}
