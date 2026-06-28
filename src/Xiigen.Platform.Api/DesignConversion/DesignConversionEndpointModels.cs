using Xiigen.Kernel;

namespace Xiigen.Platform.Api.DesignConversion;

public sealed record DesignConversionSubmitRequest
{
    public string? Html { get; init; }
    public string? Css { get; init; }
    public IReadOnlyList<DesignConversionSourceFile>? SourceTree { get; init; }
    public IReadOnlyList<DesignConversionScreenshot>? Screenshots { get; init; }
    public string? SourceKind { get; init; }
    public string? DesignTier { get; init; }
    public int? LoopBudget { get; init; }
    public string? IdempotencyKey { get; init; }
    public string? TenantId { get; init; }
    public string? UserId { get; init; }
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

public sealed record DesignConversionSourceFile(
    string Path,
    string? Kind,
    string? Content);

public sealed record DesignConversionScreenshot(
    string Id,
    string? Uri,
    string? MimeType,
    string? Sha256,
    int? Width,
    int? Height);

public sealed record DesignConversionComparisonImage(
    string Id,
    string? Uri,
    string? Label,
    double? SimilarityScore);

public sealed record DesignConversionVerdict(
    string Status,
    double? Score,
    IReadOnlyList<string> Findings);

public sealed record DesignConversionJobResponse(
    string JobId,
    string Status,
    string? CurrentHtml,
    string? CurrentCss,
    IReadOnlyList<DesignConversionComparisonImage> ComparisonImages,
    DesignConversionVerdict? Verdict,
    string SourceKind,
    string DesignTier,
    int LoopBudget,
    string TenantId,
    string? UserId,
    string? IdempotencyKey,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public IReadOnlyList<string> Defects { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> EvidenceRefs { get; init; } = Array.Empty<string>();
}

public sealed record DesignConversionSubmitCommand(
    DesignConversionSubmitRequest Request,
    string TenantId,
    string? UserId,
    string IdempotencyKey,
    string? CorrelationId,
    string? IfMatch,
    DesignConversionTierContext Tier);

public interface IDesignConversionService
{
    Task<DataProcessResult<DesignConversionJobResponse>> SubmitAsync(
        DesignConversionSubmitCommand command,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult<DesignConversionJobResponse?>> GetJobAsync(
        string jobId,
        CancellationToken cancellationToken = default);
}

public static class DesignConversionHeaderNames
{
    public const string TenantId = "X-Tenant-Id";
    public const string UserId = "X-User-Id";
    public const string IdempotencyKey = "X-Idempotency-Key";
    public const string DesignTier = "X-Design-Tier";
    public const string LoopBudget = "X-Loop-Budget";
    public const string CorrelationId = "X-Correlation-Id";
    public const string RequestId = "X-Request-Id";
    public const string IfMatch = "If-Match";
}

public static class DesignConversionEndpointModels
{
    public static DesignConversionSubmitCommand CreateCommand(
        HttpContext context,
        DesignConversionSubmitRequest request,
        DesignConversionTierContext tier)
    {
        var tenantId = ReadHeader(context, DesignConversionHeaderNames.TenantId) ??
            request.TenantId ??
            "platform";
        var userId = ReadHeader(context, DesignConversionHeaderNames.UserId) ?? request.UserId;
        var idempotencyKey = ReadHeader(context, DesignConversionHeaderNames.IdempotencyKey) ??
            request.IdempotencyKey ??
            string.Empty;
        var correlationId = ReadHeader(context, DesignConversionHeaderNames.CorrelationId) ??
            ReadHeader(context, DesignConversionHeaderNames.RequestId);
        var ifMatch = ReadHeader(context, DesignConversionHeaderNames.IfMatch);

        return new DesignConversionSubmitCommand(
            request,
            tenantId,
            userId,
            idempotencyKey,
            correlationId,
            ifMatch,
            tier);
    }

    public static IReadOnlyDictionary<string, object?> ToDictionary(this DesignConversionJobResponse response) =>
        new Dictionary<string, object?>
        {
            ["jobId"] = response.JobId,
            ["status"] = response.Status,
            ["currentHtml"] = response.CurrentHtml,
            ["currentCss"] = response.CurrentCss,
            ["comparisonImages"] = response.ComparisonImages.Select(image => new Dictionary<string, object?>
            {
                ["id"] = image.Id,
                ["uri"] = image.Uri,
                ["label"] = image.Label,
                ["similarityScore"] = image.SimilarityScore
            }).ToArray(),
            ["verdict"] = response.Verdict is null
                ? null
                : new Dictionary<string, object?>
                {
                    ["status"] = response.Verdict.Status,
                    ["score"] = response.Verdict.Score,
                    ["findings"] = response.Verdict.Findings
                },
            ["sourceKind"] = response.SourceKind,
            ["designTier"] = response.DesignTier,
            ["loopBudget"] = response.LoopBudget,
            ["tenantId"] = response.TenantId,
            ["userId"] = response.UserId,
            ["idempotencyKey"] = response.IdempotencyKey,
            ["createdAt"] = response.CreatedAt,
            ["updatedAt"] = response.UpdatedAt,
            ["defects"] = response.Defects,
            ["warnings"] = response.Warnings,
            ["evidenceRefs"] = response.EvidenceRefs
        };

    public static IResult ToErrorResult(string code, string message, int statusCode) =>
        Results.Json(new { code, message }, statusCode: statusCode);

    private static string? ReadHeader(HttpContext context, string name) =>
        context.Request.Headers.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : null;
}
