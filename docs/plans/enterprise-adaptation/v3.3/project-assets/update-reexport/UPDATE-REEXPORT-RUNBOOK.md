# Update/Re-export Lifecycle Runbook

1. Detect graph deltas from e993b1c to $head and classify Angular, .NET, SQL, evidence/package-only changes.
2. If Angular/.NET/SQL deltas exist, reindex affected wiki entries, retrain affected units, rerun prompt and plugin regressions, redeploy through FLOW-19, and capture fresh AWS visual evidence.
3. If no runtime graph delta exists, record unaffected prompt/plugin/wiki/unit evidence and preserve existing AWS visual evidence while refreshing endpoint health.
4. Re-export package as $packageName, write manifest/provenance/hash evidence, and sign the zip with an approved ephemeral signer.
5. Verify rollback to the previous package by preserving the v1.0.0 package path, evidence path, and corporate install validator path.
