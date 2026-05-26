using Microsoft.Extensions.Logging;
using Xiigen.Kernel;

namespace Xiigen.Platform.Marketplace;

public sealed class LegalPublicationService : MicroserviceBase<LegalPublicationService>, ILegalPublicationService
{
    private const string LegalDocumentsCollection = "legal-documents";

    private static readonly IReadOnlyDictionary<string, LegalRouteTemplate> Routes =
        new Dictionary<string, LegalRouteTemplate>(StringComparer.Ordinal)
        {
            ["terms-of-use"] = new("terms_of_service", "Terms of Use", "Terms governing account creation and use of XIIGen.", new[] { "signup", "accountCreation" }, new[] { "footer", "signup" }),
            ["privacy-policy"] = new("privacy_policy", "Privacy Policy", "Privacy and data handling disclosures for XIIGen.", new[] { "signup", "analyticsCookies" }, new[] { "footer", "signup", "cookieBanner" }),
            ["cookie-policy"] = new("cookie_policy", "Cookie Policy", "Cookie and tracking technology notice.", new[] { "cookiePreferences" }, new[] { "footer", "cookieBanner" }),
            ["end-user-license-agreement"] = new("eula", "End User License Agreement", "License terms for downloaded XIIGen packages.", new[] { "downloadInstall" }, new[] { "download", "install" }),
            ["enterprise-terms"] = new("enterprise_terms_msa", "Enterprise Terms", "Enterprise terms for corporate customers.", new[] { "corporateExport", "salesHandoff" }, new[] { "pricing", "corporateInstall" }),
            ["data-processing-addendum"] = new("dpa", "Data Processing Addendum", "Data processing terms for customer tenant data.", new[] { "corporateInstall" }, new[] { "privacy", "corporateInstall" }),
            ["acceptable-use-policy"] = new("acceptable_use", "Acceptable Use Policy", "Acceptable use requirements for XIIGen workspaces.", new[] { "signup", "affiliateApply" }, new[] { "footer", "signup", "affiliate" }),
            ["affiliate-program-terms"] = new("affiliate_terms", "Affiliate Program Terms", "Affiliate program terms and training obligations.", new[] { "affiliateApply" }, new[] { "affiliateApplication", "affiliateDashboard" }),
            ["refund-cancellation-policy"] = new("refund_cancellation", "Refund and Cancellation Policy", "Refund and cancellation terms for checkout.", new[] { "checkout" }, new[] { "pricing", "checkout" }),
            ["security-responsible-disclosure"] = new("security", "Security and Responsible Disclosure", "Security reporting and responsible disclosure policy.", new[] { "securityReport" }, new[] { "footer", "security" }),
            ["subprocessors-infrastructure"] = new("subprocessors", "Subprocessors and Infrastructure", "Subprocessor and infrastructure disclosure.", new[] { "corporateInstall" }, new[] { "privacy", "dpa", "corporateInstall" }),
            ["open-source-third-party-notices"] = new("open_source_notices", "Open Source and Third-Party Notices", "Open-source and third-party notices for export packages.", new[] { "downloadInstall", "exportDownload" }, new[] { "download", "exportPackage" })
        };

    private readonly IDocumentStore _documents;

    public LegalPublicationService(
        IDocumentStore documents,
        ITenantContextAccessor tenantContextAccessor,
        ICorrelationContextAccessor correlationContextAccessor,
        ILogger<LegalPublicationService> logger)
        : base(
            new ServiceDescriptor("xiigen-platform-legal-publication", "XIIGen Legal Publication", "PLATFORM"),
            tenantContextAccessor,
            correlationContextAccessor,
            logger)
    {
        _documents = documents;
    }

    public async Task<DataProcessResult<LegalRouteDocumentView>> GetRouteAsync(
        string slug,
        string locale,
        CancellationToken cancellationToken = default)
    {
        if (!Routes.TryGetValue(slug, out var template))
        {
            return Failure<LegalRouteDocumentView>("LEGAL_ROUTE_NOT_FOUND", "Legal route is not registered.");
        }

        var normalizedLocale = NormalizeLocale(locale);
        if (normalizedLocale is null)
        {
            return Failure<LegalRouteDocumentView>("LEGAL_LOCALE_UNSUPPORTED", "Legal route locale must be en, he or ru.");
        }

        var documentId = DocumentId(slug, normalizedLocale);
        var loaded = await _documents.GetAsync(LegalDocumentsCollection, documentId, cancellationToken)
            .ConfigureAwait(false);
        if (loaded.IsFailure)
        {
            return Failure<LegalRouteDocumentView>(
                loaded.Error?.Code ?? "LEGAL_DOCUMENT_READ_FAILED",
                loaded.Error?.Message ?? "Legal document could not be read.");
        }

        if (loaded.Data is null)
        {
            return Success(BlockedView(slug, normalizedLocale, template, "Final legal content is pending legal review and localization."));
        }

        var document = FromDictionary(loaded.Data.Body);
        if (!LegalPublicationPolicy.CanPublish(document) || !LegalPublicationPolicy.HasApprovalMetadata(document))
        {
            return Success(BlockedView(slug, normalizedLocale, template, "This legal document is not approved for public publication."));
        }

        return Success(new LegalRouteDocumentView(
            slug,
            document.DocumentType,
            normalizedLocale,
            document.Title,
            document.Summary,
            LegalRoutePublicationStatus.Published,
            true,
            document.Body,
            document.CanonicalUrl,
            document.RequiredAcceptanceContexts,
            document.RelatedSiteSurfaces,
            document.EvidencePath,
            "Published legal document."));
    }

    public async Task<DataProcessResult<LegalDocument>> UpsertDocumentAsync(
        LegalDocument document,
        CancellationToken cancellationToken = default)
    {
        if (LegalPublicationPolicy.CanPublish(document) && !LegalPublicationPolicy.HasApprovalMetadata(document))
        {
            return Failure<LegalDocument>("LEGAL_APPROVAL_METADATA_REQUIRED", "Approved or published legal documents require effective date, approvedBy and approvedAtUtc.");
        }

        if (!Routes.ContainsKey(document.Slug))
        {
            return Failure<LegalDocument>("LEGAL_ROUTE_NOT_REGISTERED", "Legal document slug is not registered as a public route.");
        }

        var normalizedLocale = NormalizeLocale(document.Locale);
        if (normalizedLocale is null)
        {
            return Failure<LegalDocument>("LEGAL_LOCALE_UNSUPPORTED", "Legal document locale must be en, he or ru.");
        }

        var stored = await _documents.UpsertAsync(
                LegalDocumentsCollection,
                DocumentId(document.Slug, normalizedLocale),
                ToDictionary(document with { Locale = normalizedLocale }),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return stored.IsSuccess
            ? Success(document with { Locale = normalizedLocale })
            : Failure<LegalDocument>(
                stored.Error?.Code ?? "LEGAL_DOCUMENT_STORE_FAILED",
                stored.Error?.Message ?? "Legal document could not be stored.");
    }

    private static LegalRouteDocumentView BlockedView(string slug, string locale, LegalRouteTemplate template, string message) => new(
        slug,
        template.DocumentType,
        locale,
        template.Title,
        template.Summary,
        LegalRoutePublicationStatus.LegalReviewRequired,
        false,
        null,
        $"https://gyxwjim5ea.eu-west-1.awsapprunner.com/legal/{slug}",
        template.RequiredAcceptanceContexts,
        template.RelatedSiteSurfaces,
        "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-publication-completion-evidence.json",
        message);

    private static string? NormalizeLocale(string locale)
    {
        var normalized = locale.Trim().ToLowerInvariant();
        return normalized is "en" or "he" or "ru" ? normalized : null;
    }

    private static string DocumentId(string slug, string locale) => $"{slug}:{locale}";

    private static Dictionary<string, object?> ToDictionary(LegalDocument document) => new()
    {
        ["id"] = document.Id,
        ["slug"] = document.Slug,
        ["documentType"] = document.DocumentType,
        ["version"] = document.Version,
        ["status"] = document.Status.ToString(),
        ["locale"] = document.Locale,
        ["canonicalLocale"] = document.CanonicalLocale,
        ["title"] = document.Title,
        ["body"] = document.Body,
        ["summary"] = document.Summary,
        ["effectiveDate"] = document.EffectiveDate?.ToString("yyyy-MM-dd"),
        ["approvedBy"] = document.ApprovedBy,
        ["approvedAtUtc"] = document.ApprovedAtUtc,
        ["owner"] = document.Owner,
        ["previousVersionId"] = document.PreviousVersionId,
        ["canonicalUrl"] = document.CanonicalUrl,
        ["requiredAcceptanceContexts"] = document.RequiredAcceptanceContexts.ToArray(),
        ["relatedSiteSurfaces"] = document.RelatedSiteSurfaces.ToArray(),
        ["evidencePath"] = document.EvidencePath
    };

    private static LegalDocument FromDictionary(IReadOnlyDictionary<string, object?> body) => new(
        ReadString(body, "id"),
        ReadString(body, "slug"),
        ReadString(body, "documentType"),
        ReadString(body, "version"),
        Enum.TryParse<LegalDocumentStatus>(ReadString(body, "status"), out var status) ? status : LegalDocumentStatus.LegalReviewRequired,
        ReadString(body, "locale"),
        ReadString(body, "canonicalLocale"),
        ReadString(body, "title"),
        ReadString(body, "body"),
        ReadString(body, "summary"),
        DateOnly.TryParse(ReadString(body, "effectiveDate"), out var effectiveDate) ? effectiveDate : null,
        ReadOptionalString(body, "approvedBy"),
        body.TryGetValue("approvedAtUtc", out var approvedAt) && approvedAt is DateTimeOffset approvedAtUtc ? approvedAtUtc : null,
        ReadString(body, "owner"),
        ReadOptionalString(body, "previousVersionId"),
        ReadString(body, "canonicalUrl"),
        ReadStringList(body, "requiredAcceptanceContexts"),
        ReadStringList(body, "relatedSiteSurfaces"),
        ReadString(body, "evidencePath"));

    private static string ReadString(IReadOnlyDictionary<string, object?> body, string key) =>
        body.TryGetValue(key, out var value) ? value?.ToString() ?? string.Empty : string.Empty;

    private static string? ReadOptionalString(IReadOnlyDictionary<string, object?> body, string key) =>
        body.TryGetValue(key, out var value) ? value?.ToString() : null;

    private static IReadOnlyList<string> ReadStringList(IReadOnlyDictionary<string, object?> body, string key) =>
        body.TryGetValue(key, out var value) && value is IEnumerable<string> strings
            ? strings.ToArray()
            : Array.Empty<string>();

    private sealed record LegalRouteTemplate(
        string DocumentType,
        string Title,
        string Summary,
        IReadOnlyList<string> RequiredAcceptanceContexts,
        IReadOnlyList<string> RelatedSiteSurfaces);
}
