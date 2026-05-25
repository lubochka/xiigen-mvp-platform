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
}
