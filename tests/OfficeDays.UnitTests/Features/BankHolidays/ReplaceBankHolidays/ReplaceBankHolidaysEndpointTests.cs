using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;
using OfficeDays.Features.BankHolidays.ReplaceBankHolidays;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.BankHolidays.ReplaceBankHolidays;

public sealed class ReplaceBankHolidaysEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new ReplaceBankHolidaysEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<ReplaceBankHolidaysRequest>(epb, executor, "{countryCode}/{year:int}", HttpMethod.Put);
        var endpoint = Assert.Single(epb.DataSources.SelectMany(source => source.Endpoints));
        Assert.Equal("AdminOnly", Assert.Single(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>()).Policy);
    }
}
