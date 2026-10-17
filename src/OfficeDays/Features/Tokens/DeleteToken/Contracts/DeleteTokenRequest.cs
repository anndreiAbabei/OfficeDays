using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Tokens.DeleteToken.Contracts;

public sealed record DeleteTokenRequest : IRequest
{
    [FromRoute(Name = "id")]
    public Guid Id { get; init; }
}
