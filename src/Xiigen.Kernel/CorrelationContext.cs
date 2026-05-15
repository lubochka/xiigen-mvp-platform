namespace Xiigen.Kernel;

public sealed record CorrelationContext(string CorrelationId, string Source, DateTimeOffset CreatedAt);

public interface ICorrelationContextAccessor
{
    CorrelationContext? Current { get; }
    IDisposable Push(CorrelationContext context);
}

public sealed class AsyncLocalCorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContext?> CurrentContext = new();

    public CorrelationContext? Current => CurrentContext.Value;

    public IDisposable Push(CorrelationContext context)
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
