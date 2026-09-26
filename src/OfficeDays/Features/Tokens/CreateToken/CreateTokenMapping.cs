using OfficeDays.Features.Tokens.CreateToken.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Tokens.CreateToken;

public static class CreateTokenMapping
{

    public static CreateTokenResponse ToCreatedViewModel(this ApiToken token, string rawToken) => new CreateTokenResponse(token.Id, token.Name, rawToken, token.CreatedAt);
}
