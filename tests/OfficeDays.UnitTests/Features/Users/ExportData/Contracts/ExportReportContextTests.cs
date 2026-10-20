using OfficeDays.Features.Users.ExportData.Contracts;

namespace OfficeDays.UnitTests.Features.Users.ExportData.Contracts;

public sealed class ExportReportContextTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        const int expectedCalculationVersion = 1;
        var expectedLocalDate = new DateOnly(2026, 10, 19);
        var expectedRegistrationLocalDate = new DateOnly(2026, 1, 1);

        // act
        var sut = new ExportReportContext(expectedCalculationVersion, expectedLocalDate, expectedRegistrationLocalDate);

        // assert
        Assert.Equal(expectedCalculationVersion, sut.CalculationVersion);
        Assert.Equal(expectedLocalDate, sut.LocalDate);
        Assert.Equal(expectedRegistrationLocalDate, sut.RegistrationLocalDate);
    }
}
