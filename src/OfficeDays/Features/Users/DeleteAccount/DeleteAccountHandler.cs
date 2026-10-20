using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Users.DeleteAccount.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Users.DeleteAccount;

public sealed class DeleteAccountHandler : IRequestHandler<DeleteAccountRequest>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<DeleteAccountHandler> _logger;
    
    public DeleteAccountHandler(AppDbContext db,
                                ICurrentUser currentUser,
                                IPasswordHasher<User> hasher,
                                IHttpContextAccessor contextAccessor,
                                ILogger<DeleteAccountHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _hasher = hasher;
        _contextAccessor = contextAccessor;
        _logger = logger;
    }

    public async ValueTask<IResult> Handle(DeleteAccountRequest input, CancellationToken cancellationToken)
    {
        var context = _contextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request.");
       
        // SQLite's immediate transaction serializes the last-admin check with deletion.
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == _currentUser.Id, cancellationToken);
        
        if (user is null)
        {
            _logger.LogUserNotFound(_currentUser.Id);
            return Results.Unauthorized();
        }

        var passCheckResult = _hasher.VerifyHashedPassword(user, user.PasswordHash, input.Body.Password);
        if (passCheckResult == PasswordVerificationResult.Failed)
        {
            _logger.LogPasswordRejected(user.Id);
            
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, 
                                   title: "Incorrect password", 
                                   detail: "Your current password is incorrect. Your account has not been deleted.");
        }

        if (user.IsAdmin && !await _db.Users.AnyAsync(x => x.IsAdmin && x.Id != user.Id, cancellationToken))
        {
            _logger.LogLastAdministrator(user.Id);
            
            return Results.Problem(statusCode: StatusCodes.Status409Conflict, 
                                   title: "Last administrator",
                                   detail: "You are the last administrator. Ask the server operator to assign another administrator before deleting your account.");
        }

        // All personal dependent records have database cascade-delete constraints.
        _db.Users.Remove(user);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        return Results.NoContent();
    }
}
