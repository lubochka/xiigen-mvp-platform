# XIIGen Enterprise Export Install Runbook

1. Verify `manifest.json`, `provenance.json`, `hashes.json`, and `signature/signature.json`.
2. Verify the package SHA-256 in `provenance.json` against the downloaded zip.
3. Verify `signature/signature.json` using `signature/public-key.json`.
4. Scan the package for secrets before install.
5. Import `trainable-units/registry/trainable-unit-registry.json` and `plugin-locators/plugin-unit-locator.json`.
6. Import `project-local-wiki/linked-system-graph.json` as the project-local wiki authority.
7. Apply AWS/runtime config from `config/` using environment-specific secret IDs only.
8. Run corporate install health checks and persist install evidence.
