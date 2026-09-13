# Scheduler SOP

- Workflow engine executes blocks in configured order with one shared `WorkflowContext`.
- Most E2E targets contain one independent block.
- `TokenGenerate_CommandSubmit` is the deliberate dependency workflow and must execute:

```text
TokenGenerate -> CommandSubmit -> external MQ
```

- `TokenGenerate` and `CommandSubmit` are not standalone E2E targets.
- Every E2E SOP uses an isolated DB root and isolated external mock fixture registration.
- API/HTTP and MQ are external boundaries and may be fixture-backed; internal DB/Repository/Business/Block/Workflow/Main are never mocked.
