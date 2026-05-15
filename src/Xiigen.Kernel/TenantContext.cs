namespace Xiigen.Kernel;

public sealed record TenantContext(
    string TenantId,
    string? UserId,
    IReadOnlySet<string> Roles,
    string CorrelationId,
    string Source,
    DateTimeOffset CreatedAt);

public sealed record TenantContextSnapshot(
    string TenantId,
    string? UserId,
    IReadOnlySet<string> Roles,
    string CorrelationId,
    string Source,
    DateTimeOffset CapturedAt,
    IReadOnlyDictionary<string, string> Claims);

public interface ITenantContextAccessor
{
    TenantContext? Current { get; }
    IDisposable Push(TenantContext context);
}

public sealed class AsyncLocalTenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<TenantContext?> CurrentContext = new();

    public TenantContext? Current => CurrentContext.Value;

    public IDisposable Push(TenantContext context)
    {
        var prior = CurrentContext.Value;
        CurrentContext.Value = context;
        return new Scope(() => CurrentContext.Value = prior);
    }

    private sealed class Scope : IDisposable
    {
        private readonly Action _dispose;
        private bool _disposed;

        public Scope(Action dispose) => _dispose = dispose;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _dispose();
            _disposed = true;
        }
    }
}

public interface ITenantContextRunner
{
    Task<DataProcessResult> RunAsync(
        TenantContextSnapshot snapshot,
        Func<CancellationToken, Task<DataProcessResult>> work,
        CancellationToken cancellationToken = default);
}

public sealed class TenantContextRunner : ITenantContextRunner
{
    private readonly ITenantContextAccessor _accessor;

    public TenantContextRunner(ITenantContextAccessor accessor) => _accessor = accessor;

    public async Task<DataProcessResult> RunAsync(
        TenantContextSnapshot snapshot,
        Func<CancellationToken, Task<DataProcessResult>> work,
        CancellationToken cancellationToken = default)
    {
        var context = new TenantContext(
            snapshot.TenantId,
            snapshot.UserId,
            snapshot.Roles,
            snapshot.CorrelationId,
            snapshot.Source,
            DateTimeOffset.UtcNow);

        using var _ = _accessor.Push(context);
        return await work(cancellationToken).ConfigureAwait(false);
    }
}
