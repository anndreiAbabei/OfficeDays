using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using OfficeDays.Features.Users.GetRequirements;
using OfficeDays.Features.Users.GetRequirements.Contracts;
using OfficeDays.Features.Users.CreateRequirement;
using OfficeDays.Features.Users.CreateRequirement.Contracts;
using OfficeDays.Features.Users.UpdateRequirement;
using OfficeDays.Features.Users.UpdateRequirement.Contracts;
using OfficeDays.Features.Users.DeleteRequirement;
using OfficeDays.Features.Users.DeleteRequirement.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Requirements;

public sealed class RequirementEndpointTests
{
    [Fact]
    public void List_maps_authenticated_get() => Check<GetRequirementsRequest>(executor => new GetRequirementsEndpoint(executor), "me/requirements", HttpMethod.Get);

    [Fact]
    public void Create_maps_authenticated_post() => Check<CreateRequirementRequest>(executor => new CreateRequirementEndpoint(executor), "me/requirements", HttpMethod.Post);

    [Fact]
    public void Edit_maps_authenticated_put() => Check<UpdateRequirementRequest>(executor => new UpdateRequirementEndpoint(executor), "me/requirements/{id:guid}", HttpMethod.Put);

    [Fact]
    public void Delete_maps_authenticated_delete() => Check<DeleteRequirementRequest>(executor => new DeleteRequirementEndpoint(executor), "me/requirements/{id:guid}", HttpMethod.Delete);

    private static void Check<T>(Func<IRequestExecutor, IEndpoint> create, string path, HttpMethod method) where T : IRequest
    {
        var executor = new RequestExecutor();
        var routes = Substitute.For<IEndpointRouteBuilder>();
        routes.DataSources.Returns([]);
        create(executor).MapEndpoint(routes);
        Assert.Endpoint<T>(routes, executor, path, method);
        var endpoint = Assert.Single(routes.DataSources.SelectMany(source => source.Endpoints));
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAuthorizeData>());
    }
}
