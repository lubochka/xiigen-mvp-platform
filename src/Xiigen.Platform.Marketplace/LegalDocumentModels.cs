namespace Xiigen.Platform.Marketplace;

public enum LegalDocumentStatus
{
    Draft,
    LegalReviewRequired,
    Approved,
    Published,
    Superseded
}

public sealed record LegalDocument(
    string Id,
    string Slug,
    string DocumentType,
    string Version,
    LegalDocumentStatus Status,
    string Locale,
    string CanonicalLocale,
    string Title,
    string Body,
    string Summary,
    DateOnly? EffectiveDate,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAtUtc,
    string Owner,
    string? PreviousVersionId,
    string CanonicalUrl,
    IReadOnlyList<string> RequiredAcceptanceContexts,
    IReadOnlyList<string> RelatedSiteSurfaces,
    string EvidencePath);

public sealed record LegalAcceptanceRecord(
    string UserId,
    string TenantId,
    string DocumentId,
    string DocumentType,
    string Version,
    string Locale,
    DateTimeOffset AcceptedAtUtc,
    string AcceptanceContext,
    string IpHashOrAuditReference,
    string UserAgentHashOrAuditReference);

public static class LegalPublicationPolicy
{
    public static bool CanPublish(LegalDocument document) =>
        document.Status is LegalDocumentStatus.Approved or LegalDocumentStatus.Published;

    public static bool HasApprovalMetadata(LegalDocument document) =>
        document.EffectiveDate is not null &&
        !string.IsNullOrWhiteSpace(document.ApprovedBy) &&
        document.ApprovedAtUtc is not null;
}

public enum LegalRoutePublicationStatus
{
    LegalReviewRequired,
    Published
}

public sealed record LegalRouteDocumentView(
    string Slug,
    string DocumentType,
    string Locale,
    string Title,
    string Summary,
    LegalRoutePublicationStatus PublicationStatus,
    bool CanPublish,
    string? Body,
    string CanonicalUrl,
    IReadOnlyList<string> RequiredAcceptanceContexts,
    IReadOnlyList<string> RelatedSiteSurfaces,
    string EvidencePath,
    string Message);

public interface ILegalPublicationService
{
    Task<Xiigen.Kernel.DataProcessResult<LegalRouteDocumentView>> GetRouteAsync(
        string slug,
        string locale,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<LegalDocument>> UpsertDocumentAsync(
        LegalDocument document,
        CancellationToken cancellationToken = default);
}
