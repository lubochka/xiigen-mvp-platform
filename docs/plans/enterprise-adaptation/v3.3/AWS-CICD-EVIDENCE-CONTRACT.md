# AWS CI/CD Evidence Contract — Enterprise v3.2

This contract is produced by FLOW-19 evidence work. It does not replace FLOW-19.

Required JSON fields:

```json
{
  "flow": "FLOW-19",
  "provider": "aws",
  "workflowRunUrl": "",
  "workflowConclusion": "",
  "commitSha": "",
  "region": "",
  "environment": "",
  "ecrImageDigest": "",
  "packageHash": "",
  "hostedBaseUrl": "",
  "serverHealthUrl": "",
  "clientUrl": "",
  "route53Record": "",
  "cloudFrontDomain": "",
  "albDnsName": "",
  "resolvedPublicIps": [],
  "s3ArtifactBucket": "",
  "secretsManagerUsed": true,
  "kmsOrSignerUsed": true,
  "cloudWatchLogGroup": "",
  "rollbackTarget": "",
  "complianceAuditEventId": ""
}
```

Rows closed by this contract: CI-AWS-01..13 and AWS-RUNTIME-01..06 only after runtime checks confirm the emitted endpoint.
