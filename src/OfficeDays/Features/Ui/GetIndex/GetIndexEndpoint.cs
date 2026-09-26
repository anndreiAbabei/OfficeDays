namespace OfficeDays.Features.Ui.GetIndex;

public sealed class GetIndexEndpoint(UiContent content) : IUiEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        IResult Index(HttpContext context)
        {
            context.Response.Headers.CacheControl = "no-store";
            return Results.Bytes(content.Index, "text/html; charset=utf-8");
        }

        endpoints.MapGet("/index.html", Index);
        endpoints.MapFallback(Index).WithMetadata(new HttpMethodMetadata(["GET", "HEAD"]));
    }
}
