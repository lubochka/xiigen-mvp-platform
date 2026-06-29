using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xiigen.Kernel;
using Xiigen.Platform.Api.DesignConversion;

namespace Xiigen.Platform.Api.Tests;

public sealed class DesignConversionEndpointTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _baseFactory;
    private readonly WebApplicationFactory<Program> _factory;

    public DesignConversionEndpointTests(WebApplicationFactory<Program> factory)
    {
        _baseFactory = factory;
        _factory = CreateFactoryWithLocalFakeMode(factory);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task ReadyReportsLocalFakeModeWhenExplicitlyEnabled()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadObjectAsync(response);
        Assert.Equal("ready", body.RootElement.GetProperty("status").GetString());
        Assert.Equal("local-fake", body.RootElement.GetProperty("flow47").GetProperty("mode").GetString());
    }

    [Fact]
    public async Task ReadyReportsMissingFlow47BaseUrlWhenLocalFakeModeIsNotEnabled()
    {
        using var factory = _baseFactory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Flow47:UseLocalFakeClient", "false");
            builder.UseSetting("Flow47:BaseUrl", string.Empty);
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await ReadObjectAsync(response);
        Assert.Equal("not-ready", body.RootElement.GetProperty("status").GetString());
        var flow47 = body.RootElement.GetProperty("flow47");
        Assert.Equal("not-configured", flow47.GetProperty("mode").GetString());
        Assert.Equal(Flow47DesignConversionOptions.MissingBaseUrlCode, flow47.GetProperty("code").GetString());
        Assert.Contains("Flow47:BaseUrl", flow47.GetProperty("message").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task SubmitAndStatusReturnDesignConversionJob()
    {
        var client = _factory.CreateClient();
        AddAuthorizedHeaders(client, "submit-and-status");

        var submitResponse = await client.PostAsJsonAsync("/conversion/submit", CreateSubmitRequest());

        Assert.Equal(HttpStatusCode.Accepted, submitResponse.StatusCode);
        var submitted = await ReadObjectAsync(submitResponse);
        var jobId = submitted.RootElement.GetProperty("jobId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(jobId));
        Assert.Equal("accepted", submitted.RootElement.GetProperty("status").GetString());
        Assert.Equal("<main>hello</main>", submitted.RootElement.GetProperty("currentHtml").GetString());

        var statusResponse = await client.GetAsync($"/conversion/jobs/{jobId}");

        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
        var status = await ReadObjectAsync(statusResponse);
        Assert.Equal(jobId, status.RootElement.GetProperty("jobId").GetString());
        Assert.Equal("pending", status.RootElement.GetProperty("verdict").GetProperty("status").GetString());
    }

    [Fact]
    public async Task SubmitAndStatusDispatchThroughFakeFlowClientAndPreserveResponseShape()
    {
        var fakeFlowClient = new TrackingFakeFlow47DesignConversionClient();
        using var factory = CreateFactoryWithFakeFlowClient(fakeFlowClient);
        var client = factory.CreateClient();
        AddAuthorizedHeaders(client, "fake-flow-dispatch");

        var submitResponse = await client.PostAsJsonAsync("/conversion/submit", CreateSubmitRequest());

        Assert.Equal(HttpStatusCode.Accepted, submitResponse.StatusCode);
        var submitted = await ReadObjectAsync(submitResponse);
        AssertDesignConversionResponseShape(submitted.RootElement);
        var jobId = submitted.RootElement.GetProperty("jobId").GetString();
        Assert.Equal("tracking_fake_flow47_job_1", jobId);
        Assert.Equal("html", fakeFlowClient.LastSubmitRequest?.SourceKind);
        Assert.Equal("pro", fakeFlowClient.LastSubmitRequest?.DesignTier);
        Assert.Equal(3, fakeFlowClient.LastSubmitRequest?.LoopBudget);
        Assert.Equal("tenant-a", fakeFlowClient.LastSubmitRequest?.TenantId);
        Assert.Equal("user-a", fakeFlowClient.LastSubmitRequest?.UserId);

        var statusResponse = await client.GetAsync($"/conversion/jobs/{jobId}");

        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
        var status = await ReadObjectAsync(statusResponse);
        AssertDesignConversionResponseShape(status.RootElement);
        Assert.Equal(1, fakeFlowClient.SubmitCount);
        Assert.Equal(1, fakeFlowClient.StatusCount);
    }

    [Fact]
    public async Task SubmitRequiresStudioRoleHeader()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", "tenant-a");
        client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
        client.DefaultRequestHeaders.Add("X-Design-Tier", "pro");
        client.DefaultRequestHeaders.Add("X-Loop-Budget", "3");
        client.DefaultRequestHeaders.Add("X-Idempotency-Key", "missing-role");

        var response = await client.PostAsJsonAsync("/conversion/submit", CreateSubmitRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SubmitRejectsTierBudgetAbovePolicyLimit()
    {
        var client = _factory.CreateClient();
        AddAuthorizedHeaders(client, "tier-denied", designTier: "starter", loopBudget: "3");

        var response = await client.PostAsJsonAsync("/conversion/submit", CreateSubmitRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadObjectAsync(response);
        Assert.Equal("LOOP_BUDGET_DENIED", body.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task SubmitReplaysSucceededIdempotencyKey()
    {
        var fakeFlowClient = new TrackingFakeFlow47DesignConversionClient();
        using var factory = CreateFactoryWithFakeFlowClient(fakeFlowClient);
        var client = factory.CreateClient();
        AddAuthorizedHeaders(client, "replay-key");

        var first = await client.PostAsJsonAsync("/conversion/submit", CreateSubmitRequest());
        var second = await client.PostAsJsonAsync("/conversion/submit", CreateSubmitRequest());

        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var firstBody = await ReadObjectAsync(first);
        var secondBody = await ReadObjectAsync(second);
        Assert.Equal(
            firstBody.RootElement.GetProperty("jobId").GetString(),
            secondBody.RootElement.GetProperty("jobId").GetString());
        Assert.Equal(1, fakeFlowClient.SubmitCount);
    }

    [Fact]
    public async Task HttpFlowClientPostsFlow47SubmitContractHeadersAndMapsStatus()
    {
        var handler = new CapturingFlow47Handler(request =>
        {
            var now = DateTimeOffset.UtcNow;
            return new
            {
                jobId = "dcj-contract",
                tenantId = "tenant-a",
                status = "queued",
                idempotencyKey = "flow-contract",
                sourceType = "html",
                sourceUri = "inline://platform/design-conversion/html",
                outputTargets = new[] { "angular" },
                coreProviderJobId = "fake-core-dcj-contract",
                error = (object?)null,
                createdAt = now,
                updatedAt = now,
                scaffoldNotice = "FLOW-47 scaffold: status reflects fake core dispatch only."
            };
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://flow47.local") };
        var flowClient = new HttpFlow47DesignConversionClient(
            httpClient,
            new Flow47DesignConversionOptions { SubmitRoute = "/jobs/design-conversions" });

        var result = await flowClient.SubmitAsync(CreateFlowJobRequest("flow-contract"));

        Assert.False(result.IsFailure);
        Assert.NotNull(result.Data);
        Assert.Equal("/flow-47/design-conversion/jobs", handler.LastRequest?.RequestUri?.AbsolutePath);
        AssertHeader(handler.LastRequest, "X-Tenant-Id", "tenant-a");
        AssertHeader(handler.LastRequest, "X-Role", "platform_admin");
        AssertHeader(handler.LastRequest, "X-Correlation-Id", "corr-contract");
        AssertHeader(handler.LastRequest, "Idempotency-Key", "flow-contract");
        Assert.Equal(new AuthenticationHeaderValue("Bearer", "local-tenant-a-token"), handler.LastRequest?.Headers.Authorization);

        var body = JsonDocument.Parse(handler.LastRequestBody ?? "{}").RootElement;
        Assert.Equal("tenant-a", body.GetProperty("tenantId").GetString());
        Assert.Equal("html", body.GetProperty("sourceType").GetString());
        Assert.Equal("inline://platform/design-conversion/html", body.GetProperty("sourceUri").GetString());
        Assert.Equal("flow-contract", body.GetProperty("idempotencyKey").GetString());
        Assert.Equal("angular", body.GetProperty("outputTargets")[0].GetString());
        Assert.Equal("pro", body.GetProperty("options").GetProperty("designTier").GetString());
        Assert.Equal(3, body.GetProperty("options").GetProperty("loopBudget").GetInt32());
        Assert.False(body.TryGetProperty("sourceKind", out _));

        Assert.Equal("dcj-contract", result.Data.JobId);
        Assert.Equal("queued", result.Data.Status);
        Assert.Equal("html", result.Data.SourceKind);
        Assert.Equal("pro", result.Data.DesignTier);
        Assert.Equal(3, result.Data.LoopBudget);
        Assert.Equal("tenant-a", result.Data.TenantId);
        Assert.Contains(result.Data.Verdict!.Findings, finding => finding.Contains("fake-core-dcj-contract", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Flow47DispatchPreservesSubmitContextWhenHttpStatusOmitsIt()
    {
        var handler = new CapturingFlow47Handler(request =>
        {
            var now = DateTimeOffset.UtcNow;
            if (request.Method == HttpMethod.Post)
            {
                return new
                {
                    jobId = "dcj-status",
                    tenantId = "tenant-a",
                    status = "accepted",
                    idempotencyKey = "flow-contract",
                    sourceType = "html",
                    sourceUri = "inline://platform/design-conversion/html",
                    outputTargets = new[] { "angular" },
                    coreProviderJobId = "fake-core-dcj-submit",
                    error = (object?)null,
                    createdAt = now,
                    updatedAt = now,
                    scaffoldNotice = "FLOW-47 scaffold: submit accepted."
                };
            }

            return new
            {
                jobId = "dcj-status",
                tenantId = "tenant-a",
                status = "queued",
                idempotencyKey = "flow-contract",
                sourceType = "html",
                sourceUri = "inline://platform/design-conversion/html",
                outputTargets = new[] { "angular" },
                coreProviderJobId = "fake-core-dcj-status",
                error = (object?)null,
                createdAt = now,
                updatedAt = now,
                scaffoldNotice = "FLOW-47 scaffold: status reflects fake core dispatch only."
            };
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://flow47.local") };
        var flowClient = new HttpFlow47DesignConversionClient(
            httpClient,
            new Flow47DesignConversionOptions { StatusRouteTemplate = "/jobs/design-conversions/{jobId}" });
        var dispatchService = new Flow47DesignConversionDispatchService(flowClient);

        var submit = await dispatchService.SubmitAsync(CreateDesignConversionCommand("flow-contract", "corr-contract"));
        var result = await dispatchService.GetJobAsync(submit.Data!.JobId);

        Assert.False(submit.IsFailure);
        Assert.False(result.IsFailure);
        Assert.Equal("/flow-47/design-conversion/jobs/dcj-status", handler.LastRequest?.RequestUri?.AbsolutePath);
        AssertHeader(handler.LastRequest, "X-Tenant-Id", "tenant-a");
        AssertHeader(handler.LastRequest, "X-Role", "platform_support");
        Assert.Equal(new AuthenticationHeaderValue("Bearer", "local-tenant-a-token"), handler.LastRequest?.Headers.Authorization);
        Assert.Equal("queued", result.Data?.Status);
        Assert.Equal("<main>hello</main>", result.Data?.CurrentHtml);
        Assert.Equal("main { color: #123456; }", result.Data?.CurrentCss);
        Assert.Equal("html", result.Data?.SourceKind);
        Assert.Equal("pro", result.Data?.DesignTier);
        Assert.Equal(3, result.Data?.LoopBudget);
        Assert.Equal("tenant-a", result.Data?.TenantId);
        Assert.Equal("user-a", result.Data?.UserId);
        Assert.Equal("flow-contract", result.Data?.IdempotencyKey);
    }

    [Fact]
    public async Task HttpFlowClientMapsCompletedFlow47EvaluationResult()
    {
        var now = DateTimeOffset.UtcNow;
        var handler = new CapturingFlow47Handler(request => new
        {
            jobId = "dcj-completed",
            tenantId = "tenant-a",
            status = "completed",
            idempotencyKey = "flow-completed",
            sourceType = "html",
            sourceUri = "inline://platform/design-conversion/html",
            outputTargets = new[] { "angular" },
            coreProviderJobId = "core-completed",
            error = (object?)null,
            createdAt = now,
            updatedAt = now,
            scaffoldNotice = (string?)null,
            evaluation = new
            {
                verdictId = "pass",
                passed = true,
                score = 0.94,
                currentHtml = "<main>converted</main>",
                currentCss = "main { color: #00ff00; }",
                defects = new object[]
                {
                    "minor-spacing",
                    new
                    {
                        code = "hero-offset",
                        message = "Hero content is shifted down by 12px.",
                        severity = "medium",
                        selector = ".hero"
                    }
                },
                comparisonArtifacts = new[]
                {
                    new
                    {
                        artifactId = "cmp-1",
                        expectedImageUri = "memory://cmp-1-expected",
                        actualImageUri = "memory://cmp-1-actual",
                        diffImageUri = "memory://cmp-1-diff",
                        score = 0.94
                    }
                },
                warnings = new[] { "fallback-font-used" },
                evidenceRefs = new[] { "evidence://flow47/dcj-completed/result.json" }
            }
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://flow47.local") };
        var flowClient = new HttpFlow47DesignConversionClient(httpClient, new Flow47DesignConversionOptions());

        var result = await flowClient.GetJobAsync("dcj-completed", "tenant-a");

        Assert.False(result.IsFailure);
        Assert.NotNull(result.Data);
        Assert.Equal("completed", result.Data.Status);
        Assert.Equal("<main>converted</main>", result.Data.CurrentHtml);
        Assert.Equal("main { color: #00ff00; }", result.Data.CurrentCss);
        Assert.Equal("pass", result.Data.Verdict?.Status);
        Assert.Equal(0.94, result.Data.Verdict?.Score);
        Assert.Contains("minor-spacing", result.Data.Defects);
        Assert.Contains(result.Data.Defects, defect =>
            defect.Contains("hero-offset", StringComparison.Ordinal) &&
            defect.Contains("Hero content is shifted down by 12px.", StringComparison.Ordinal) &&
            defect.Contains("severity: medium", StringComparison.Ordinal) &&
            defect.Contains("selector: .hero", StringComparison.Ordinal));
        Assert.Contains(result.Data.Verdict!.Findings, finding =>
            finding.Contains("hero-offset", StringComparison.Ordinal) &&
            finding.Contains("Hero content is shifted down by 12px.", StringComparison.Ordinal));
        Assert.Contains("fallback-font-used", result.Data.Warnings);
        Assert.Contains("evidence://flow47/dcj-completed/result.json", result.Data.EvidenceRefs);
        Assert.Contains(result.Data.ComparisonImages, comparison =>
            comparison.Id == "cmp-1:diff" &&
            comparison.Uri == "memory://cmp-1-diff" &&
            comparison.Label == "diff" &&
            comparison.SimilarityScore == 0.94);
        Assert.Contains(result.Data.ComparisonImages, comparison =>
            comparison.Id == "cmp-1:expected" &&
            comparison.Uri == "memory://cmp-1-expected" &&
            comparison.Label == "expected" &&
            comparison.SimilarityScore == 0.94);
        Assert.Contains(result.Data.ComparisonImages, comparison =>
            comparison.Id == "cmp-1:actual" &&
            comparison.Uri == "memory://cmp-1-actual" &&
            comparison.Label == "actual" &&
            comparison.SimilarityScore == 0.94);
    }

    [Fact]
    public async Task HttpFlowClientMapsFailedFlow47EvaluationResult()
    {
        var now = DateTimeOffset.UtcNow;
        var handler = new CapturingFlow47Handler(request => new
        {
            jobId = "dcj-failed",
            tenantId = "tenant-a",
            status = "failed",
            idempotencyKey = "flow-failed",
            sourceType = "html",
            sourceUri = "inline://platform/design-conversion/html",
            outputTargets = new[] { "angular" },
            coreProviderJobId = "core-failed",
            error = new { code = "FLOW47_EVALUATION_FAILED", message = "Evaluation worker failed." },
            createdAt = now,
            updatedAt = now,
            scaffoldNotice = (string?)null,
            result = new
            {
                status = "failed",
                errorSummary = "Renderer crashed before comparison.",
                defects = new[] { "render-crash" },
                warnings = new[] { "no-comparison-artifact" },
                evidenceRefs = new[] { "evidence://flow47/dcj-failed/error.json" }
            }
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://flow47.local") };
        var flowClient = new HttpFlow47DesignConversionClient(httpClient, new Flow47DesignConversionOptions());

        var result = await flowClient.GetJobAsync("dcj-failed", "tenant-a");

        Assert.False(result.IsFailure);
        Assert.NotNull(result.Data);
        Assert.Equal("failed", result.Data.Status);
        Assert.Equal("failed", result.Data.Verdict?.Status);
        Assert.Contains("render-crash", result.Data.Defects);
        Assert.Contains("no-comparison-artifact", result.Data.Warnings);
        Assert.Contains("evidence://flow47/dcj-failed/error.json", result.Data.EvidenceRefs);
        Assert.Contains(result.Data.Verdict!.Findings, finding => finding.Contains("Evaluation worker failed", StringComparison.Ordinal));
        Assert.Contains(result.Data.Verdict.Findings, finding => finding.Contains("Renderer crashed", StringComparison.Ordinal));
    }

    [Fact]
    public async Task HttpFlowClientMapsFlow47EvaluationPassedFalseToFailVerdict()
    {
        var now = DateTimeOffset.UtcNow;
        var handler = new CapturingFlow47Handler(request => new
        {
            jobId = "dcj-completed-fail",
            tenantId = "tenant-a",
            status = "completed",
            idempotencyKey = "flow-completed-fail",
            sourceType = "html",
            sourceUri = "inline://platform/design-conversion/html",
            outputTargets = new[] { "angular" },
            coreProviderJobId = "core-completed-fail",
            error = (object?)null,
            createdAt = now,
            updatedAt = now,
            scaffoldNotice = (string?)null,
            evaluation = new
            {
                passed = false,
                score = 0.41,
                currentHtml = "<main>converted with defects</main>",
                currentCss = "main { color: #ff0000; }",
                defects = new[] { "missing-primary-button" },
                warnings = new[] { "low-contrast" },
                comparisonArtifacts = new[]
                {
                    new
                    {
                        artifactId = "cmp-fail-1",
                        diffImageUri = "memory://cmp-fail-1-diff",
                        score = 0.41
                    }
                }
            }
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://flow47.local") };
        var flowClient = new HttpFlow47DesignConversionClient(httpClient, new Flow47DesignConversionOptions());

        var result = await flowClient.GetJobAsync("dcj-completed-fail", "tenant-a");

        Assert.False(result.IsFailure);
        Assert.NotNull(result.Data);
        Assert.Equal("completed", result.Data.Status);
        Assert.Equal("<main>converted with defects</main>", result.Data.CurrentHtml);
        Assert.Equal("main { color: #ff0000; }", result.Data.CurrentCss);
        Assert.Equal("fail", result.Data.Verdict?.Status);
        Assert.Equal(0.41, result.Data.Verdict?.Score);
        Assert.Contains("missing-primary-button", result.Data.Defects);
        Assert.Contains("low-contrast", result.Data.Warnings);
        var comparison = Assert.Single(result.Data.ComparisonImages);
        Assert.Equal("cmp-fail-1:diff", comparison.Id);
        Assert.Equal("memory://cmp-fail-1-diff", comparison.Uri);
        Assert.Equal(0.41, comparison.SimilarityScore);
    }

    [Fact]
    public async Task HttpFlowReadinessProbeReturnsReadyOnSuccess()
    {
        var handler = new ReadinessFlow47Handler(HttpStatusCode.OK);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://flow47.local") };
        var probe = new HttpFlow47DesignConversionReadinessProbe(httpClient, new Flow47DesignConversionOptions
        {
            BaseUrl = "https://flow47.local"
        });

        var result = await probe.CheckAsync();

        Assert.True(result.IsReady);
        Assert.Equal("external", result.Mode);
        Assert.Equal("/health/ready", handler.Requests[0].RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task HttpFlowReadinessProbeReturnsNotReadyOnFailure()
    {
        var handler = new ReadinessFlow47Handler(HttpStatusCode.ServiceUnavailable);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://flow47.local") };
        var probe = new HttpFlow47DesignConversionReadinessProbe(httpClient, new Flow47DesignConversionOptions
        {
            BaseUrl = "https://flow47.local"
        });

        var result = await probe.CheckAsync();

        Assert.False(result.IsReady);
        Assert.Equal("external", result.Mode);
        Assert.Equal(Flow47DesignConversionOptions.ReadinessProbeFailedCode, result.Code);
        Assert.Contains("HTTP 503", result.Message, StringComparison.Ordinal);
    }

    private WebApplicationFactory<Program> CreateFactoryWithFakeFlowClient(
        TrackingFakeFlow47DesignConversionClient fakeFlowClient) =>
        _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IFlow47DesignConversionClient>();
                services.AddSingleton<IFlow47DesignConversionClient>(fakeFlowClient);
            });
        });

    private static WebApplicationFactory<Program> CreateFactoryWithLocalFakeMode(
        WebApplicationFactory<Program> factory) =>
        factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Flow47:UseLocalFakeClient", "true");
        });

    private static object CreateSubmitRequest() => new
    {
        html = "<main>hello</main>",
        css = "main { color: #123456; }",
        sourceKind = "html",
        sourceTree = new[]
        {
            new { path = "index.html", kind = "html", content = "<main>hello</main>" }
        },
        screenshots = new[]
        {
            new { id = "shot-1", uri = "memory://shot-1", mimeType = "image/png", sha256 = "abc", width = 640, height = 480 }
        },
        metadata = new Dictionary<string, string>
        {
            ["source"] = "endpoint-test"
        }
    };

    private static Flow47DesignConversionJobRequest CreateFlowJobRequest(string idempotencyKey) =>
        new(
            "tenant-a",
            "user-a",
            idempotencyKey,
            "corr-contract",
            "etag-1",
            "html",
            "pro",
            3,
            "<main>hello</main>",
            "main { color: #123456; }",
            new[]
            {
                new DesignConversionSourceFile("index.html", "html", "<main>hello</main>")
            },
            new[]
            {
                new DesignConversionScreenshot("shot-1", "memory://shot-1", "image/png", "abc", 640, 480)
            },
            new Dictionary<string, string>
            {
                ["source"] = "endpoint-test"
            });

    private static DesignConversionSubmitCommand CreateDesignConversionCommand(
        string idempotencyKey,
        string correlationId) =>
        new(
            new DesignConversionSubmitRequest
            {
                Html = "<main>hello</main>",
                Css = "main { color: #123456; }",
                SourceKind = "html",
                SourceTree = new[]
                {
                    new DesignConversionSourceFile("index.html", "html", "<main>hello</main>")
                },
                Screenshots = new[]
                {
                    new DesignConversionScreenshot("shot-1", "memory://shot-1", "image/png", "abc", 640, 480)
                },
                Metadata = new Dictionary<string, string>
                {
                    ["source"] = "endpoint-test"
                }
            },
            "tenant-a",
            "user-a",
            idempotencyKey,
            correlationId,
            "etag-1",
            new DesignConversionTierContext("pro", 3));

    private static void AddAuthorizedHeaders(
        HttpClient client,
        string idempotencyKey,
        string designTier = "pro",
        string loopBudget = "3")
    {
        client.DefaultRequestHeaders.Add("X-Roles", "tenant_ops");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", "tenant-a");
        client.DefaultRequestHeaders.Add("X-User-Id", "user-a");
        client.DefaultRequestHeaders.Add("X-Design-Tier", designTier);
        client.DefaultRequestHeaders.Add("X-Loop-Budget", loopBudget);
        client.DefaultRequestHeaders.Add("X-Idempotency-Key", idempotencyKey);
    }

    private static async Task<JsonDocument> ReadObjectAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }

    private static void AssertDesignConversionResponseShape(JsonElement body)
    {
        Assert.Equal(JsonValueKind.Object, body.ValueKind);
        Assert.True(body.TryGetProperty("jobId", out _));
        Assert.True(body.TryGetProperty("status", out _));
        Assert.True(body.TryGetProperty("currentHtml", out _));
        Assert.True(body.TryGetProperty("currentCss", out _));
        Assert.True(body.TryGetProperty("comparisonImages", out var comparisonImages));
        Assert.Equal(JsonValueKind.Array, comparisonImages.ValueKind);
        Assert.True(body.TryGetProperty("verdict", out var verdict));
        Assert.True(verdict.TryGetProperty("status", out _));
        Assert.True(verdict.TryGetProperty("score", out _));
        Assert.True(verdict.TryGetProperty("findings", out var findings));
        Assert.Equal(JsonValueKind.Array, findings.ValueKind);
        Assert.True(body.TryGetProperty("sourceKind", out _));
        Assert.True(body.TryGetProperty("designTier", out _));
        Assert.True(body.TryGetProperty("loopBudget", out _));
        Assert.True(body.TryGetProperty("tenantId", out _));
        Assert.True(body.TryGetProperty("userId", out _));
        Assert.True(body.TryGetProperty("idempotencyKey", out _));
        Assert.True(body.TryGetProperty("createdAt", out _));
        Assert.True(body.TryGetProperty("updatedAt", out _));
        Assert.True(body.TryGetProperty("defects", out var defects));
        Assert.Equal(JsonValueKind.Array, defects.ValueKind);
        Assert.True(body.TryGetProperty("warnings", out var warnings));
        Assert.Equal(JsonValueKind.Array, warnings.ValueKind);
        Assert.True(body.TryGetProperty("evidenceRefs", out var evidenceRefs));
        Assert.Equal(JsonValueKind.Array, evidenceRefs.ValueKind);
        Assert.Equal(17, body.EnumerateObject().Count());
    }

    private static void AssertHeader(HttpRequestMessage? request, string name, string expected)
    {
        Assert.NotNull(request);
        Assert.True(request!.Headers.TryGetValues(name, out var values), $"Missing header {name}.");
        Assert.Contains(expected, values);
    }

    private sealed class TrackingFakeFlow47DesignConversionClient : IFlow47DesignConversionClient
    {
        private readonly Dictionary<string, DesignConversionJobResponse> _jobs = new(StringComparer.Ordinal);

        public int SubmitCount { get; private set; }
        public int StatusCount { get; private set; }
        public Flow47DesignConversionJobRequest? LastSubmitRequest { get; private set; }

        public Task<DataProcessResult<DesignConversionJobResponse>> SubmitAsync(
            Flow47DesignConversionJobRequest request,
            CancellationToken cancellationToken = default)
        {
            SubmitCount++;
            LastSubmitRequest = request;
            var now = DateTimeOffset.UtcNow;
            var response = new DesignConversionJobResponse(
                $"tracking_fake_flow47_job_{SubmitCount}",
                "accepted",
                request.Html,
                request.Css,
                Array.Empty<DesignConversionComparisonImage>(),
                new DesignConversionVerdict("pending", null, new[] { "Tracking fake FLOW-47 client accepted the request." }),
                request.SourceKind,
                request.DesignTier,
                request.LoopBudget,
                request.TenantId,
                request.UserId,
                request.IdempotencyKey,
                now,
                now);
            _jobs[response.JobId] = response;
            return Task.FromResult(DataProcessResult<DesignConversionJobResponse>.Success(response));
        }

        public Task<DataProcessResult<DesignConversionJobResponse?>> GetJobAsync(
            string jobId,
            string? tenantId = null,
            string? correlationId = null,
            CancellationToken cancellationToken = default)
        {
            StatusCount++;
            _jobs.TryGetValue(jobId, out var job);
            return Task.FromResult(DataProcessResult<DesignConversionJobResponse?>.Success(job));
        }
    }

    private sealed class CapturingFlow47Handler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, object> _responseFactory;

        public CapturingFlow47Handler(Func<HttpRequestMessage, object> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            return new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = JsonContent.Create(_responseFactory(request))
            };
        }
    }

    private sealed class ReadinessFlow47Handler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public ReadinessFlow47Handler(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        public List<HttpRequestMessage> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = JsonContent.Create(new { status = _statusCode == HttpStatusCode.OK ? "ready" : "not-ready" })
            });
        }
    }
}
