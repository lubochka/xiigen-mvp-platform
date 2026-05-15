using Microsoft.Extensions.DependencyInjection;
using Xiigen.Kernel;

namespace Xiigen.Fabrics.Scheduler;

public sealed record ScheduledJobResult(string JobId, string Action, DateTimeOffset DueAt);

public interface ISchedulerService
{
    Task<DataProcessResult<ScheduledJobResult>> ScheduleDelayedAsync(
        string action,
        TimeSpan delay,
        IReadOnlyDictionary<string, object?> payload,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult<IReadOnlyList<ScheduledJobResult>>> ListAsync(
        CancellationToken cancellationToken = default);
}

public sealed class InMemorySchedulerService : ISchedulerService
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, ScheduledJobResult> _jobs = new(StringComparer.Ordinal);

    public Task<DataProcessResult<ScheduledJobResult>> ScheduleDelayedAsync(
        string action,
        TimeSpan delay,
        IReadOnlyDictionary<string, object?> payload,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(action) || string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return Task.FromResult(DataProcessResult<ScheduledJobResult>.Failure(
                "VALIDATION_FAILED",
                "Action and idempotency key are required"));
        }

        lock (_gate)
        {
            if (_jobs.TryGetValue(idempotencyKey, out var existing))
            {
                return Task.FromResult(DataProcessResult<ScheduledJobResult>.Success(existing, fromCache: true));
            }

            var job = new ScheduledJobResult(idempotencyKey, action, DateTimeOffset.UtcNow.Add(delay));
            _jobs[idempotencyKey] = job;
            return Task.FromResult(DataProcessResult<ScheduledJobResult>.Success(job));
        }
    }

    public Task<DataProcessResult<IReadOnlyList<ScheduledJobResult>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(DataProcessResult<IReadOnlyList<ScheduledJobResult>>.Success(_jobs.Values.ToArray()));
        }
    }
}

public static class XiigenSchedulerServiceCollectionExtensions
{
    public static IServiceCollection AddXiigenInMemoryScheduler(this IServiceCollection services)
    {
        services.AddSingleton<ISchedulerService, InMemorySchedulerService>();
        return services;
    }
}
