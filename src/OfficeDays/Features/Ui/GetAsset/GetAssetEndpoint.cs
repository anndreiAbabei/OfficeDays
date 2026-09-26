namespace OfficeDays.Features.Ui.GetAsset;

public sealed class GetAssetEndpoint(UiContent content) : IUiEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        foreach (var asset in content.Assets)
        {
            // Snapshot URLs and bytes together so a URL always serves identical content.
            endpoints.MapGet(asset.Path, (HttpContext context) =>
            {
                context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                return Results.Bytes(asset.Bytes, asset.ContentType);
            });
            endpoints.MapGet($"/{asset.Name}", (HttpContext context) =>
            {
                context.Response.Headers.CacheControl = "no-store";
                return Results.Bytes(asset.Bytes, asset.ContentType);
            });
        }
    }
}
