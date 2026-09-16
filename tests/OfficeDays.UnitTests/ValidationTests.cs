using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Domain;
using OfficeDays.Features.Attendance;
using OfficeDays.Features.Authentication;
using OfficeDays.Features.BankHolidays;
using OfficeDays.Features.Common;
using OfficeDays.Features.HolidayJurisdictions;
using OfficeDays.Features.Tokens;
using OfficeDays.Features.Users;
using OfficeDays.Features.Vacations;
using OfficeDays.Services;

namespace OfficeDays.UnitTests;

public sealed class ValidationTests
{
    [Fact]
    public void Known_timezone_is_valid() =>
        Assert.True(UserDateService.IsValidTimeZone("Europe/Bucharest"));

    [Fact]
    public void Missing_timezone_is_invalid() =>
        Assert.False(UserDateService.IsValidTimeZone("Not/A-Time-Zone"));

    [Fact]
    public void Timezone_not_found_exception_is_handled() =>
        Assert.False(UserDateService.IsValidTimeZone("missing", _ => throw new TimeZoneNotFoundException()));

    [Fact]
    public void Invalid_timezone_exception_is_handled() =>
        Assert.False(UserDateService.IsValidTimeZone("invalid", _ => throw new InvalidTimeZoneException()));

    [Fact]
    public void Validation_result_contains_expected_problem_details()
    {
        var result = ApiResults.Validation("Bad value", "field");

        Assert.Equal(StatusCodes.Status400BadRequest, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        var value = Assert.IsAssignableFrom<IValueHttpResult>(result).Value;
        var problem = Assert.IsType<HttpValidationProblemDetails>(value);
        Assert.Equal("Bad value", Assert.Single(problem.Errors["field"]));
    }

    [Fact]
    public void Sqlite_constraint_error_is_a_unique_violation()
    {
        var exception = new DbUpdateException("conflict", new SqliteException("constraint", 19));
        Assert.True(ApiResults.IsUniqueViolation(exception));
    }

    [Fact]
    public void Other_database_error_is_not_a_unique_violation()
    {
        var exception = new DbUpdateException("failure", new InvalidOperationException());
        Assert.False(ApiResults.IsUniqueViolation(exception));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(2026, null)]
    [InlineData(null, 9)]
    [InlineData(0, 9)]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    public void Invalid_month_parameters_return_false(int? year, int? month)
    {
        Assert.False(ApiResults.TryMonth(year, month, out _, out var error));
        Assert.Equal(StatusCodes.Status400BadRequest, Assert.IsAssignableFrom<IStatusCodeHttpResult>(error).StatusCode);
    }

    [Fact]
    public void Valid_month_parameters_return_period()
    {
        Assert.True(ApiResults.TryMonth(2026, 9, out var period, out _));
        Assert.Equal(new DateOnly(2026, 9, 1), period.Start);
        Assert.Equal(new DateOnly(2026, 9, 30), period.End);
    }

    [Theory]
    [MemberData(nameof(InvalidUsers))]
    public void User_validation_rejects_invalid_requests(CreateUserRequest request, string expectedKey)
    {
        var error = UserValidation.Validate(request);
        Assert.NotNull(error);
        Assert.Equal(expectedKey, error.Value.Key);
    }

    [Fact]
    public void User_validation_accepts_valid_request() =>
        Assert.Null(UserValidation.Validate(new CreateUserRequest("andrew", "long-password!", "Europe/Bucharest", "RO")));

    public static TheoryData<CreateUserRequest, string> InvalidUsers => new()
    {
        { new CreateUserRequest(null, "long-password!", "Europe/Bucharest", "RO"), "username" },
        { new CreateUserRequest("ab", "long-password!", "Europe/Bucharest", "RO"), "username" },
        { new CreateUserRequest(new string('u', 101), "long-password!", "Europe/Bucharest", "RO"), "username" },
        { new CreateUserRequest("andrew", "short", "Europe/Bucharest", "RO"), "password" },
        { new CreateUserRequest("andrew", "long-password!", "Invalid/Zone", "RO"), "timeZoneId" },
        { new CreateUserRequest("andrew", "long-password!", "Europe/Bucharest", "invalid"), "countryCode" }
    };

    [Theory]
    [InlineData("ro", "RO")]
    [InlineData("UK", "GB")]
    [InlineData("uk-nir", "GB-NIR")]
    [InlineData("GB-SCT", "GB-SCT")]
    public void Holiday_jurisdiction_codes_are_normalized(string input, string expected)
    {
        Assert.True(HolidayJurisdictionCodes.TryNormalize(input, out var actual));
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ZZ")]
    [InlineData("GB-TOOLONG")]
    [InlineData("GB_ENG")]
    public void Invalid_holiday_jurisdiction_codes_are_rejected(string? input) =>
        Assert.False(HolidayJurisdictionCodes.TryNormalize(input, out _));

    [Fact]
    public void Holiday_jurisdiction_validation_checks_code_and_name()
    {
        Assert.Equal("code", HolidayJurisdictionValidation.Validate("invalid", "Name")?.Key);
        Assert.Equal("name", HolidayJurisdictionValidation.Validate("RO", " ")?.Key);
        Assert.Equal("name", HolidayJurisdictionValidation.ValidateName(new string('x', 101))?.Key);
        Assert.Null(HolidayJurisdictionValidation.Validate("RO", "Romania"));
    }

    [Fact]
    public void Authentication_validation_requires_both_values()
    {
        Assert.False(AuthenticationValidation.HasCredentials(new LoginRequest(null, "password")));
        Assert.False(AuthenticationValidation.HasCredentials(new LoginRequest("user", null)));
        Assert.True(AuthenticationValidation.HasCredentials(new LoginRequest("user", "password")));
    }

    [Fact]
    public void Attendance_validation_requires_iso_date()
    {
        Assert.False(AttendanceValidation.TryParseDate("09/15/2026", out _));
        Assert.True(AttendanceValidation.TryParseDate("2026-09-15", out var date));
        Assert.Equal(new DateOnly(2026, 9, 15), date);
    }

    [Fact]
    public void Token_validation_checks_name()
    {
        Assert.NotNull(TokenValidation.ValidateName(null));
        Assert.NotNull(TokenValidation.ValidateName(new string('x', 101)));
        Assert.Null(TokenValidation.ValidateName("Phone"));
    }

    [Fact]
    public void Vacation_validation_checks_date_order()
    {
        Assert.NotNull(VacationValidation.Validate(new CreateVacationRequest(new(2026, 9, 2), new(2026, 9, 1))));
        Assert.Null(VacationValidation.Validate(new CreateVacationRequest(new(2026, 9, 1), new(2026, 9, 2))));
    }

    [Theory]
    [MemberData(nameof(InvalidHolidayCollections))]
    public void Bank_holiday_validation_rejects_invalid_collections(
        int year, List<BankHolidayRequest>? holidays, string expectedKey)
    {
        var error = BankHolidayValidation.Validate(year, holidays);
        Assert.NotNull(error);
        Assert.Equal(expectedKey, error.Value.Key);
    }

    [Fact]
    public void Bank_holiday_validation_accepts_valid_collection() =>
        Assert.Null(BankHolidayValidation.Validate(2026,
            [new BankHolidayRequest(new DateOnly(2026, 1, 1), "New Year")]));

    public static TheoryData<int, List<BankHolidayRequest>?, string> InvalidHolidayCollections => new()
    {
        { 0, [], "year" },
        { 2026, null, "request" },
        { 2026, [new BankHolidayRequest(new DateOnly(2025, 1, 1), "Wrong year")], "date" },
        { 2026, [new BankHolidayRequest(new DateOnly(2026, 1, 1), " ")], "name" },
        { 2026, [new BankHolidayRequest(new DateOnly(2026, 1, 1), new string('x', 151))], "name" },
        { 2026, [new BankHolidayRequest(new DateOnly(2026, 1, 1), "One"),
                 new BankHolidayRequest(new DateOnly(2026, 1, 1), "Duplicate")], "date" }
    };
}
