using Microsoft.AspNetCore.Builder;
using NSubstitute;
using OfficeDays.Features.HolidayJurisdictions;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions;

public sealed class HolidayJurisdictionsEndpointGroupTests
{
    [Fact]
    public async Task Should_MapGroup()
    {
        // arrange
        var ep = Substitute.For<IEndpointGroupBuilder>();
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var group = app.MapGroup("holiday-jurisdictions");
        ep.MapEndpoints<IHolidayJurisdictionsEndpoint>("holiday-jurisdictions").Returns(group);
        var sut = new HolidayJurisdictionsEndpointGroup();

        // act
        sut.MapGroup(ep);

        // assert
        ep.Received(1).MapEndpoints<IHolidayJurisdictionsEndpoint>("holiday-jurisdictions");
    }
}
