using OfficeDays.Domain;

namespace OfficeDays.UnitTests.Domain;

public sealed class BankHolidayTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedDate = DateOnly.FromDateTime(DateTime.Now);
        const string expectedName = "test";
        const int expectedHolidayJurisdictionId = 1234;
        var expectedHolidayJurisdiction = new HolidayJurisdiction
        {
            Code = null!,
            Name = null!
        };
        
        // act
        var sut = new BankHoliday
        {
            Id = expectedId,
            Date = expectedDate,
            Name = expectedName,
            HolidayJurisdictionId = expectedHolidayJurisdictionId,
            HolidayJurisdiction = expectedHolidayJurisdiction,
        };

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedDate, sut.Date);
        Assert.Equal(expectedName, sut.Name);
        Assert.Equal(expectedHolidayJurisdictionId, sut.HolidayJurisdictionId);
        Assert.Equal(expectedHolidayJurisdiction, sut.HolidayJurisdiction);
    }
}
