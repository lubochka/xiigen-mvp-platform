using Xiigen.Fabrics.Scheduler;

namespace Xiigen.Fabrics.Scheduler.Tests;

public sealed class SchedulerTests
{
    [Fact]
    public async Task ScheduleDelayedAsync_ReplaysSameIdempotencyKey()
    {
        var scheduler = new InMemorySchedulerService();

        var first = await scheduler.ScheduleDelayedAsync("token-expiry", TimeSpan.FromHours(24), new Dictionary<string, object?>(), "job-1");
        var second = await scheduler.ScheduleDelayedAsync("token-expiry", TimeSpan.FromHours(24), new Dictionary<string, object?>(), "job-1");

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.True(second.FromCache);
        Assert.Equal(first.Data!.JobId, second.Data!.JobId);
    }
}
