using Microsoft.Extensions.DependencyInjection;
using Xiigen.Kernel;

namespace Xiigen.Fabrics.InMemory;

public static class XiigenInMemoryFabricServiceCollectionExtensions
{
    public static IServiceCollection AddXiigenInMemoryFabrics(this IServiceCollection services)
    {
        services.AddSingleton<ITenantContextAccessor, AsyncLocalTenantContextAccessor>();
        services.AddSingleton<ICorrelationContextAccessor, AsyncLocalCorrelationContextAccessor>();
        services.AddSingleton<IDocumentStore, InMemoryDocumentStore>();
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
        services.AddSingleton<IOutboxStore, InMemoryOutboxStore>();
        services.AddSingleton<IFlowBootstrapper, InMemoryFlowBootstrapper>();
        return services;
    }
}
