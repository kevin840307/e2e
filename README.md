# E2E Regression Sample Project

This project demonstrates:

- single-block E2E workflows;
- composite/non-independent workflows;
- external API/HTTP mock fixtures;
- external MQ mock fixtures;
- extensible Kafka/NATS/gRPC/WSDL fixture registration;
- DB before-state via parameterized `prepare.sql`;
- coverage hard gates driven by `config/function_mapping.json`.

## Composite example

`TokenGenerate_CommandSubmit` is one E2E target:

```text
TokenGenerate -> CommandSubmit -> MQ
```

The mapping validates both blocks' entry/critical functions together. It must not be split into independent TokenGenerate and CommandSubmit E2E targets.

## Workflow / SOP templates

Single-block SOP topology only has two common forms: `Action` or `Condition`. Reusable creation SQL belongs under `Global/Workflow/` and should be reused across targets.

Case-specific differences belong only in each SOP folder:

```text
<TARGET>-SOP-NNN/
  prepare.sql
  mock_<external>.*   # optional
```

Only a real non-independent composite may define target-specific workflow SQL when the Global templates cannot represent its topology. AI must never invent random block chains for coverage.

## External mock fixtures

Fixtures live inside the owning SOP folder:

```text
mock_api_response.json
mock_mq_response.json
mock_kafka_event.json
mock_nats_event.json
mock_grpc_response.json
mock_wsdl_response.xml
```

Tests register them through TestInfrastructure helpers, then call the real Main/Workflow. Internal DB/Repository/Business/Block/Workflow/Main are never mocked.
