using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Extensions;
using OfficeDays.Features.Users.CreateUser.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Services;

namespace OfficeDays.Features.Users.CreateUser;

public sealed class CreateUserHandler : IRequestHandler<CreateUserRequest>
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _hasher;
    private readonly TimeProvider _timeProvider;
    private readonly IUserService _userService;
    private readonly ILogger<CreateUserHandler> _logger;
    
    public CreateUserHandler(AppDbContext dbContext,
                             IPasswordHasher<User> hasher,
                             TimeProvider timeProvider,
                             IUserService userService,
                             ILogger<CreateUserHandler> logger)
    {
        _dbContext = dbContext;
        _hasher = hasher;
        _timeProvider = timeProvider;
        _userService = userService;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(CreateUserRequest input, CancellationToken cancellationToken)
    {
        var request = input.Body;
        var username = request.Username.Trim();
        var normalized = _userService.NormalizeUsername(username);
        HolidayJurisdictionCodes.TryNormalize(request.CountryCode, out var countryCode);
        var jurisdiction = await _dbContext.HolidayJurisdictions
                                   .SingleOrDefaultAsync(x => x.Code == countryCode, cancellationToken);
        if (jurisdiction is null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["countryCode"] = ["The selected holiday jurisdiction does not exist."] });
                
        if (await _dbContext.Users.AnyAsync(x => x.NormalizedUsername == normalized, cancellationToken))
        {
            _logger.LogDuplicateUsername(username);
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict, 
                Title = "Username already exists"
            };
            return Results.Conflict(problem);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = _userService.NormalizeEmail(request.Email),
            RequiredOfficePercentage = request.RequiredOfficePercentage.GetValueOrDefault(50),
            NormalizedUsername = normalized,
            PasswordHash = string.Empty,
            TimeZoneId = request.TimeZoneId,
            HolidayJurisdictionId = jurisdiction.Id,
            HolidayJurisdiction = jurisdiction,
            IsAdmin = false,
            CreatedAt = _timeProvider.GetUtcNow()
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password!);
        
        await _dbContext.Users.AddAsync(user, cancellationToken);
        
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) 
            when (exception.IsUniqueViolation())
        {
            _logger.LogDuplicateUsername(username);
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict, 
                Title = "Username already exists"
            };
            
            return Results.Conflict(problem);
        }

        _logger.LogUserCreated(user.Username, user.Id);
        
        return Results.Created($"/api/users/{user.Id}", user.ToViewModel());
    }
}
