# AI Grill + Review Rules

Review only. Do not modify files.

Treat the current `--block` value as an **E2E target**, not necessarily one production Block. Read its `workflow_blocks` from `function_mapping.json`.

Verify:
- meaningful behaviors are covered with the smallest sufficient SOP set;
- for composite targets, all mapped `workflow_blocks` run in one Main/Workflow with one shared WorkflowContext and correct order; they are not split into independent E2E tests;
- `<TARGET>/Workflow/Create SOP.sql`, `Create Condition.sql`, `Create Action.sql`, `Validation.sql` exist and describe this target only;
- composite Workflow SQL contains all blocks in correct order and Validation.sql evaluates the whole workflow post-state;
- Global was not used for target-specific workflow SQL;
- one SOP folder <-> one TestMethod;
- every SOP is independent from other SOPs;
- `prepare.sql` is INSERT-only before-state and does not manufacture result/audit/history/final state;
- `mock_<external>.*` exists only for a real external boundary and is consumed from its owning SOP;
- API/HTTP, MQ, Kafka, NATS, WSDL/SOAP, gRPC or third-party boundaries may be mocked;
- DB/Repository/Business/Block/Workflow/Main/Entry/Critical are never mocked;
- test flow is `[optional external mock] -> own prepare.sql -> CallMain(current E2E target) -> Assert`;
- no direct Block.Execute/Business/Repository call is used to manufacture coverage;
- TestMethod has meaningful Chinese DisplayName;
- Python validation, build, test and current TRX evidence are successful;
- every mapped `entry_functions` and `critical_functions` symbol resolves uniquely and is > configured `min_coverage`;
- for composite targets, coverage is evaluated together in the same validation run, not one block at a time.

Report concrete issues with target/SOP/TestMethod/mock/function evidence and required repair. Do not make the runner's final pass/fail decision.
