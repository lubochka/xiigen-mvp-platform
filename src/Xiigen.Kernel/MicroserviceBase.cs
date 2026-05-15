using Microsoft.Extensions.Logging;

namespace Xiigen.Kernel;

public sealed record ServiceDescriptor(string ServiceId, string ServiceName, string FlowId);

public abstract class MicroserviceBase<TService>
{
    protected MicroserviceBase(
        ServiceDescriptor descriptor,
        ITenantContextAccessor tenantContextAccessor,
        ICorrelationContextAccessor correlationContextAccessor,
        ILogger<TService> logger)
    {
        Descriptor = descriptor;
        TenantContextAccessor = tenantContextAccessor;
        CorrelationContextAccessor = correlationContextAccessor;
        Logger = logger;
    }

    public ServiceDescriptor Descriptor { get; }

    protected ITenantContextAccessor TenantContextAccessor { get; }
    protected ICorrelationContextAccessor CorrelationContextAccessor { get; }
    protected ILogger<TService> Logger { get; }

    protected TenantContext CurrentTenant => TenantContextAccessor.Current
        ?? throw new InvalidOperationException("Tenant context is required for XIIGen service execution.");

    protected string CurrentCorrelationId => CorrelationContextAccessor.Current?.CorrelationId
        ?? "correlation-unset";

    protected static DataProcessResult<T> Success<T>(T value) => DataProcessResult<T>.Success(value);

    protected static DataProcessResult<T> Failure<T>(
        string code,
        string message,
        IReadOnlyDictionary<string, object?>? details = null) =>
        DataProcessResult<T>.Failure(code, message, details);
}
