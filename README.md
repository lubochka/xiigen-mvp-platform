# XIIGen — Self-Building AI Code Generation Engine

## Shared .NET platform

XIIGen is a fabric-first AI code generation engine for agentic workflows. It turns business intent into application flows through planning, generation, orchestration, retrieval and review. This is the .NET and Angular microservices implementation of the same product developed in [xiigen-mvp](https://github.com/lubochka/xiigen-mvp).

This repository supplies the shared .NET foundation. Business microservices and Angular applications live in [XIIGen Flows](https://github.com/lubochka/xiigen-mvp-flows). Together, the repositories keep reusable infrastructure contracts separate from individual business flows.

## What the platform provides

| Component | Responsibility |
| --- | --- |
| [Xiigen.Kernel](src/Xiigen.Kernel) | Result types, tenant and correlation context, service base class and fabric contracts |
| [Xiigen.Fabrics.InMemory](src/Xiigen.Fabrics.InMemory) | In-memory infrastructure implementations |
| [Xiigen.Fabrics.Scheduler](src/Xiigen.Fabrics.Scheduler) | Scheduling infrastructure |
| [Xiigen.Auth](src/Xiigen.Auth) | Authentication contracts and password-hashing support |
| [Xiigen.Freedom](src/Xiigen.Freedom) | Configuration-driven application behavior |
| [Xiigen.Platform.Marketplace](src/Xiigen.Platform.Marketplace) | Marketplace listings, package readback, tenant forks and adaptation history |
| [Xiigen.Platform.Api](src/Xiigen.Platform.Api) | Shared platform HTTP endpoints |

The kernel expresses the architecture in .NET terms: `DataProcessResult<T>` carries an operation's result, `MicroserviceBase<TService>` supplies service context, and `IDocumentStore`, `IIdempotencyStore` and `IOutboxStore` define shared infrastructure boundaries. A flow consumes these contracts rather than duplicating the platform inside its own service.

## How it fits together

```text
Business intent and developer decisions
                 |
XIIGen planning, generation, retrieval and review
                 |
.NET business microservices + Angular clients  (flows repository)
                 |
Kernel, fabrics, context, marketplace and APIs (this repository)
```

The product combines flow generation and orchestration, DAG-based workflow descriptions, RAG-assisted reasoning and tenant-aware execution. The flows repository contains domain applications, the meta-flow engine, arbitration, RAG feedback and platform-agent workflows. This platform supplies their common vocabulary and infrastructure boundaries.

Agent guidance is kept in repository files so that Claude, Codex, Cursor and other compatible coding tools can work from the same project context. The human owns the goal and product decisions; agents work within that scope.

## Stack and getting started

The platform uses C#, .NET 10, ASP.NET Core and NuGet packages. [global.json](global.json) selects SDK **10.0.300**, with the configured feature-band roll-forward policy. [Directory.Build.props](Directory.Build.props) contains the shared build settings.

From the repository root, restore and build the platform solution:

```sh
dotnet restore Xiigen.Platform.slnx --locked-mode
dotnet build Xiigen.Platform.slnx -c Release --no-restore
```

The existing [PowerShell check script](scripts/pre-commit-check.ps1) documents the repository's restore, build and test sequence. The [Compose file](docker-compose.platform.yml) describes the local dependency stack. Select the services needed by your application and keep credentials outside committed files.

For a client application, continue in the [flows repository](https://github.com/lubochka/xiigen-flows-dot-net-angular), starting with FLOW-00 Bundle Activation. That repository consumes versioned platform packages; coordinate shared contract changes with their consumers.

## Repository and documentation map

- [src/](src) — platform libraries and API.
- [tests/](tests) — platform test sources.
- [Xiigen.Platform.slnx](Xiigen.Platform.slnx) — solution entry point.
- [Fabric contracts](src/Xiigen.Kernel/FabricContracts.cs), [service base](src/Xiigen.Kernel/MicroserviceBase.cs) and [tenant context](src/Xiigen.Kernel/TenantContext.cs) — the shared architecture in code.
- [Skill usage guide](docs/HOW-TO-USE-SKILLS.md) and [skills](docs/skills) — contributor and coding-agent guidance.
- [FAQ](docs/FAQ.md) — ten product questions about verification, architecture, algorithm design, learning, agents and developer ownership.

## Contributing

Read the [contributor guide](CONTRIBUTING.md), [support guidance](SUPPORT.md) and [security policy](SECURITY.md).

Use [issues](https://github.com/lubochka/xiigen-mvp-platform) for bugs, documentation corrections and proposed improvements. Explain the affected component and intended result. Shared infrastructure changes belong here; flow-specific behavior belongs in the companion repository. Keep examples synthetic and exclude credentials and private tenant data.

## License and related projects

XIIGen's first-party licensing is GNU Affero General Public License version 3 only (`AGPL-3.0-only`). Third-party dependencies retain their own licenses. See [LICENSE](LICENSE), [NOTICE](NOTICE) and [third-party notices](THIRD_PARTY_NOTICES.md).

- [XIIGen Flows for .NET and Angular](https://github.com/lubochka/xiigen-mvp-flows) — business microservices and clients built on this platform.
- [xiigen-mvp](https://github.com/lubochka/xiigen-mvp) — the parallel XIIGen implementation and product reference.

The engine consumes models and can produce learning signals; model-weight training belongs to the separate XIIGen model core. See the [FAQ](docs/FAQ.md).
