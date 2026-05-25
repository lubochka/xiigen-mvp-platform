# Corporate Install Runbook

1. Verify `manifest.json` and tenant/project/version fields.
2. Verify package SHA-256 against `signature.packageSha256`.
3. Verify RSA signature with `public-key.json`.
4. Verify every `hashes.json` entry against package-root files.
5. Run high-confidence no-secrets scan.
6. Import trainable unit registry and plugin locator.
7. Resolve checkpoint references.
8. Run server/client/data health checks.
9. Verify Visual Studio and VS Code locator entries can find imported units.
10. Persist corporate install evidence and rollback plan.
