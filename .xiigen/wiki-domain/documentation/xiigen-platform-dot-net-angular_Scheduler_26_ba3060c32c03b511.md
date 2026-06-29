# xiigen-platform-dot-net-angular::InMemorySchedulerService.ScheduleDelayedAsync

## Meaning
xiigen-platform-dot-net-angular::InMemorySchedulerService.ScheduleDelayedAsync implements local project behavior with return type Task<DataProcessResult<ScheduledJobResult>>. It has no detected internal callee. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Fabrics.Scheduler.Tests\SchedulerTests.cs:8::xiigen-platform-dot-net-angular::SchedulerTests.ScheduleDelayedAsync_ReplaysSameIdempotencyKey.

## Location
src\Xiigen.Fabrics.Scheduler\Scheduler.cs:26

## Parameters
- string action
- TimeSpan delay
- IReadOnlyDictionary<string
- object?> payload
- string idempotencyKey
- CancellationToken cancellationToken

## Return
Task<DataProcessResult<ScheduledJobResult>> returned by xiigen-platform-dot-net-angular::InMemorySchedulerService.ScheduleDelayedAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Fabrics.Scheduler.Tests\SchedulerTests.cs:8::xiigen-platform-dot-net-angular::SchedulerTests.ScheduleDelayedAsync_ReplaysSameIdempotencyKey

## Calls
- none recorded

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.Scheduler\Scheduler.cs:26::xiigen-platform-dot-net-angular::InMemorySchedulerService.ScheduleDelayedAsync