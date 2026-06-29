# xiigen-platform-dot-net-angular::TenantContextRunner.RunAsync

## Meaning
xiigen-platform-dot-net-angular::TenantContextRunner.RunAsync implements local project behavior with return type Task<DataProcessResult>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Kernel\CorrelationContext.cs:17::xiigen-platform-dot-net-angular::AsyncLocalCorrelationContextAccessor.Push. No internal caller was detected.

## Location
src\Xiigen.Kernel\TenantContext.cs:73

## Parameters
- TenantContextSnapshot snapshot
- Func<CancellationToken
- Task<DataProcessResult>> work
- CancellationToken cancellationToken

## Return
Task<DataProcessResult> returned by xiigen-platform-dot-net-angular::TenantContextRunner.RunAsync

## Called By
- none recorded

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Kernel\CorrelationContext.cs:17::xiigen-platform-dot-net-angular::AsyncLocalCorrelationContextAccessor.Push

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Kernel\TenantContext.cs:73::xiigen-platform-dot-net-angular::TenantContextRunner.RunAsync