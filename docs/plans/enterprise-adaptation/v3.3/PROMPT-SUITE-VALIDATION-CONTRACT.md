# Prompt Suite Validation Contract — Enterprise v3.2

The prompt suite cannot be validated by prompt class only. A generated matrix must include one row per prompt item.

Required row fields:

```json
{
  "promptId": "",
  "promptTextHash": "",
  "sourceFile": "orchestrator prompts.txt",
  "expectedUnitRoute": "",
  "expectedFanout": [],
  "expectedArbiterPath": "",
  "expectedReprocessingPath": "",
  "expectedAuthoritySource": "",
  "actualUnitRoute": "UNKNOWN",
  "actualResultEvidence": "",
  "accuracy": "UNKNOWN",
  "elapsedMs": "UNKNOWN",
  "cpu": "UNKNOWN",
  "ram": "UNKNOWN",
  "lossOrReward": "UNKNOWN",
  "status": "UNKNOWN"
}
```

Class/category summary can pass only after every generated prompt row is PASS or NOT_APPLICABLE with evidence.
