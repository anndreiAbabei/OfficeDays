using OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;
using OfficeDays.Features.BankHolidays.ReplaceBankHolidays;

namespace OfficeDays.UnitTests.Features.BankHolidays.ReplaceBankHolidays;

public sealed class BankHolidayItemValidatorTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("Holiday", true)]
    public void Should_ValidateName(string? name, bool expectedIsValid)
    {
        // arrange
        var request = new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), name!);
        var sut = new BankHolidayItemValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.Equal(expectedIsValid, result.IsValid);
    }

    [Fact]
    public void Should_AcceptMaximumNameLength()
    {
        // arrange
        var request = new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), new string('x', 150));
        var sut = new BankHolidayItemValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_RejectNameBeyondMaximumLength()
    {
        // arrange
        var request = new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), new string('x', 151));
        var sut = new BankHolidayItemValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }
}
