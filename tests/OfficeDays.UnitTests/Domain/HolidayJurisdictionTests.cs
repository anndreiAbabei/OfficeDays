using OfficeDays.Domain;

namespace OfficeDays.UnitTests.Domain;

public sealed class HolidayJurisdictionTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        const int expectedId = 1234;
        const string expectedCode = "test";
        const string expectedName = "Test Jur";
        var expectedBankHolidays = new List<BankHoliday>();
        var expectedUsers = new List<User>();
        
        // act
        var sut = new HolidayJurisdiction
        {
            Id = expectedId,
            Code = expectedCode,
            Name = expectedName,
            BankHolidays = expectedBankHolidays,
            Users = expectedUsers,
        };
        
        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedCode, sut.Code);
        Assert.Equal(expectedName, sut.Name);
        Assert.Same(expectedBankHolidays, sut.BankHolidays);
        Assert.Same(expectedUsers, sut.Users);
    }
}
