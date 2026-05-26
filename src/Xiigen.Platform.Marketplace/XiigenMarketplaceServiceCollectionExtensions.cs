using Microsoft.Extensions.DependencyInjection;

namespace Xiigen.Platform.Marketplace;

public static class XiigenMarketplaceServiceCollectionExtensions
{
    public static IServiceCollection AddXiigenMarketplace(this IServiceCollection services)
    {
        services.AddSingleton<IMarketplaceService, MarketplaceService>();
        services.AddSingleton<ILegalPublicationService, LegalPublicationService>();
        services.AddSingleton<ITenantAdaptationStudioService, TenantAdaptationStudioService>();
        services.AddSingleton<ISmartWikiService, SmartWikiService>();
        services.AddSingleton<ISmartCodeAssistantService, SmartCodeAssistantService>();
        return services;
    }
}
