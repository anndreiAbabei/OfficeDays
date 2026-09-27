namespace OfficeDays.Services;

public interface IAttendanceCalculator
{
    AttendanceStatus Calculate(CalculationPeriod period,
        IEnumerable<DateOnly> bankHolidays,
        IEnumerable<(DateOnly From, DateOnly To)> vacations,
        IEnumerable<DateOnly> officeDates,
        int requiredOfficePercentage = 50);
}
