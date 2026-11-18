namespace OfficeDays.Domain;

public sealed class UserPreferences
{
    public Appearance Appearance { get; set; } = Appearance.System;

    public DayOfWeek? TeamDay { get; set; }
}
