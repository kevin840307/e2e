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

## Target-owned workflow SQL

Each E2E target owns:

```text
<TARGET>/Workflow/
  Create SOP.sql
  Create Condition.sql
  Create Action.sql
  Validation.sql
```

This allows composite workflows to have different workflow-definition SQL from single-block workflows.

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
