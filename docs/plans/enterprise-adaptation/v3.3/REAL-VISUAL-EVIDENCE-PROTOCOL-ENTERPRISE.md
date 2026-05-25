# Real Visual Evidence Protocol — Enterprise v3.2

A visual evidence artifact is valid only if it contains:

- UTC timestamp;
- tested URL;
- DNS-resolved public IPs at test time;
- AWS region/environment;
- tenant and role context with secrets redacted;
- screenshot path;
- Playwright trace/video path when applicable;
- matrix row mapping;
- source command that produced it.

Invalid evidence:

- localhost;
- Angular dev server;
- mock endpoint;
- direct helper call;
- contract harness only;
- screenshot without URL/IP metadata;
- build/readiness report without runtime UI.

Visual evidence index schema:

```json
{
  "hostedBaseUrl": "",
  "resolvedPublicIps": [],
  "capturedAtUtc": "",
  "artifacts": [
    {
      "matrixRow": "AWS-RUNTIME-07",
      "url": "",
      "tenant": "",
      "role": "",
      "screenshot": "",
      "trace": "",
      "video": "",
      "secretsRedacted": true
    }
  ]
}
```
