# XIIGen .NET/Angular Platform

This repository contains the shared .NET 10 platform primitives used by translated XIIGen flows.

## Current Scope

- `Xiigen.Kernel`: `DataProcessResult<T>`, tenant/correlation context, `MicroserviceBase<TService>`, and fabric contracts.
- `Xiigen.Platform.Marketplace`: local marketplace listing, package readback, tenant fork provenance, and adaptation-history contracts for Layer 8 certification.
- `Xiigen.Platform.Api`: local platform API exposing health and marketplace evidence endpoints.
- `docker-compose.platform.yml`: local platform dependency stack for later flows.
- `scripts/pre-commit-check.ps1`: restore, build, and test gate.

## Verify

```powershell
powershell -ExecutionPolicy Bypass -File scripts\pre-commit-check.ps1
```
