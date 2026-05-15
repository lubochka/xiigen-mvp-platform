# XIIGen .NET/Angular Platform

This repository contains the shared .NET 10 platform primitives used by translated XIIGen flows.

## Current Scope

- `Xiigen.Kernel`: `DataProcessResult<T>`, tenant/correlation context, `MicroserviceBase<TService>`, and fabric contracts.
- `docker-compose.platform.yml`: local platform dependency stack for later flows.
- `scripts/pre-commit-check.ps1`: restore, build, and test gate.

## Verify

```powershell
powershell -ExecutionPolicy Bypass -File scripts\pre-commit-check.ps1
```
