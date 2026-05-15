using System.Net;
using System.Text;
using Xiigen.Fabrics.InMemory;
using Xiigen.Kernel;
using Xiigen.Platform.Marketplace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITenantContextAccessor, AsyncLocalTenantContextAccessor>();
builder.Services.AddSingleton<ICorrelationContextAccessor, AsyncLocalCorrelationContextAccessor>();
builder.Services.AddSingleton<ITenantContextRunner, TenantContextRunner>();
builder.Services.AddXiigenInMemoryFabrics();
builder.Services.AddXiigenMarketplace();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var tenantAccessor = context.RequestServices.GetRequiredService<ITenantContextAccessor>();
    var correlationAccessor = context.RequestServices.GetRequiredService<ICorrelationContextAccessor>();
    var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var incomingCorrelation) &&
        !string.IsNullOrWhiteSpace(incomingCorrelation)
            ? incomingCorrelation.ToString()
            : $"corr-{Guid.NewGuid():N}";
    var tenantId = context.Request.Headers.TryGetValue("X-Tenant-Id", out var incomingTenant) &&
        !string.IsNullOrWhiteSpace(incomingTenant)
            ? incomingTenant.ToString()
            : "platform";
    var roles = context.Request.Headers.TryGetValue("X-Roles", out var incomingRoles)
        ? incomingRoles.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal)
        : new HashSet<string>(StringComparer.Ordinal) { "platform_admin" };

    using var correlationScope = correlationAccessor.Push(new CorrelationContext(
        correlationId,
        "http-request",
        DateTimeOffset.UtcNow));
    using var tenantScope = tenantAccessor.Push(new TenantContext(
        tenantId,
        context.Request.Headers.TryGetValue("X-User-Id", out var userId) ? userId.ToString() : null,
        roles,
        correlationId,
        "http-request",
        DateTimeOffset.UtcNow));

    context.Response.Headers["X-Correlation-Id"] = correlationId;
    await next().ConfigureAwait(false);
});

app.MapGet("/health/live", () => Results.Ok(new { status = "live", service = "xiigen-platform-api" }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready", service = "xiigen-platform-api" }));

app.MapGet("/api/marketplace/listings", async (IMarketplaceService marketplace, CancellationToken cancellationToken) =>
{
    var result = await marketplace.ListListingsAsync(cancellationToken).ConfigureAwait(false);
    return ToHttpResult(result);
});

app.MapGet("/api/marketplace/listings/{flowSlug}/{version}", async (
    string flowSlug,
    string version,
    IMarketplaceService marketplace,
    CancellationToken cancellationToken) =>
{
    var result = await marketplace.GetListingAsync(flowSlug, version, cancellationToken).ConfigureAwait(false);
    return result.IsSuccess && result.Data is null
        ? Results.NotFound(new { code = "LISTING_NOT_FOUND", message = "Marketplace listing was not found." })
        : ToHttpResult(result);
});

app.MapPost("/api/marketplace/listings", async (
    PublishMarketplaceListingRequest request,
    IMarketplaceService marketplace,
    CancellationToken cancellationToken) =>
{
    var result = await marketplace.PublishListingAsync(request, cancellationToken).ConfigureAwait(false);
    return ToHttpResult(result);
});

app.MapPost("/api/marketplace/readbacks", async (
    PackageReadbackRequest request,
    IMarketplaceService marketplace,
    CancellationToken cancellationToken) =>
{
    var result = await marketplace.RecordPackageReadbackAsync(request, cancellationToken).ConfigureAwait(false);
    return ToHttpResult(result);
});

app.MapPost("/api/marketplace/forks", async (
    TenantForkRequest request,
    IMarketplaceService marketplace,
    CancellationToken cancellationToken) =>
{
    var result = await marketplace.CreateTenantForkAsync(request, cancellationToken).ConfigureAwait(false);
    return ToHttpResult(result);
});

app.MapPost("/api/marketplace/adaptations", async (
    ApplyAdaptationRequest request,
    IMarketplaceService marketplace,
    CancellationToken cancellationToken) =>
{
    var result = await marketplace.ApplyAdaptationAsync(request, cancellationToken).ConfigureAwait(false);
    return ToHttpResult(result);
});

app.MapGet("/marketplace", async (IMarketplaceService marketplace, CancellationToken cancellationToken) =>
{
    var result = await marketplace.ListListingsAsync(cancellationToken).ConfigureAwait(false);
    if (result.IsFailure)
    {
        return Results.Problem(result.Error?.Message, statusCode: StatusCodes.Status500InternalServerError);
    }

    var rows = new StringBuilder();
    foreach (var listing in result.Data ?? Array.Empty<IReadOnlyDictionary<string, object?>>())
    {
        rows.Append("<tr>");
        rows.AppendCell(listing, "flowId");
        rows.AppendCell(listing, "flowSlug");
        rows.AppendCell(listing, "version");
        rows.AppendCell(listing, "moduleName");
        rows.AppendCell(listing, "sourceRepo");
        rows.AppendCell(listing, "status");
        rows.Append("</tr>");
    }

    var html = $$"""
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <title>XIIGen Marketplace</title>
          <style>
            body { font-family: Arial, sans-serif; margin: 32px; color: #17202a; }
            h1 { font-size: 28px; margin-bottom: 4px; }
            p { margin-top: 0; color: #4d5b66; }
            table { border-collapse: collapse; width: 100%; margin-top: 24px; }
            th, td { border-bottom: 1px solid #d8dee4; padding: 10px 12px; text-align: left; }
            th { background: #f5f7f9; font-size: 13px; text-transform: uppercase; }
          </style>
        </head>
        <body>
          <h1>XIIGen Marketplace</h1>
          <p>Local package listing and tenant fork evidence surface.</p>
          <table>
            <thead>
              <tr>
                <th>Flow</th>
                <th>Slug</th>
                <th>Version</th>
                <th>Module</th>
                <th>Source Repo</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>{{rows}}</tbody>
          </table>
        </body>
        </html>
        """;
    return Results.Content(html, "text/html");
});

app.Run();

static IResult ToHttpResult<T>(DataProcessResult<T> result)
{
    if (result.IsSuccess)
    {
        return Results.Ok(result.Data);
    }

    return Results.BadRequest(new
    {
        code = result.Error?.Code ?? "REQUEST_FAILED",
        message = result.Error?.Message ?? "Request failed."
    });
}

internal static class MarketplaceHtmlExtensions
{
    public static void AppendCell(this StringBuilder builder, IReadOnlyDictionary<string, object?> row, string key)
    {
        var value = row.TryGetValue(key, out var raw) ? raw?.ToString() ?? string.Empty : string.Empty;
        builder.Append("<td>");
        builder.Append(WebUtility.HtmlEncode(value));
        builder.Append("</td>");
    }
}
