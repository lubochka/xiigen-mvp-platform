using Xiigen.Kernel;

namespace Xiigen.Platform.Api.DesignConversion;

public sealed record DesignConversionTierContext(string DesignTier, int LoopBudget);

public static class DesignConversionTierPolicy
{
    private static readonly IReadOnlyDictionary<string, int> MaxLoopBudgetByTier =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["starter"] = 2,
            ["pro"] = 8,
            ["enterprise"] = 20
        };

    public static DataProcessResult<DesignConversionTierContext> Validate(
        HttpContext context,
        DesignConversionSubmitRequest request)
    {
        var designTier = ReadHeader(context, DesignConversionHeaderNames.DesignTier) ?? request.DesignTier;
        if (string.IsNullOrWhiteSpace(designTier))
        {
            return DataProcessResult<DesignConversionTierContext>.Failure(
                "DESIGN_TIER_REQUIRED",
                "X-Design-Tier or designTier is required.");
        }

        if (!MaxLoopBudgetByTier.TryGetValue(designTier, out var maxLoopBudget))
        {
            return DataProcessResult<DesignConversionTierContext>.Failure(
                "DESIGN_TIER_UNSUPPORTED",
                "X-Design-Tier must be starter, pro, or enterprise.");
        }

        var loopBudgetText = ReadHeader(context, DesignConversionHeaderNames.LoopBudget);
        var hasLoopBudget = int.TryParse(loopBudgetText, out var loopBudget);
        loopBudget = hasLoopBudget ? loopBudget : request.LoopBudget.GetValueOrDefault();
        if (loopBudget < 1)
        {
            return DataProcessResult<DesignConversionTierContext>.Failure(
                "LOOP_BUDGET_REQUIRED",
                "X-Loop-Budget or loopBudget must be a positive integer.");
        }

        // TODO: Replace this local ceiling with tenant-specific entitlement and quota policy.
        if (loopBudget > maxLoopBudget)
        {
            return DataProcessResult<DesignConversionTierContext>.Failure(
                "LOOP_BUDGET_DENIED",
                $"Loop budget {loopBudget} exceeds the {designTier} tier limit of {maxLoopBudget}.");
        }

        return DataProcessResult<DesignConversionTierContext>.Success(new DesignConversionTierContext(
            designTier,
            loopBudget));
    }

    private static string? ReadHeader(HttpContext context, string name) =>
        context.Request.Headers.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : null;
}
