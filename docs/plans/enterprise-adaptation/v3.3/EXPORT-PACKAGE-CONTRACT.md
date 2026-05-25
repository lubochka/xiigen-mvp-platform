# Export Package Contract — Enterprise v3.2

Required package tree:

```text
package-root/
  manifest.json
  provenance.json
  hashes.json
  signature/
  app/
  config/
  trainable-units/
  project-local-wiki/
  plugin-locators/
  evidence/
  install/
```

Required proofs:

- package manifest includes tenant/project/version/source commit;
- package hash is recorded;
- package is signed or signing gap is explicit;
- no-secrets scan exists;
- trainable unit manifests are included;
- plugin locators are included;
- evidence map is included;
- package download authorization is tenant-scoped.
