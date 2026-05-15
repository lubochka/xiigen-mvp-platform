using Microsoft.Extensions.DependencyInjection;

namespace Xiigen.Platform.Marketplace;

public static class XiigenMarketplaceServiceCollectionExtensions
{
    public static IServiceCollection AddXiigenMarketplace(this IServiceCollection services)
    {
        services.AddSingleton<IMarketplaceService, MarketplaceService>();
        return services;
    }
}
