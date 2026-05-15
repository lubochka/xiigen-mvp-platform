using Microsoft.Extensions.DependencyInjection;
using Xiigen.Kernel;

namespace Xiigen.Freedom;

public sealed record FreedomConfigValue(
    string Key,
    object? Value,
    string ValueType,
    string Scope,
    string? TenantId,
    string Environment,
    long Version);

public interface IFreedomConfigService
{
    Task<DataProcessResult<FreedomConfigValue?>> GetAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult> SetAsync(
        string key,
        object? value,
        string valueType,
        CancellationToken cancellationToken = default);
}

public sealed class InMemoryFreedomConfigService : IFreedomConfigService
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, FreedomConfigValue> _values = new(StringComparer.Ordinal);

    public InMemoryFreedomConfigService()
    {
        Seed("flow01_resend_rate_limit_minutes", 60, "number");
        Seed("flow01_email_verification_ttl_seconds", 86_400, "number");
        Seed("flow01_invitation_inviter_name", "The Team", "string");
        Seed("flow01_invitation_community_name", "Community", "string");
        Seed("flow_module_name", "user-registration", "string");
    }

    public Task<DataProcessResult<FreedomConfigValue?>> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _values.TryGetValue(key, out var value);
            return Task.FromResult(DataProcessResult<FreedomConfigValue?>.Success(value));
        }
    }

    public Task<DataProcessResult> SetAsync(
        string key,
        object? value,
        string valueType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.FromResult(DataProcessResult.Failure("VALIDATION_FAILED", "FREEDOM key is required"));
        }

        lock (_gate)
        {
            var version = _values.TryGetValue(key, out var existing) ? existing.Version + 1 : 1;
            _values[key] = new FreedomConfigValue(key, value, valueType, "platform-default", null, "local", version);
            return Task.FromResult(DataProcessResult.Success());
        }
    }

    private void Seed(string key, object value, string valueType) =>
        _values[key] = new FreedomConfigValue(key, value, valueType, "platform-default", null, "local", 1);
}

public static class XiigenFreedomConfigServiceCollectionExtensions
{
    public static IServiceCollection AddXiigenFreedomConfig(this IServiceCollection services)
    {
        services.AddSingleton<IFreedomConfigService, InMemoryFreedomConfigService>();
        return services;
    }
}
