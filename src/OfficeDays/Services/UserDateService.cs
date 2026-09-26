using OfficeDays.Domain;

namespace OfficeDays.Services;

public interface IUserDateService
{
    DateOnly Today(User user);
    bool IsValidTimeZone(string id);
}

public sealed class UserDateService : IUserDateService
{
    private readonly TimeProvider _timeProvider;

    public UserDateService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public DateOnly Today(User user)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
        var local = TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), timeZone);
        
        return DateOnly.FromDateTime(local.DateTime);
    }

    public bool IsValidTimeZone(string id) =>
        IsValidTimeZone(id, TimeZoneInfo.FindSystemTimeZoneById);

    private static bool IsValidTimeZone(string id, Func<string, TimeZoneInfo> findTimeZone)
    {
        try
        {
            _ = findTimeZone(id);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}
