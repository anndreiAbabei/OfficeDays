using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Common;
using OfficeDays.Services;

namespace OfficeDays.Features.Users;

public static class UserEndpoints
{
    private const string LogCategory = "OfficeDays.Features.Users";

    public static void Map(RouteGroupBuilder api) => api.MapPost("/users", CreateUser).AllowAnonymous();

    private static async Task<IResult> CreateUser([FromBody] CreateUserRequest request,
                                                  AppDbContext db,
                                                  IPasswordHasher<User> hasher,
                                                  TimeProvider timeProvider,
                                                  ILoggerFactory loggerFactory,
                                                  CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LogCategory);
        var validation = UserValidation.Validate(request);
        if (validation.HasValue)
            return ApiResults.Validation(validation.Value.Message, validation.Value.Key);

        var username = request.Username!.Trim();
        var normalized = ApiResults.NormalizeUsername(username);
        HolidayJurisdictionCodes.TryNormalize(request.CountryCode, out var countryCode);
        var jurisdiction = await db.HolidayJurisdictions.SingleOrDefaultAsync(
            x => x.Code == countryCode, cancellationToken);
        if (jurisdiction is null)
            return ApiResults.Validation("The selected holiday jurisdiction does not exist.", "countryCode");

        if (await db.Users.AnyAsync(x => x.NormalizedUsername == normalized, cancellationToken))
            return Results.Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Username already exists" });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
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
            logger.LogWarning("A duplicate username registration was attempted for {Username}", username);
            return Results.Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Username already exists" });
        }

        logger.LogInformation("Created normal user {Username} with user ID {UserId}", user.Username, user.Id);
        return Results.Created($"/api/users/{user.Id}", user.ToViewModel());
    }
}
