using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Tokens.CreateToken.Contracts;

public sealed record CreateTokenRequest : IRequest
{
    [FromBody]
    public required CreateTokenRequestBody Body { get; init; }
}
