using OfficeDays.Features.Attendance.GetAttendance;
using OfficeDays.Features.Attendance.GetAttendance.Contracts;

namespace OfficeDays.UnitTests.Features.Attendance;

public sealed class GetAttendanceRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBothYearAndMonthAreNull()
    {
        // arrange
        var request = new GetAttendanceRequest
        {
            Year = null,
            Month = null
        };
        var sut = new GetAttendanceRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
    
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBothYearAndMonthAreValid()
    {
        // arrange
        var request = new GetAttendanceRequest
        {
            Year = 2025,
            Month = 12
        };
        var sut = new GetAttendanceRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
    
    [Fact]
    public void Should_ReturnValidationFailure_WhenYearIsLessThan1()
    {
        // arrange
        var request = new GetAttendanceRequest
        {
            Year = 0,
            Month = 12
        };
        var sut = new GetAttendanceRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
    }
    
    [Fact]
    public void Should_ReturnValidationFailure_WhenYearIsBiggerThan9999()
    {
        // arrange
        var request = new GetAttendanceRequest
        {
            Year = 10000,
            Month = 12
        };
        var sut = new GetAttendanceRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
    }
    
    [Fact]
    public void Should_ReturnValidationFailure_WhenYearIsValidButMonthIsNull()
    {
        // arrange
        var request = new GetAttendanceRequest
        {
            Year = 2015,
            Month = null
        };
        var sut = new GetAttendanceRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
    }
    
    [Fact]
    public void Should_ReturnValidationFailure_WhenMonthIsLessThan1()
    {
        // arrange
        var request = new GetAttendanceRequest
        {
            Year = 2015,
            Month = 0
        };
        var sut = new GetAttendanceRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
    }
    
    [Fact]
    public void Should_ReturnValidationFailure_WhenMonthIsBiggerThan12()
    {
        // arrange
        var request = new GetAttendanceRequest
        {
            Year = 2015,
            Month = 13
        };
        var sut = new GetAttendanceRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
    }
    
    [Fact]
    public void Should_ReturnValidationFailure_WhenMonthIsValidButYearIsNull()
    {
        // arrange
        var request = new GetAttendanceRequest
        {
            Year = null,
            Month = 11
        };
        var sut = new GetAttendanceRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
    }
}
