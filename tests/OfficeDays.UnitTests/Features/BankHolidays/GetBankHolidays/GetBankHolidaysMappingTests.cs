using System.Diagnostics.CodeAnalysis;
using OfficeDays.Domain;
using OfficeDays.Features.BankHolidays.GetBankHolidays;

namespace OfficeDays.UnitTests.Features.BankHolidays.GetBankHolidays;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
public sealed class GetBankHolidaysMappingTests
{
    [Fact]
    public void Should_MapToViewModel()
    {
        // arrange
        var sut = new BankHoliday { Id = Guid.NewGuid(), HolidayJurisdiction = new HolidayJurisdiction { Id = 7, Code = "RO", Name = "Romania" }, Date = DateOnly.FromDateTime(DateTime.Now), Name = "New Year" };

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Equal(sut.Date, result.Date);
        Assert.Equal(sut.Name, result.Name);
        Assert.Equal(sut.HolidayJurisdiction.Code, result.CountryCode);
    }

    [Fact]
    public void Should_MapCollectionInSourceOrder()
    {
        // arrange
        var firstEntity = new BankHoliday { Id = Guid.NewGuid(), HolidayJurisdiction = new HolidayJurisdiction { Id = 7, Code = "RO", Name = "Romania" }, Date = DateOnly.FromDateTime(DateTime.Now), Name = "New Year" };
        var secondEntity = new BankHoliday { Id = Guid.NewGuid(), HolidayJurisdiction = new HolidayJurisdiction { Id = 7, Code = "RO", Name = "Romania" }, Date = new DateOnly(2026, 12, 25), Name = "Christmas" };
        BankHoliday[] sut = [firstEntity, secondEntity];

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Collection(result,
            first =>
            {
                Assert.Equal(firstEntity.Date, first.Date);
                Assert.Equal(firstEntity.Name, first.Name);
                Assert.Equal(firstEntity.HolidayJurisdiction.Code, first.CountryCode);
            },
            second =>
            {
                Assert.Equal(secondEntity.Date, second.Date);
                Assert.Equal(secondEntity.Name, second.Name);
                Assert.Equal(secondEntity.HolidayJurisdiction.Code, second.CountryCode);
            });
    }

    [Fact]
    public void Should_MapEmptyCollection()
    {
        // arrange
        BankHoliday[] sut = [];

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Empty(result);
    }
}
