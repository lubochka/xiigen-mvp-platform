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

app.MapPost("/api/studio/workspaces", async (
    HttpContext context,
    CreateTenantWorkspaceRequest request,
    ITenantAdaptationStudioService studio,
    CancellationToken cancellationToken) =>
{
    var authorization = RequireStudioRole(context);
    if (authorization is not null)
    {
        return authorization;
    }

    var result = await studio.CreateWorkspaceAsync(request, cancellationToken).ConfigureAwait(false);
    return ToHttpResult(result);
});

app.MapGet("/api/studio/workspaces/{workspaceId}", async (
    HttpContext context,
    string workspaceId,
    ITenantAdaptationStudioService studio,
    CancellationToken cancellationToken) =>
{
    var authorization = RequireStudioRole(context);
    if (authorization is not null)
    {
        return authorization;
    }

    var result = await studio.GetWorkspaceAsync(workspaceId, cancellationToken).ConfigureAwait(false);
    return result.IsSuccess && result.Data is null
        ? Results.NotFound(new { code = "WORKSPACE_NOT_FOUND", message = "Workspace was not found in the current tenant." })
        : ToHttpResult(result);
});

app.MapPost("/api/studio/workspaces/{workspaceId}/sources", async (
    HttpContext context,
    string workspaceId,
    ConnectProjectSourceRequest request,
    ITenantAdaptationStudioService studio,
    CancellationToken cancellationToken) =>
{
    var authorization = RequireStudioRole(context);
    if (authorization is not null)
    {
        return authorization;
    }

    var result = await studio.ConnectProjectSourceAsync(request with { WorkspaceId = workspaceId }, cancellationToken)
        .ConfigureAwait(false);
    return ToHttpResult(result);
});

app.MapPost("/api/studio/workspaces/{workspaceId}/policies", async (
    HttpContext context,
    string workspaceId,
    CaptureTenantPolicyRequest request,
    ITenantAdaptationStudioService studio,
    CancellationToken cancellationToken) =>
{
    var authorization = RequireStudioRole(context);
    if (authorization is not null)
    {
        return authorization;
    }

    var result = await studio.CapturePolicyAsync(request with { WorkspaceId = workspaceId }, cancellationToken)
        .ConfigureAwait(false);
    return ToHttpResult(result);
});

app.MapPost("/api/studio/workspaces/{workspaceId}/adaptations", async (
    HttpContext context,
    string workspaceId,
    RequestTenantAdaptationRequest request,
    ITenantAdaptationStudioService studio,
    CancellationToken cancellationToken) =>
{
    var authorization = RequireStudioRole(context);
    if (authorization is not null)
    {
        return authorization;
    }

    var result = await studio.RequestAdaptationAsync(request with { WorkspaceId = workspaceId }, cancellationToken)
        .ConfigureAwait(false);
    return ToHttpResult(result);
});

app.MapPost("/api/studio/workspaces/{workspaceId}/adaptations/{adaptationId}/reviews", async (
    HttpContext context,
    string workspaceId,
    string adaptationId,
    ReviewAdaptationDiffRequest request,
    ITenantAdaptationStudioService studio,
    CancellationToken cancellationToken) =>
{
    var authorization = RequireStudioRole(context);
    if (authorization is not null)
    {
        return authorization;
    }

    var result = await studio.ReviewDiffAsync(request with { WorkspaceId = workspaceId, AdaptationId = adaptationId }, cancellationToken)
        .ConfigureAwait(false);
    return ToHttpResult(result);
});

app.MapGet("/api/studio/workspaces/{workspaceId}/audit", async (
    HttpContext context,
    string workspaceId,
    ITenantAdaptationStudioService studio,
    CancellationToken cancellationToken) =>
{
    var authorization = RequireStudioRole(context);
    if (authorization is not null)
    {
        return authorization;
    }

    var result = await studio.ListAuditAsync(workspaceId, cancellationToken).ConfigureAwait(false);
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

app.MapGet("/studio", () =>
{
    const string html = """
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <title>XIIGen Tenant Adaptation Studio</title>
          <style>
            body { font-family: Arial, sans-serif; margin: 32px; color: #17202a; background: #fafafa; }
            h1 { font-size: 30px; margin: 0 0 8px; }
            h2 { font-size: 18px; margin: 0 0 10px; }
            p { color: #4d5b66; line-height: 1.5; }
            main { max-width: 1120px; }
            section { background: #fff; border: 1px solid #d8dee4; border-radius: 8px; padding: 18px; margin: 16px 0; }
            .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 14px; }
            .badge { display: inline-block; padding: 4px 8px; border-radius: 999px; background: #eef6ff; color: #17446b; font-size: 12px; font-weight: 700; }
            code { background: #f0f3f6; padding: 2px 5px; border-radius: 4px; }
            ul { padding-inline-start: 20px; }
          </style>
        </head>
        <body>
          <main>
            <span class="badge">Primary tenant workspace</span>
            <h1>XIIGen Tenant Adaptation Studio</h1>
            <p>Create a tenant workspace, connect a project source, capture company coding and architecture policy, request an adaptation, review the plan and diff, then approve export only after audit is recorded.</p>

            <div class="grid">
              <section>
                <h2>Workspace</h2>
                <p>Endpoint: <code>POST /api/studio/workspaces</code></p>
                <ul>
                  <li>Tenant context from <code>X-Tenant-Id</code></li>
                  <li>Explicit role header required</li>
                  <li>Every record receives an audit event</li>
                </ul>
              </section>
              <section>
                <h2>Project Source</h2>
                <p>Endpoint: <code>POST /api/studio/workspaces/{workspaceId}/sources</code></p>
                <ul>
                  <li>Repository URL or snapshot evidence accepted</li>
                  <li>Tenant isolation checked before write</li>
                </ul>
              </section>
              <section>
                <h2>Company Policy</h2>
                <p>Endpoint: <code>POST /api/studio/workspaces/{workspaceId}/policies</code></p>
                <ul>
                  <li>Supported policy kinds: coding and architecture</li>
                  <li>Policy capture is immutable evidence for adaptation</li>
                </ul>
              </section>
              <section>
                <h2>Adaptation Review</h2>
                <p>Endpoint: <code>POST /api/studio/workspaces/{workspaceId}/adaptations</code></p>
                <ul>
                  <li>Plan-before-patch is created first</li>
                  <li>Diff remains review-required until approval</li>
                  <li>Approval/rejection writes audit evidence</li>
                </ul>
              </section>
            </div>
          </main>
        </body>
        </html>
        """;
    return Results.Content(html, "text/html");
});

app.Run();

static IResult? RequireStudioRole(HttpContext context)
{
    if (!context.Request.Headers.ContainsKey("X-Roles"))
    {
        return Results.Unauthorized();
    }

    var tenantAccessor = context.RequestServices.GetRequiredService<ITenantContextAccessor>();
    var roles = tenantAccessor.Current?.Roles ?? new HashSet<string>(StringComparer.Ordinal);
    return roles.Contains("platform_admin") ||
        roles.Contains("tenant_admin") ||
        roles.Contains("tenant_ops")
            ? null
            : Results.Forbid();
}

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
