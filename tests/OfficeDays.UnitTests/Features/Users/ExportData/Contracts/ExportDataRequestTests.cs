using OfficeDays.Features.Users.ExportData.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Users.ExportData.Contracts;

public sealed class ExportDataRequestTests
{
    [Fact]
    public void Should_CreateRequest()
    {
        // arrange

        // act
        var sut = new ExportDataRequest();

        // assert
        Assert.IsType<IRequest>(sut, exactMatch: false);
    }
}
