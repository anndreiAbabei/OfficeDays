using OfficeDays.Extensions;

namespace OfficeDays.Security;

public interface ICurrentUser
{
    Guid Id { get; }
}

public sealed class CurrentUser(IHttpContextAccessor contextAccessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
    
    public Guid Id
    {
        get
        {
            var principal = _contextAccessor.HttpContext?.User;
            
            return principal?.Identity?.IsAuthenticated != true 
                       ? throw new InvalidOperationException("An authenticated user is required.") 
                       : principal.GetUserId();
        }
    }
}
