using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Routing;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Testing;

[ExcludeFromCodeCoverage]
public static class EndpointAssert
{
    extension(Assert)
    {
        public static void Endpoint<TRequest>(IEndpointRouteBuilder endpointBuilder, IRequestExecutor executor, [StringSyntax("Route")] string path, HttpMethod method)
            where TRequest : IRequest
        {
            var routes = endpointBuilder.DataSources.SelectMany(ds => ds.Endpoints).OfType<RouteEndpoint>();
            var expectedMethod = executor.GetType().GetMethod(nameof(executor.Execute))?.MakeGenericMethod(typeof(TRequest));
            var methodStr = method.ToString();
        
            Assert.Contains(routes, ds => ds.RoutePattern.RawText == path && 
                                          ds.Metadata.OfType<HttpMethodMetadata>().First().HttpMethods.Single().ToString().Equals(methodStr, StringComparison.OrdinalIgnoreCase) && 
                                          ds.Metadata.OfType<MethodInfo>().First() == expectedMethod);
        }
    }
}
