using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Tokens.CreateToken.Contracts;

public sealed record CreateTokenRequest : IRequest
{
    [FromBody]
    public CreateTokenRequestBody Body { get; init; } = default!;
}
