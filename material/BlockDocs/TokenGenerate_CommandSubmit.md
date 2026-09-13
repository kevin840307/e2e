# TokenGenerate + CommandSubmit Composite Workflow

This is one E2E target, not two independent block tests.

Required production workflow order:

```text
TokenGenerateBlock.Execute
  -> WorkflowContext["Command.Token"]
  -> CommandSubmitBlock.Execute
  -> MQ publish
```

Dependency evidence:
- `TokenGenerateBlock.Execute` creates and stores a token, then writes it to `WorkflowContext["Command.Token"]`.
- `CommandSubmitBlock.Execute` requires that exact context value and validates it against `COMMAND_TOKEN`.
- A valid command is persisted to `COMMAND_AUDIT` and then published to the external MQ boundary.
- MQ ACK produces `CommandSubmit.Result=SUBMITTED`; NACK produces `CommandSubmit.Result=MQ_FAILED`.

E2E rule:
- Generate only `TokenGenerate_CommandSubmit/TokenGenerate_CommandSubmit.vb`.
- Call only `CallMain("TokenGenerate_CommandSubmit")`.
- Do not create independent TokenGenerate or CommandSubmit E2E folders/tests.
- `Workflow/Create SOP.sql`, `Create Condition.sql`, `Create Action.sql`, and `Validation.sql` must describe the combined two-block workflow in the exact order above.
- MQ is an external boundary and may use an owning SOP fixture such as `mock_mq_response.json`.
- Minimal MQ fixture format: `{"ack": true}` or `{"ack": false}`.
- `COMMAND_TOKEN` and `COMMAND_AUDIT` are production outputs for the normal success path and must not be pre-seeded by `prepare.sql`.


Meaningful E2E behaviors:
1. valid correlation + MQ ACK -> token is produced, command is persisted, MQ is called, result is `SUBMITTED`;
2. valid correlation + MQ NACK -> token and command are produced by production code, MQ is called, result is `MQ_FAILED`;
3. correlation id `INVALID` -> TokenGenerate still executes but returns an empty token, CommandSubmit executes the invalid-token path and does not publish MQ.

These are different behaviors, not data-only duplicates.
