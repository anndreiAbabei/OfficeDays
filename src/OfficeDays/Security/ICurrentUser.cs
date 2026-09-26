namespace OfficeDays.Security;

public interface ICurrentUser
{
    Guid Id { get; }
}

public sealed class CurrentUser(IHttpContextAccessor contextAccessor) : ICurrentUser
{
    public Guid Id
    {
        get
        {
            var principal = contextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
                throw new InvalidOperationException("An authenticated user is required.");
            return principal.GetUserId();
        }
    }
}
