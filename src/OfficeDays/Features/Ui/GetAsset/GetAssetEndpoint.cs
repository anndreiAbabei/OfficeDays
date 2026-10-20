namespace OfficeDays.Features.Ui.GetAsset;

public sealed class GetAssetEndpoint(UiContent content) : IUiEndpoint
{
    private readonly UiContent _content = content;

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        foreach (var asset in _content.Assets)
        {
            endpoints.MapMethods(asset.Path, ["GET", "HEAD"], (HttpContext context) =>
            {
                context.Response.Headers.CacheControl = asset.Immutable
                    ? "public, max-age=31536000, immutable" : "no-store";
                return Results.Bytes(asset.Bytes, asset.ContentType);
            });
        }
    }
}
