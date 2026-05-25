using Microsoft.Extensions.Logging.Abstractions;
using Xiigen.Fabrics.InMemory;
using Xiigen.Kernel;
using Xiigen.Platform.Marketplace;

namespace Xiigen.Platform.Marketplace.Tests;

public sealed class SmartWikiServiceTests
{
    [Fact]
    public async Task IndexGraphAsync_StoresLinkedGraphAsTenantLocalWikiData()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());

        var indexed = await harness.Service.IndexGraphAsync(SampleIndexRequest());
        var loaded = await harness.Service.GetIndexAsync("xiigen-enterprise-master");

        Assert.True(indexed.IsSuccess);
        Assert.True(loaded.IsSuccess);
        Assert.Equal("primary-master-tenant", loaded.Data?["tenant_id"]);
        Assert.Equal(8, loaded.Data?["nodeCount"]);
        Assert.Equal("docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json", loaded.Data?["registryPath"]);
    }

    [Theory]
    [InlineData("entity-impact", "xiigen-flow19-compliance-records", "ANSWERED_WITH_CITATIONS")]
    [InlineData("flow-trace", "saga start", "ANSWERED_WITH_CITATIONS")]
    [InlineData("angular-impact", "route", "ANSWERED_WITH_CITATIONS")]
    [InlineData("dotnet-impact", "Program", "ANSWERED_WITH_CITATIONS")]
    [InlineData("missing-evidence-abstention", "SQL", "ANSWERED_WITH_CITATIONS")]
    public async Task AnswerAsync_ReturnsCitedAnswersForAdvancedQuestions(
        string questionKind,
        string subject,
        string expectedStatus)
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());
        await harness.Service.IndexGraphAsync(SampleIndexRequest());

        var answer = await harness.Service.AnswerAsync(new SmartWikiQuestionRequest(
            "xiigen-enterprise-master",
            questionKind,
            subject,
            "tenant-admin"));

        Assert.True(answer.IsSuccess);
        Assert.Equal(expectedStatus, answer.Data?["status"]);
        Assert.NotEmpty((IReadOnlyList<IReadOnlyDictionary<string, object?>>)answer.Data!["citations"]!);
    }

    [Fact]
    public async Task AnswerAsync_AbstainsForStoredProcedureImpactWhenSqlEvidenceIsMissing()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());
        await harness.Service.IndexGraphAsync(SampleIndexRequest());

        var answer = await harness.Service.AnswerAsync(new SmartWikiQuestionRequest(
            "xiigen-enterprise-master",
            "stored-procedure-impact",
            "sp_UpdateSaga",
            "tenant-admin"));

        Assert.True(answer.IsSuccess);
        Assert.Equal("ABSTAIN_NO_SOURCE", answer.Data?["status"]);
        Assert.True((bool)answer.Data!["abstention"]!);
    }

    [Fact]
    public async Task ReindexAsync_RecordsGraphChecksumChange()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());
        await harness.Service.IndexGraphAsync(SampleIndexRequest());

        var reindexed = await harness.Service.ReindexAsync(new SmartWikiReindexRequest(
            "xiigen-enterprise-master",
            "checksum-after-route-change",
            "Angular route and .NET endpoint graph changed.",
            "tenant-admin"));

        Assert.True(reindexed.IsSuccess);
        Assert.Equal("checksum-after-route-change", reindexed.Data?["sourceGraphChecksum"]);
        Assert.Equal("Angular route and .NET endpoint graph changed.", reindexed.Data?["lastReindexSummary"]);
    }

    [Fact]
    public async Task GetIndexAsync_ReturnsNullForCrossTenantAccess()
    {
        var harness = CreateHarness();
        using (harness.Tenants.Push(TestTenant()))
        using (harness.Correlations.Push(TestCorrelation()))
        {
            await harness.Service.IndexGraphAsync(SampleIndexRequest());
        }

        using var tenant = harness.Tenants.Push(TestTenant("other-tenant"));
        using var correlation = harness.Correlations.Push(TestCorrelation());

        var index = await harness.Service.GetIndexAsync("xiigen-enterprise-master");

        Assert.True(index.IsSuccess);
        Assert.Null(index.Data);
    }

    private static SmartWikiIndexRequest SampleIndexRequest() => new(
        "xiigen-enterprise-master",
        "primary-master-tenant.xiigen-enterprise-master.project-local-wiki",
        "docs/plans/enterprise-adaptation/v3.3/evidence/linked-system-graph.json",
        "5a6e0322a53d85b8650c11a40fd061ffcb141402e0b89afdc38c60c6d4a7300a",
        new[]
        {
            Node("angular-route:questionnaire", "AngularRoute", "client/angular/src/app/app.routes.ts", 6, ("path", "durable-sagas-compliance/questionnaire")),
            Node("angular-component:App", "AngularComponent", "client/angular/src/app/app.ts", 39, ("selector", "app-root")),
            Node("angular-service:none", "AngularService", "client/angular/src/app", null, ("status", "ABSTAIN_NO_SOURCE")),
            Node("dotnet-controller:Program", "DotNetController", "server/Xiigen.Flow.DurableSagasCompliance.Api/Program.cs", 42, ("route", "/api/dynamic/durable-sagas-compliance")),
            Node("dotnet-service:DurableSagasComplianceService", "DotNetService", "server/Xiigen.Flow.DurableSagasCompliance.Application/DurableSagasComplianceService.cs", 13, ("serviceClass", "DurableSagasComplianceService")),
            Node("contract:request-schema", "Contract", "contracts/schemas/durable-sagas-compliance-request.schema.json", 1, ("contractKind", "json-schema-request")),
            Node("table:xiigen-flow19-compliance-records", "Table", "server/Xiigen.Flow.DurableSagasCompliance.Application/DurableSagasComplianceConstants.cs", 13, ("logicalName", "xiigen-flow19-compliance-records")),
            Node("stored-procedure:none", "StoredProcedure", "flows/FLOW-19-durable-sagas-compliance", null, ("status", "ABSTAIN_NO_SOURCE"))
        },
        new[]
        {
            Edge("angular-route:questionnaire", "angular-component:App", "renders", "client/angular/src/app/app.routes.ts", 6),
            Edge("dotnet-controller:Program", "dotnet-service:DurableSagasComplianceService", "calls", "server/Xiigen.Flow.DurableSagasCompliance.Api/Program.cs", 62),
            Edge("dotnet-controller:Program", "contract:request-schema", "usesDto", "contracts/schemas/durable-sagas-compliance-request.schema.json", 1),
            Edge("dotnet-service:DurableSagasComplianceService", "table:xiigen-flow19-compliance-records", "writesTable", "server/Xiigen.Flow.DurableSagasCompliance.Application/DurableSagasComplianceService.cs", 336),
            Edge("dotnet-service:DurableSagasComplianceService", "job:export-queued", "emitsEvent", "server/Xiigen.Flow.DurableSagasCompliance.Application/DurableSagasComplianceService.cs", 471)
        },
        new[]
        {
            new SmartWikiAbstention(
                "SQL stored procedures",
                "ABSTAIN_NO_SOURCE",
                "No CREATE PROCEDURE, CREATE PROC, stored procedure or DbContext source exists for FLOW-19.",
                "rg -n \"CREATE PROCEDURE|CREATE PROC|stored procedure|DbContext\" C:\\Projects\\xiigen-flows-dot-net-angular\\flows\\FLOW-19-durable-sagas-compliance")
        },
        "tenant-admin");

    private static SmartWikiGraphNode Node(
        string nodeId,
        string nodeType,
        string sourcePath,
        int? line,
        (string Key, object? Value) metadata) =>
        new(nodeId, nodeType, sourcePath, line, new Dictionary<string, object?> { [metadata.Key] = metadata.Value });

    private static SmartWikiGraphEdge Edge(
        string from,
        string to,
        string edgeType,
        string sourcePath,
        int? line) =>
        new(from, to, edgeType, sourcePath, line, new Dictionary<string, object?>());

    private static Harness CreateHarness()
    {
        var tenants = new AsyncLocalTenantContextAccessor();
        var correlations = new AsyncLocalCorrelationContextAccessor();
        var service = new SmartWikiService(
            new InMemoryDocumentStore(),
            tenants,
            correlations,
            NullLogger<SmartWikiService>.Instance);
        return new Harness(service, tenants, correlations);
    }

    private static TenantContext TestTenant(string tenantId = "primary-master-tenant") => new(
        tenantId,
        "tenant-admin",
        new HashSet<string>(StringComparer.Ordinal) { "tenant_admin" },
        "corr-smart-wiki-test",
        "test",
        DateTimeOffset.UtcNow);

    private static CorrelationContext TestCorrelation() => new(
        "corr-smart-wiki-test",
        "test",
        DateTimeOffset.UtcNow);

    private sealed record Harness(
        SmartWikiService Service,
        ITenantContextAccessor Tenants,
        ICorrelationContextAccessor Correlations);
}
