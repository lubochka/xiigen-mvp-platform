using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xiigen.Kernel;

namespace Xiigen.Platform.Api.DesignConversion;

public sealed class Flow47DesignConversionOptions
{
    public const string MissingBaseUrlCode = "FLOW47_BASE_URL_REQUIRED";
    public const string MissingBaseUrlMessage = "Flow47:BaseUrl is required unless Flow47:UseLocalFakeClient is true.";
    public const string ReadinessProbeFailedCode = "FLOW47_READINESS_PROBE_FAILED";
    public const string SubmitEndpoint = "/flow-47/design-conversion/jobs";
    public const string StatusEndpointTemplate = "/flow-47/design-conversion/jobs/{jobId}";
    public const string ReadinessEndpoint = "/health/ready";
    public const string ReadinessFallbackEndpoint = "/health/live";
    private const string LegacySubmitEndpoint = "/jobs/design-conversions";
    private const string LegacyStatusEndpointTemplate = "/jobs/design-conversions/{jobId}";

    public string? BaseUrl { get; init; }
    public bool UseLocalFakeClient { get; init; }
    public string SubmitRoute { get; init; } = SubmitEndpoint;
    public string StatusRouteTemplate { get; init; } = StatusEndpointTemplate;
    public string ReadinessRoute { get; init; } = ReadinessEndpoint;
    public string SubmitRole { get; init; } = "platform_admin";
    public string StatusRole { get; init; } = "platform_support";
    public string DefaultTenantId { get; init; } = "master";
    public string AuthorizationTokenTemplate { get; init; } = "Bearer local-{tenantId}-token";

    public bool HasBaseUrl => !string.IsNullOrWhiteSpace(BaseUrl);
    public string Mode => UseLocalFakeClient ? "local-fake" : HasBaseUrl ? "external" : "not-configured";
    public string EffectiveSubmitRoute => NormalizeRoute(SubmitRoute, SubmitEndpoint, LegacySubmitEndpoint);
    public string EffectiveStatusRouteTemplate => NormalizeRoute(StatusRouteTemplate, StatusEndpointTemplate, LegacyStatusEndpointTemplate);
    public string EffectiveReadinessRoute => string.IsNullOrWhiteSpace(ReadinessRoute) ? ReadinessEndpoint : ReadinessRoute;

    private static string NormalizeRoute(string route, string currentRoute, string legacyRoute) =>
        string.Equals(route, legacyRoute, StringComparison.Ordinal) ? currentRoute : route;
}

public sealed record Flow47DesignConversionJobRequest(
    string TenantId,
    string? UserId,
    string IdempotencyKey,
    string? CorrelationId,
    string? IfMatch,
    string SourceKind,
    string DesignTier,
    int LoopBudget,
    string? Html,
    string? Css,
    IReadOnlyList<DesignConversionSourceFile> SourceTree,
    IReadOnlyList<DesignConversionScreenshot> Screenshots,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record Flow47DesignConversionSubmitRequest(
    string? TenantId,
    string SourceType,
    string SourceUri,
    IReadOnlyList<string>? OutputTargets,
    string? IdempotencyKey,
    IReadOnlyDictionary<string, object?>? Options);

public sealed record Flow47DesignConversionJobStatusResponse(
    string JobId,
    string TenantId,
    string Status,
    string? IdempotencyKey,
    string? SourceType,
    string? SourceUri,
    IReadOnlyList<string>? OutputTargets,
    string? CoreProviderJobId,
    Flow47DesignConversionErrorResponse? Error,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? ScaffoldNotice,
    Flow47DesignConversionEvaluationResultResponse? Result = null,
    Flow47DesignConversionEvaluationResultResponse? EvaluationResult = null,
    Flow47DesignConversionEvaluationResultResponse? Evaluation = null,
    string? VerdictId = null,
    bool? Passed = null,
    string? CurrentHtml = null,
    string? CurrentCss = null,
    Flow47DesignConversionVerdictResponse? Verdict = null,
    double? Score = null,
    IReadOnlyList<JsonElement>? Defects = null,
    IReadOnlyList<Flow47DesignConversionComparisonImageResponse>? ComparisonImages = null,
    IReadOnlyList<Flow47DesignConversionComparisonImageResponse>? ComparisonArtifacts = null,
    IReadOnlyList<Flow47DesignConversionArtifactResponse>? Artifacts = null,
    IReadOnlyList<string>? Warnings = null,
    IReadOnlyList<string>? EvidenceRefs = null,
    string? ErrorSummary = null);

public sealed record Flow47DesignConversionErrorResponse(string Code, string Message);

public sealed record Flow47DesignConversionEvaluationResultResponse(
    string? Status = null,
    string? VerdictId = null,
    bool? Passed = null,
    string? CurrentHtml = null,
    string? CurrentCss = null,
    Flow47DesignConversionVerdictResponse? Verdict = null,
    double? Score = null,
    IReadOnlyList<JsonElement>? Defects = null,
    IReadOnlyList<Flow47DesignConversionComparisonImageResponse>? ComparisonImages = null,
    IReadOnlyList<Flow47DesignConversionComparisonImageResponse>? ComparisonArtifacts = null,
    IReadOnlyList<Flow47DesignConversionArtifactResponse>? Artifacts = null,
    IReadOnlyList<string>? Warnings = null,
    IReadOnlyList<string>? EvidenceRefs = null,
    string? ErrorSummary = null);

public sealed record Flow47DesignConversionVerdictResponse(
    string? Status = null,
    double? Score = null,
    IReadOnlyList<JsonElement>? Defects = null,
    IReadOnlyList<string>? Findings = null);

public sealed record Flow47DesignConversionComparisonImageResponse(
    string? Id = null,
    string? Uri = null,
    string? Label = null,
    double? SimilarityScore = null,
    string? ArtifactId = null,
    string? ExpectedImageUri = null,
    string? ActualImageUri = null,
    string? DiffImageUri = null,
    double? Score = null);

public sealed record Flow47DesignConversionArtifactResponse(
    string? Id = null,
    string? Uri = null,
    string? Label = null,
    string? Kind = null,
    double? SimilarityScore = null);

public sealed record Flow47DesignConversionReadinessResult(
    bool IsReady,
    string Mode,
    string? Code = null,
    string? Message = null);

public interface IFlow47DesignConversionReadinessProbe
{
    Task<Flow47DesignConversionReadinessResult> CheckAsync(CancellationToken cancellationToken = default);
}

public interface IFlow47DesignConversionClient
{
    Task<DataProcessResult<DesignConversionJobResponse>> SubmitAsync(
        Flow47DesignConversionJobRequest request,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult<DesignConversionJobResponse?>> GetJobAsync(
        string jobId,
        string? tenantId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default);
}

public sealed class Flow47DesignConversionDispatchService : IDesignConversionService
{
    private readonly IFlow47DesignConversionClient _flowClient;
    private readonly ConcurrentDictionary<string, string> _jobTenants = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Flow47DesignConversionJobRequest> _jobRequests = new(StringComparer.Ordinal);

    public Flow47DesignConversionDispatchService(IFlow47DesignConversionClient flowClient)
    {
        _flowClient = flowClient;
    }

    public Task<DataProcessResult<DesignConversionJobResponse>> SubmitAsync(
        DesignConversionSubmitCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Request.Html) &&
            string.IsNullOrWhiteSpace(command.Request.Css) &&
            (command.Request.SourceTree is null || command.Request.SourceTree.Count == 0) &&
            (command.Request.Screenshots is null || command.Request.Screenshots.Count == 0))
        {
            return Task.FromResult(DataProcessResult<DesignConversionJobResponse>.Failure(
                "DESIGN_SOURCE_REQUIRED",
                "At least one of html, css, sourceTree, or screenshots is required."));
        }

        var request = new Flow47DesignConversionJobRequest(
            command.TenantId,
            command.UserId,
            command.IdempotencyKey,
            command.CorrelationId,
            command.IfMatch,
            command.Request.SourceKind ?? "html",
            command.Tier.DesignTier,
            command.Tier.LoopBudget,
            command.Request.Html,
            command.Request.Css,
            command.Request.SourceTree ?? Array.Empty<DesignConversionSourceFile>(),
            command.Request.Screenshots ?? Array.Empty<DesignConversionScreenshot>(),
            command.Request.Metadata ?? new Dictionary<string, string>());

        return SubmitAndRememberTenantAsync(request, command.TenantId, cancellationToken);
    }

    public Task<DataProcessResult<DesignConversionJobResponse?>> GetJobAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return Task.FromResult(DataProcessResult<DesignConversionJobResponse?>.Failure(
                "JOB_ID_REQUIRED",
                "jobId is required."));
        }

        _jobTenants.TryGetValue(jobId, out var tenantId);
        _jobRequests.TryGetValue(jobId, out var originalRequest);
        return GetAndRestoreRequestContextAsync(jobId, tenantId, originalRequest, cancellationToken);
    }

    private async Task<DataProcessResult<DesignConversionJobResponse>> SubmitAndRememberTenantAsync(
        Flow47DesignConversionJobRequest request,
        string tenantId,
        CancellationToken cancellationToken)
    {
        var result = await _flowClient.SubmitAsync(request, cancellationToken).ConfigureAwait(false);
        if (!result.IsFailure && result.Data is not null)
        {
            _jobTenants[result.Data.JobId] = tenantId;
            _jobRequests[result.Data.JobId] = request;
        }

        return result;
    }

    private async Task<DataProcessResult<DesignConversionJobResponse?>> GetAndRestoreRequestContextAsync(
        string jobId,
        string? tenantId,
        Flow47DesignConversionJobRequest? originalRequest,
        CancellationToken cancellationToken)
    {
        var result = await _flowClient.GetJobAsync(jobId, tenantId, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (result.IsFailure || result.Data is null || originalRequest is null)
        {
            return result;
        }

        return DataProcessResult<DesignConversionJobResponse?>.Success(
            RestoreRequestContext(result.Data, originalRequest));
    }

    private static DesignConversionJobResponse RestoreRequestContext(
        DesignConversionJobResponse response,
        Flow47DesignConversionJobRequest originalRequest) =>
        response with
        {
            CurrentHtml = response.CurrentHtml ?? originalRequest.Html,
            CurrentCss = response.CurrentCss ?? originalRequest.Css,
            SourceKind = originalRequest.SourceKind,
            DesignTier = originalRequest.DesignTier,
            LoopBudget = originalRequest.LoopBudget,
            TenantId = originalRequest.TenantId,
            UserId = originalRequest.UserId ?? response.UserId,
            IdempotencyKey = originalRequest.IdempotencyKey
        };
}

public sealed class LocalFakeFlow47DesignConversionClient : IFlow47DesignConversionClient
{
    private readonly ConcurrentDictionary<string, DesignConversionJobResponse> _jobs = new(StringComparer.Ordinal);

    public Task<DataProcessResult<DesignConversionJobResponse>> SubmitAsync(
        Flow47DesignConversionJobRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var jobId = $"flow47_fake_{Guid.NewGuid():N}";
        var response = new DesignConversionJobResponse(
            jobId,
            "accepted",
            request.Html,
            request.Css,
            Array.Empty<DesignConversionComparisonImage>(),
            new DesignConversionVerdict(
                "pending",
                null,
                new[] { "Design conversion was accepted by the local fake FLOW-47 client; configure Flow47:BaseUrl for external dispatch." }),
            request.SourceKind,
            request.DesignTier,
            request.LoopBudget,
            request.TenantId,
            request.UserId,
            request.IdempotencyKey,
            now,
            now);

        _jobs[jobId] = response;
        return Task.FromResult(DataProcessResult<DesignConversionJobResponse>.Success(response));
    }

    public Task<DataProcessResult<DesignConversionJobResponse?>> GetJobAsync(
        string jobId,
        string? tenantId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        _jobs.TryGetValue(jobId, out var job);
        return Task.FromResult(DataProcessResult<DesignConversionJobResponse?>.Success(job));
    }
}

public sealed class LocalFakeFlow47DesignConversionReadinessProbe : IFlow47DesignConversionReadinessProbe
{
    private readonly Flow47DesignConversionOptions _options;

    public LocalFakeFlow47DesignConversionReadinessProbe(Flow47DesignConversionOptions options)
    {
        _options = options;
    }

    public Task<Flow47DesignConversionReadinessResult> CheckAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new Flow47DesignConversionReadinessResult(true, _options.Mode));
}

public sealed class MissingFlow47ConfigurationDesignConversionClient : IFlow47DesignConversionClient
{
    public Task<DataProcessResult<DesignConversionJobResponse>> SubmitAsync(
        Flow47DesignConversionJobRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(DataProcessResult<DesignConversionJobResponse>.Failure(
            Flow47DesignConversionOptions.MissingBaseUrlCode,
            Flow47DesignConversionOptions.MissingBaseUrlMessage));

    public Task<DataProcessResult<DesignConversionJobResponse?>> GetJobAsync(
        string jobId,
        string? tenantId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(DataProcessResult<DesignConversionJobResponse?>.Failure(
            Flow47DesignConversionOptions.MissingBaseUrlCode,
            Flow47DesignConversionOptions.MissingBaseUrlMessage));
}

public sealed class MissingFlow47ConfigurationReadinessProbe : IFlow47DesignConversionReadinessProbe
{
    private readonly Flow47DesignConversionOptions _options;

    public MissingFlow47ConfigurationReadinessProbe(Flow47DesignConversionOptions options)
    {
        _options = options;
    }

    public Task<Flow47DesignConversionReadinessResult> CheckAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new Flow47DesignConversionReadinessResult(
            false,
            _options.Mode,
            Flow47DesignConversionOptions.MissingBaseUrlCode,
            Flow47DesignConversionOptions.MissingBaseUrlMessage));
}

public sealed class HttpFlow47DesignConversionClient : IFlow47DesignConversionClient
{
    private readonly HttpClient _httpClient;
    private readonly Flow47DesignConversionOptions _options;

    public HttpFlow47DesignConversionClient(HttpClient httpClient, Flow47DesignConversionOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<DataProcessResult<DesignConversionJobResponse>> SubmitAsync(
        Flow47DesignConversionJobRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _options.EffectiveSubmitRoute)
        {
            Content = JsonContent.Create(ToFlowSubmitRequest(request))
        };
        ApplyFlowHeaders(httpRequest, request.TenantId, request.CorrelationId, request.IdempotencyKey, _options.SubmitRole);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return DataProcessResult<DesignConversionJobResponse>.Failure(
                "FLOW47_SUBMIT_FAILED",
                $"FLOW-47 submit failed with HTTP {(int)response.StatusCode}.");
        }

        var job = await response.Content.ReadFromJsonAsync<Flow47DesignConversionJobStatusResponse>(cancellationToken)
            .ConfigureAwait(false);
        return job is null
            ? DataProcessResult<DesignConversionJobResponse>.Failure(
                "FLOW47_SUBMIT_EMPTY_RESPONSE",
                "FLOW-47 submit returned an empty response.")
            : DataProcessResult<DesignConversionJobResponse>.Success(ToPlatformResponse(job, request));
    }

    public async Task<DataProcessResult<DesignConversionJobResponse?>> GetJobAsync(
        string jobId,
        string? tenantId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var route = _options.EffectiveStatusRouteTemplate.Replace("{jobId}", Uri.EscapeDataString(jobId), StringComparison.Ordinal);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, route);
        ApplyFlowHeaders(httpRequest, tenantId ?? _options.DefaultTenantId, correlationId, null, _options.StatusRole);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return DataProcessResult<DesignConversionJobResponse?>.Success(null);
        }

        if (!response.IsSuccessStatusCode)
        {
            return DataProcessResult<DesignConversionJobResponse?>.Failure(
                "FLOW47_STATUS_FAILED",
                $"FLOW-47 status lookup failed with HTTP {(int)response.StatusCode}.");
        }

        var job = await response.Content.ReadFromJsonAsync<Flow47DesignConversionJobStatusResponse>(cancellationToken)
            .ConfigureAwait(false);
        return DataProcessResult<DesignConversionJobResponse?>.Success(job is null ? null : ToPlatformResponse(job));
    }

    private Flow47DesignConversionSubmitRequest ToFlowSubmitRequest(Flow47DesignConversionJobRequest request)
    {
        var options = new Dictionary<string, object?>
        {
            ["designTier"] = request.DesignTier,
            ["loopBudget"] = request.LoopBudget,
            ["userId"] = request.UserId,
            ["ifMatch"] = request.IfMatch,
            ["html"] = request.Html,
            ["css"] = request.Css,
            ["sourceTree"] = request.SourceTree,
            ["screenshots"] = request.Screenshots,
            ["metadata"] = request.Metadata
        };

        return new Flow47DesignConversionSubmitRequest(
            request.TenantId,
            NormalizeSourceType(request.SourceKind),
            ResolveSourceUri(request),
            ResolveOutputTargets(request),
            request.IdempotencyKey,
            options);
    }

    private static DesignConversionJobResponse ToPlatformResponse(
        Flow47DesignConversionJobStatusResponse response,
        Flow47DesignConversionJobRequest? originalRequest = null)
    {
        var findings = new List<string>();
        var evaluation = response.Result ?? response.EvaluationResult ?? response.Evaluation;
        if (!string.IsNullOrWhiteSpace(response.CoreProviderJobId))
        {
            findings.Add($"FLOW-47 provider job: {response.CoreProviderJobId}");
        }

        if (response.Error is not null)
        {
            findings.Add($"{response.Error.Code}: {response.Error.Message}");
        }

        if (!string.IsNullOrWhiteSpace(response.ScaffoldNotice))
        {
            findings.Add(response.ScaffoldNotice);
        }

        if (!string.IsNullOrWhiteSpace(response.ErrorSummary))
        {
            findings.Add(response.ErrorSummary);
        }

        if (!string.IsNullOrWhiteSpace(evaluation?.ErrorSummary))
        {
            findings.Add(evaluation.ErrorSummary);
        }

        var defects = DistinctDefects(
            response.Defects,
            response.Verdict?.Defects,
            evaluation?.Defects,
            evaluation?.Verdict?.Defects);
        var warnings = DistinctStrings(response.Warnings, evaluation?.Warnings);
        var evidenceRefs = DistinctStrings(response.EvidenceRefs, evaluation?.EvidenceRefs);
        findings.AddRange(DistinctStrings(response.Verdict?.Findings, evaluation?.Verdict?.Findings, defects, warnings));

        var status = evaluation?.Status ?? response.Status;
        var verdictStatus = response.Verdict?.Status ??
            evaluation?.Verdict?.Status ??
            evaluation?.VerdictId ??
            response.VerdictId ??
            ToVerdictStatus(evaluation?.Passed ?? response.Passed) ??
            evaluation?.Status ??
            response.Status;
        var score = response.Verdict?.Score ?? evaluation?.Verdict?.Score ?? evaluation?.Score ?? response.Score;

        return new DesignConversionJobResponse(
            response.JobId,
            status,
            evaluation?.CurrentHtml ?? response.CurrentHtml ?? originalRequest?.Html,
            evaluation?.CurrentCss ?? response.CurrentCss ?? originalRequest?.Css,
            MapComparisonImages(response, evaluation),
            new DesignConversionVerdict(
                verdictStatus,
                score,
                DistinctStrings(findings)),
            originalRequest?.SourceKind ?? response.SourceType ?? "unknown",
            originalRequest?.DesignTier ?? "unknown",
            originalRequest?.LoopBudget ?? 0,
            response.TenantId,
            originalRequest?.UserId,
            response.IdempotencyKey,
            response.CreatedAt,
            response.UpdatedAt)
        {
            Defects = defects,
            Warnings = warnings,
            EvidenceRefs = evidenceRefs
        };
    }

    private void ApplyFlowHeaders(
        HttpRequestMessage request,
        string tenantId,
        string? correlationId,
        string? idempotencyKey,
        string role)
    {
        request.Headers.TryAddWithoutValidation("X-Tenant-Id", tenantId);
        request.Headers.TryAddWithoutValidation("X-Role", role);
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
        }

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
        }

        request.Headers.Authorization = AuthenticationHeaderValue.Parse(
            _options.AuthorizationTokenTemplate.Replace("{tenantId}", tenantId, StringComparison.Ordinal));
    }

    private static string NormalizeSourceType(string sourceKind)
    {
        return sourceKind switch
        {
            "figma" => "figma-file",
            "figma-file" => "figma-file",
            "canva" => "canva-design",
            "canva-design" => "canva-design",
            "image" => "image",
            "html" => "html",
            _ => "unknown"
        };
    }

    private static string ResolveSourceUri(Flow47DesignConversionJobRequest request)
    {
        if (request.Metadata.TryGetValue("sourceUri", out var sourceUri) && !string.IsNullOrWhiteSpace(sourceUri))
        {
            return sourceUri;
        }

        return NormalizeSourceType(request.SourceKind) switch
        {
            "figma-file" => "figma://platform/design-conversion/source",
            "canva-design" => "canva://platform/design-conversion/source",
            "image" => request.Screenshots.FirstOrDefault()?.Uri ?? "image://platform/design-conversion/source",
            "html" => "inline://platform/design-conversion/html",
            _ => "unknown://platform/design-conversion/source"
        };
    }

    private static IReadOnlyList<string> ResolveOutputTargets(Flow47DesignConversionJobRequest request)
    {
        if (request.Metadata.TryGetValue("outputTargets", out var outputTargets) && !string.IsNullOrWhiteSpace(outputTargets))
        {
            return outputTargets.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        return new[] { "angular" };
    }

    private static IReadOnlyList<DesignConversionComparisonImage> MapComparisonImages(
        Flow47DesignConversionJobStatusResponse response,
        Flow47DesignConversionEvaluationResultResponse? evaluation)
    {
        var images = new List<DesignConversionComparisonImage>();
        AddComparisonImages(images, response.ComparisonImages);
        AddComparisonImages(images, response.ComparisonArtifacts);
        AddArtifacts(images, response.Artifacts);
        AddComparisonImages(images, evaluation?.ComparisonImages);
        AddComparisonImages(images, evaluation?.ComparisonArtifacts);
        AddArtifacts(images, evaluation?.Artifacts);
        return images
            .GroupBy(image => string.IsNullOrWhiteSpace(image.Id) ? image.Uri ?? image.Label ?? string.Empty : image.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
    }

    private static void AddComparisonImages(
        List<DesignConversionComparisonImage> target,
        IReadOnlyList<Flow47DesignConversionComparisonImageResponse>? source)
    {
        if (source is null)
        {
            return;
        }

        foreach (var image in source)
        {
            var artifactId = image.ArtifactId ?? image.Id;
            var score = image.Score ?? image.SimilarityScore;
            var addedFlowArtifact = false;

            addedFlowArtifact |= AddFlowArtifactImage(target, artifactId, image.DiffImageUri, "diff", score);
            addedFlowArtifact |= AddFlowArtifactImage(target, artifactId, image.ExpectedImageUri, "expected", score);
            addedFlowArtifact |= AddFlowArtifactImage(target, artifactId, image.ActualImageUri, "actual", score);

            if (!addedFlowArtifact || !string.IsNullOrWhiteSpace(image.Uri))
            {
                target.Add(new DesignConversionComparisonImage(
                    artifactId ?? image.Uri ?? "comparison",
                    image.Uri,
                    image.Label,
                    score));
            }
        }
    }

    private static bool AddFlowArtifactImage(
        List<DesignConversionComparisonImage> target,
        string? artifactId,
        string? uri,
        string label,
        double? score)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return false;
        }

        var id = string.IsNullOrWhiteSpace(artifactId)
            ? $"{label}-comparison"
            : $"{artifactId}:{label}";
        target.Add(new DesignConversionComparisonImage(id, uri, label, score));
        return true;
    }

    private static void AddArtifacts(
        List<DesignConversionComparisonImage> target,
        IReadOnlyList<Flow47DesignConversionArtifactResponse>? source)
    {
        if (source is null)
        {
            return;
        }

        foreach (var artifact in source.Where(artifact =>
            string.Equals(artifact.Kind, "comparison", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(artifact.Kind, "comparison-image", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(artifact.Kind, "image", StringComparison.OrdinalIgnoreCase) ||
            !string.IsNullOrWhiteSpace(artifact.Uri)))
        {
            target.Add(new DesignConversionComparisonImage(
                artifact.Id ?? artifact.Uri ?? "comparison",
                artifact.Uri,
                artifact.Label ?? artifact.Kind,
                artifact.SimilarityScore));
        }
    }

    private static IReadOnlyList<string> DistinctStrings(params IReadOnlyList<string>?[] sources) =>
        sources
            .Where(source => source is not null)
            .SelectMany(source => source!)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> DistinctDefects(params IReadOnlyList<JsonElement>?[] sources) =>
        sources
            .Where(source => source is not null)
            .SelectMany(source => source!)
            .Select(FormatDefect)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static string? FormatDefect(JsonElement defect) =>
        defect.ValueKind switch
        {
            JsonValueKind.String => defect.GetString(),
            JsonValueKind.Object => FormatDefectObject(defect),
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => defect.ToString(),
            _ => null
        };

    private static string FormatDefectObject(JsonElement defect)
    {
        var label = GetJsonPropertyString(defect, "code", "id", "type", "kind", "category", "name");
        var message = GetJsonPropertyString(defect, "message", "description", "summary", "detail", "details");
        var contextParts = new[]
            {
                FormatDefectProperty(defect, "severity"),
                FormatDefectProperty(defect, "selector"),
                FormatDefectProperty(defect, "path"),
                FormatDefectProperty(defect, "expected"),
                FormatDefectProperty(defect, "actual")
            }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        if (string.IsNullOrWhiteSpace(label) && string.IsNullOrWhiteSpace(message))
        {
            return defect.GetRawText();
        }

        var text = string.IsNullOrWhiteSpace(label)
            ? message!
            : string.IsNullOrWhiteSpace(message)
                ? label
                : $"{label}: {message}";

        return contextParts.Length == 0
            ? text
            : $"{text} ({string.Join("; ", contextParts)})";
    }

    private static string? FormatDefectProperty(JsonElement defect, string name)
    {
        var value = GetJsonPropertyString(defect, name);
        return string.IsNullOrWhiteSpace(value) ? null : $"{name}: {value}";
    }

    private static string? GetJsonPropertyString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (!string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString(),
                    JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => property.Value.ToString(),
                    JsonValueKind.Object or JsonValueKind.Array => property.Value.GetRawText(),
                    _ => null
                };
            }
        }

        return null;
    }

    private static string? ToVerdictStatus(bool? passed) =>
        passed switch
        {
            true => "pass",
            false => "fail",
            _ => null
        };
}

public sealed class HttpFlow47DesignConversionReadinessProbe : IFlow47DesignConversionReadinessProbe
{
    private readonly HttpClient _httpClient;
    private readonly Flow47DesignConversionOptions _options;

    public HttpFlow47DesignConversionReadinessProbe(HttpClient httpClient, Flow47DesignConversionOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<Flow47DesignConversionReadinessResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        var primary = await ProbeAsync(_options.EffectiveReadinessRoute, cancellationToken).ConfigureAwait(false);
        if (primary.IsReady || !string.Equals(_options.EffectiveReadinessRoute, Flow47DesignConversionOptions.ReadinessEndpoint, StringComparison.Ordinal))
        {
            return primary;
        }

        return await ProbeAsync(Flow47DesignConversionOptions.ReadinessFallbackEndpoint, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Flow47DesignConversionReadinessResult> ProbeAsync(string route, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(route, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return new Flow47DesignConversionReadinessResult(true, _options.Mode);
            }

            return new Flow47DesignConversionReadinessResult(
                false,
                _options.Mode,
                Flow47DesignConversionOptions.ReadinessProbeFailedCode,
                $"FLOW-47 readiness probe {route} failed with HTTP {(int)response.StatusCode}.");
        }
        catch (HttpRequestException ex)
        {
            return new Flow47DesignConversionReadinessResult(
                false,
                _options.Mode,
                Flow47DesignConversionOptions.ReadinessProbeFailedCode,
                $"FLOW-47 readiness probe {route} failed: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return new Flow47DesignConversionReadinessResult(
                false,
                _options.Mode,
                Flow47DesignConversionOptions.ReadinessProbeFailedCode,
                $"FLOW-47 readiness probe {route} timed out: {ex.Message}");
        }
    }
}
