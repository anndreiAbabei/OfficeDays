using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Authentication.Login.Contracts;

public sealed record LoginRequest : IRequest
{
    [FromBody]
    public required LoginRequestBody Body { get; init; }
}
