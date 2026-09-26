using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Common;
using OfficeDays.Features.Users.CreateUser.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Services;

namespace OfficeDays.Features.Users.CreateUser;

public sealed class CreateUserHandler(AppDbContext db,
    IPasswordHasher<User> hasher,
    TimeProvider timeProvider,
    ILogger<CreateUserHandler> logger) : IRequestHandler<CreateUserRequest>
{
    public async ValueTask<IResult> Handle(CreateUserRequest input, CancellationToken cancellationToken)
    {
        var request = input.Body;

        var username = request.Username!.Trim();
        var normalized = ApiResults.NormalizeUsername(username);
        HolidayJurisdictionCodes.TryNormalize(request.CountryCode, out var countryCode);
        var jurisdiction = await db.HolidayJurisdictions
                                   .SingleOrDefaultAsync(x => x.Code == countryCode, cancellationToken);
        if (jurisdiction is null)
            return ApiResults.Validation("The selected holiday jurisdiction does not exist.", "countryCode");

        if (await db.Users.AnyAsync(x => x.NormalizedUsername == normalized, cancellationToken))
            return Results.Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Username already exists" });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = UserFields.NormalizeEmail(request.Email),
            RequiredOfficePercentage = request.RequiredOfficePercentage,
            NormalizedUsername = normalized,
            PasswordHash = string.Empty,
            TimeZoneId = request.TimeZoneId!,
            HolidayJurisdictionId = jurisdiction.Id,
            HolidayJurisdiction = jurisdiction,
            IsAdmin = false,
            CreatedAt = timeProvider.GetUtcNow()
        };
        user.PasswordHash = hasher.HashPassword(user, request.Password!);
        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (ApiResults.IsUniqueViolation(exception))
        {
            logger.LogDuplicateUsername(username);
            return Results.Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Username already exists" });
        }

        logger.LogUserCreated(user.Username, user.Id);
        return Results.Created($"/api/users/{user.Id}", user.ToViewModel());
    }
}
