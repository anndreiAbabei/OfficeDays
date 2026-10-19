using Microsoft.AspNetCore.Builder;
using NSubstitute;
using OfficeDays.Features.BankHolidays;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.BankHolidays;

public sealed class BankHolidaysEndpointGroupTests
{
    [Fact]
    public async Task Should_MapGroup()
    {
        // arrange
        var ep = Substitute.For<IEndpointGroupBuilder>();
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var group = app.MapGroup("bank-holidays");
        ep.MapEndpoints<IBankHolidaysEndpoint>("bank-holidays").Returns(group);
        var sut = new BankHolidaysEndpointGroup();

        // act
        sut.MapGroup(ep);

        // assert
        ep.Received(1).MapEndpoints<IBankHolidaysEndpoint>("bank-holidays");
    }
}
