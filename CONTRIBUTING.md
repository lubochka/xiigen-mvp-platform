# Contributing to XIIGen

Start with the [README](README.md) and [FAQ](docs/FAQ.md). Propose small changes with a clear user or contributor outcome. Use [issues](https://github.com/lubochka/xiigen-platform-dot-net-angular/issues) to describe a bug, documentation correction or feature request.

## Where changes belong

This repository owns shared .NET contracts, fabrics, context and platform APIs. Business microservices and Angular clients belong in the companion flows repository. Coordinate changes to shared package contracts with their consumers. Preserve existing identifiers and behavior when translating documentation.

## Preparing a contribution

- Explain the problem, resulting behavior and affected paths.
- Follow the existing result, tenant-context and infrastructure boundaries.
- Use synthetic examples. Exclude credentials, personal data and private tenant material from source, logs and screenshots.
- Describe checks actually performed and their observed results. State what was not checked; do not copy an earlier success claim as fresh evidence. For documentation, read back the edited text and check its links.
- Keep relevant documentation with the change. Preserve third-party copyright notices and license terms, and identify the source of imported material.

Repository commands are documented in the [README](README.md) and existing scripts. Select checks appropriate to the change and the agreed task scope.

Submit only material you have the right to contribute under the repository's [license](LICENSE), while preserving applicable third-party terms. See [third-party notices](THIRD_PARTY_NOTICES.md).

Follow the [code of conduct](CODE_OF_CONDUCT.md). For vulnerabilities, use the initial contact procedure in [SECURITY.md](SECURITY.md) without posting sensitive details.
