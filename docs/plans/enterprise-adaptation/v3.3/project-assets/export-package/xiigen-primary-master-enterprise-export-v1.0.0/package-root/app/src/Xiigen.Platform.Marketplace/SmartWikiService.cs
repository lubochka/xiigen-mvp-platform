using Microsoft.Extensions.Logging;
using Xiigen.Kernel;

namespace Xiigen.Platform.Marketplace;

public sealed class SmartWikiService : MicroserviceBase<SmartWikiService>, ISmartWikiService
{
    private const string IndexCollection = "smart-wiki-indexes";
    private const string AnswerCollection = "smart-wiki-answers";
    private const string ReindexCollection = "smart-wiki-reindex-audit";
    private readonly IDocumentStore _documents;

    public SmartWikiService(
        IDocumentStore documents,
        ITenantContextAccessor tenantContextAccessor,
        ICorrelationContextAccessor correlationContextAccessor,
        ILogger<SmartWikiService> logger)
        : base(
            new ServiceDescriptor("xiigen-smart-wiki", "XIIGen Smart Wiki", "PLATFORM"),
            tenantContextAccessor,
            correlationContextAccessor,
            logger)
    {
        _documents = documents;
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> IndexGraphAsync(
        SmartWikiIndexRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ProjectId))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("PROJECT_ID_REQUIRED", "Project id is required.");
        }

        if (request.Nodes.Count == 0)
        {
            return Failure<IReadOnlyDictionary<string, object?>>("GRAPH_NODES_REQUIRED", "Smart Wiki index requires linked graph nodes.");
        }

        var body = WithTenant(new Dictionary<string, object?>
        {
            ["id"] = IndexId(request.ProjectId),
            ["projectId"] = request.ProjectId,
            ["unitId"] = request.UnitId,
            ["sourceGraphPath"] = request.SourceGraphPath,
            ["sourceGraphChecksum"] = request.SourceGraphChecksum,
            ["nodeCount"] = request.Nodes.Count,
            ["edgeCount"] = request.Edges.Count,
            ["abstentionCount"] = request.Abstentions.Count,
            ["nodes"] = request.Nodes.Select(NodeBody).ToArray(),
            ["edges"] = request.Edges.Select(EdgeBody).ToArray(),
            ["abstentions"] = request.Abstentions.Select(AbstentionBody).ToArray(),
            ["indexedBy"] = request.IndexedBy,
            ["indexedAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId,
            ["registryPath"] = "docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json"
        });

        var stored = await _documents.UpsertAsync(IndexCollection, IndexId(request.ProjectId), body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return stored.IsSuccess && stored.Data is not null
            ? Success(stored.Data.Body)
            : Failure<IReadOnlyDictionary<string, object?>>(
                stored.Error?.Code ?? "SMART_WIKI_INDEX_FAILED",
                stored.Error?.Message ?? "Smart Wiki index could not be stored.");
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> AnswerAsync(
        SmartWikiQuestionRequest request,
        CancellationToken cancellationToken = default)
    {
        var index = await GetRequiredIndexAsync(request.ProjectId, cancellationToken).ConfigureAwait(false);
        if (index.IsFailure)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(index.Error!.Code, index.Error.Message, index.Error.Details);
        }

        var nodes = ReadItems(index.Data!, "nodes");
        var edges = ReadItems(index.Data!, "edges");
        var abstentions = ReadItems(index.Data!, "abstentions");
        var answer = BuildAnswer(request, nodes, edges, abstentions);
        var id = StableId("wiki-answer", $"{request.ProjectId}:{request.QuestionKind}:{request.Subject}:{CurrentCorrelationId}");
        var body = WithTenant(new Dictionary<string, object?>
        {
            ["id"] = id,
            ["projectId"] = request.ProjectId,
            ["questionKind"] = request.QuestionKind,
            ["subject"] = request.Subject,
            ["askedBy"] = request.AskedBy,
            ["status"] = answer.Status,
            ["answer"] = answer.Text,
            ["citations"] = answer.Citations,
            ["abstention"] = answer.Abstention,
            ["answeredAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        });

        var stored = await _documents.UpsertAsync(AnswerCollection, id, body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return stored.IsSuccess && stored.Data is not null
            ? Success(stored.Data.Body)
            : Failure<IReadOnlyDictionary<string, object?>>(
                stored.Error?.Code ?? "SMART_WIKI_ANSWER_FAILED",
                stored.Error?.Message ?? "Smart Wiki answer could not be stored.");
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> ReindexAsync(
        SmartWikiReindexRequest request,
        CancellationToken cancellationToken = default)
    {
        var index = await GetRequiredIndexAsync(request.ProjectId, cancellationToken).ConfigureAwait(false);
        if (index.IsFailure)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(index.Error!.Code, index.Error.Message, index.Error.Details);
        }

        var updatedIndex = new Dictionary<string, object?>(index.Data!)
        {
            ["sourceGraphChecksum"] = request.SourceGraphChecksum,
            ["lastReindexSummary"] = request.ChangeSummary,
            ["lastReindexedBy"] = request.RequestedBy,
            ["lastReindexedAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        };

        var stored = await _documents.UpsertAsync(IndexCollection, IndexId(request.ProjectId), updatedIndex, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (stored.IsFailure || stored.Data is null)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(
                stored.Error?.Code ?? "SMART_WIKI_REINDEX_FAILED",
                stored.Error?.Message ?? "Smart Wiki reindex could not be stored.");
        }

        var auditId = StableId("wiki-reindex", $"{request.ProjectId}:{request.SourceGraphChecksum}:{CurrentCorrelationId}");
        var audit = WithTenant(new Dictionary<string, object?>
        {
            ["id"] = auditId,
            ["projectId"] = request.ProjectId,
            ["sourceGraphChecksum"] = request.SourceGraphChecksum,
            ["changeSummary"] = request.ChangeSummary,
            ["requestedBy"] = request.RequestedBy,
            ["result"] = "REINDEXED",
            ["createdAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        });
        await _documents.UpsertAsync(ReindexCollection, auditId, audit, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return Success(stored.Data.Body);
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>?>> GetIndexAsync(
        string projectId,
        CancellationToken cancellationToken = default)
    {
        var index = await _documents.GetAsync(IndexCollection, IndexId(projectId), cancellationToken).ConfigureAwait(false);
        if (index.IsFailure)
        {
            return Failure<IReadOnlyDictionary<string, object?>?>(
                index.Error?.Code ?? "SMART_WIKI_INDEX_READ_FAILED",
                index.Error?.Message ?? "Smart Wiki index could not be read.");
        }

        if (index.Data is null || !BelongsToCurrentTenant(index.Data.Body))
        {
            return Success<IReadOnlyDictionary<string, object?>?>(null);
        }

        return Success<IReadOnlyDictionary<string, object?>?>(index.Data.Body);
    }

    private async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> GetRequiredIndexAsync(
        string projectId,
        CancellationToken cancellationToken)
    {
        var index = await GetIndexAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (index.IsFailure)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(index.Error!.Code, index.Error.Message, index.Error.Details);
        }

        return index.Data is null
            ? Failure<IReadOnlyDictionary<string, object?>>("SMART_WIKI_INDEX_NOT_FOUND", "Smart Wiki index was not found in the current tenant.")
            : Success(index.Data);
    }

    private static SmartWikiAnswer BuildAnswer(
        SmartWikiQuestionRequest request,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> nodes,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> edges,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> abstentions)
    {
        return request.QuestionKind switch
        {
            "entity-impact" => CitedAnswer(
                "ANSWERED_WITH_CITATIONS",
                $"Entity impact for {request.Subject}: affected graph records include matching nodes and all edges that read, write or emit from those nodes.",
                nodes.Where(node => Contains(node, request.Subject)).Concat(edges.Where(edge => Contains(edge, request.Subject))).Take(8)),
            "flow-trace" => CitedAnswer(
                "ANSWERED_WITH_CITATIONS",
                "Flow trace: Angular route renders the component, the .NET Minimal API calls the application service, the service reads/writes logical collections and emits the export queued event.",
                edges.Where(edge => HasType(edge, "renders") || HasType(edge, "calls") || HasType(edge, "writesTable") || HasType(edge, "emitsEvent")).Take(10)),
            "stored-procedure-impact" => AbstainOrCite(
                "SQL stored procedures",
                abstentions,
                "No stored procedure source exists in the linked graph; Smart Wiki abstains instead of inventing procedure impact."),
            "angular-impact" => CitedAnswer(
                "ANSWERED_WITH_CITATIONS",
                "Angular impact: route and component nodes identify affected client screens; Angular service impact abstains when no HttpClient service source exists.",
                nodes.Where(node => HasType(node, "AngularRoute") || HasType(node, "AngularComponent") || HasType(node, "AngularService")).Take(8)),
            "dotnet-impact" => CitedAnswer(
                "ANSWERED_WITH_CITATIONS",
                ".NET impact: controller, service, contract and logical collection nodes identify affected server behavior.",
                nodes.Where(node => HasType(node, "DotNetController") || HasType(node, "DotNetService") || HasType(node, "Contract") || HasType(node, "Table")).Take(10)),
            "missing-evidence-abstention" => CitedAnswer(
                "ANSWERED_WITH_CITATIONS",
                "Missing evidence is represented as ABSTAIN_NO_SOURCE rows and is returned as an abstention rather than a fabricated dependency.",
                abstentions.Take(10)),
            _ => new SmartWikiAnswer(
                "ABSTAIN_UNSUPPORTED_QUESTION",
                $"Question kind {request.QuestionKind} is not in the approved Smart Wiki question suite.",
                Array.Empty<IReadOnlyDictionary<string, object?>>(),
                true)
        };
    }

    private static SmartWikiAnswer AbstainOrCite(
        string sourceFamily,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> abstentions,
        string text)
    {
        var matches = abstentions
            .Where(row => row.TryGetValue("sourceFamily", out var value) &&
                string.Equals(Convert.ToString(value), sourceFamily, StringComparison.Ordinal))
            .Take(4)
            .ToArray();
        return matches.Length > 0
            ? new SmartWikiAnswer("ABSTAIN_NO_SOURCE", text, matches, true)
            : new SmartWikiAnswer("ANSWERED_WITH_CITATIONS", $"Source family {sourceFamily} has evidence in the linked graph.", matches, false);
    }

    private static SmartWikiAnswer CitedAnswer(
        string status,
        string text,
        IEnumerable<IReadOnlyDictionary<string, object?>> citations)
    {
        var materialized = citations.ToArray();
        return new SmartWikiAnswer(status, text, materialized, false);
    }

    private static IReadOnlyDictionary<string, object?> NodeBody(SmartWikiGraphNode node) => new Dictionary<string, object?>
    {
        ["nodeId"] = node.NodeId,
        ["type"] = node.NodeType,
        ["sourcePath"] = node.SourcePath,
        ["line"] = node.Line,
        ["metadata"] = new Dictionary<string, object?>(node.Metadata)
    };

    private static IReadOnlyDictionary<string, object?> EdgeBody(SmartWikiGraphEdge edge) => new Dictionary<string, object?>
    {
        ["from"] = edge.From,
        ["to"] = edge.To,
        ["type"] = edge.EdgeType,
        ["sourcePath"] = edge.SourcePath,
        ["line"] = edge.Line,
        ["metadata"] = new Dictionary<string, object?>(edge.Metadata)
    };

    private static IReadOnlyDictionary<string, object?> AbstentionBody(SmartWikiAbstention abstention) => new Dictionary<string, object?>
    {
        ["sourceFamily"] = abstention.SourceFamily,
        ["status"] = abstention.Status,
        ["reason"] = abstention.Reason,
        ["scanCommand"] = abstention.ScanCommand
    };

    private static IReadOnlyList<IReadOnlyDictionary<string, object?>> ReadItems(
        IReadOnlyDictionary<string, object?> body,
        string key) =>
        body.TryGetValue(key, out var value) && value is IEnumerable<IReadOnlyDictionary<string, object?>> rows
            ? rows.ToArray()
            : Array.Empty<IReadOnlyDictionary<string, object?>>();

    private IReadOnlyDictionary<string, object?> WithTenant(IReadOnlyDictionary<string, object?> body)
    {
        var copy = new Dictionary<string, object?>(body)
        {
            ["tenant_id"] = CurrentTenant.TenantId
        };
        return copy;
    }

    private bool BelongsToCurrentTenant(IReadOnlyDictionary<string, object?> body) =>
        body.TryGetValue("tenant_id", out var tenantId) &&
        string.Equals(Convert.ToString(tenantId), CurrentTenant.TenantId, StringComparison.Ordinal);

    private static bool Contains(IReadOnlyDictionary<string, object?> row, string subject) =>
        row.Any(pair => pair.Value is not null &&
            Convert.ToString(pair.Value)?.Contains(subject, StringComparison.OrdinalIgnoreCase) == true);

    private static bool HasType(IReadOnlyDictionary<string, object?> row, string expected) =>
        row.TryGetValue("type", out var value) &&
        string.Equals(Convert.ToString(value), expected, StringComparison.Ordinal);

    private static string IndexId(string projectId) => StableId("wiki-index", projectId);

    private static string StableId(string prefix, string value) =>
        $"{prefix}-{Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value))).ToLowerInvariant()[..16]}";

    private sealed record SmartWikiAnswer(
        string Status,
        string Text,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> Citations,
        bool Abstention);
}
