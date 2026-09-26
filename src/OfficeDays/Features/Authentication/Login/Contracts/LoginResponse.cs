namespace OfficeDays.Features.Authentication.Login.Contracts;

public sealed record LoginResponse(Guid Id, 
                                   string Username, 
                                   string TimeZoneId,
                                   string CountryCode, 
                                   string CountryName, 
                                   bool IsAdmin, 
                                   string? Email, 
                                   int RequiredOfficePercentage);
