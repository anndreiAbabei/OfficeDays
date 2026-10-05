namespace OfficeDays.Features.Ui.GetIndex;

public sealed class GetIndexEndpoint(UiContent content) : IUiEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods("/index.html", ["GET", "HEAD"], Index);
        endpoints.MapFallback(Index).WithMetadata(new HttpMethodMetadata(["GET", "HEAD"]));
        return;

        IResult Index(HttpContext context)
        {
            // Unknown API/asset URLs must not become successful HTML responses.
            if (context.Request.Path.StartsWithSegments("/api") ||
                context.Request.Path.StartsWithSegments("/health") ||
                context.Request.Path.StartsWithSegments("/_app") ||
                context.Request.Path.StartsWithSegments("/assets"))
                return Results.NotFound();
            context.Response.Headers.CacheControl = "no-store";
            return Results.Bytes(content.Index, "text/html; charset=utf-8");
        }
    }
}
