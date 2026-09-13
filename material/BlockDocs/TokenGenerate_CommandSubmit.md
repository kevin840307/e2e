# TokenGenerate + CommandSubmit Composite Workflow

This is one E2E target, not two independent block tests.

Required production workflow order:

```text
TokenGenerateBlock.Execute
  -> CommandService.GenerateToken
  -> CommandRepository.QueryTokenPolicy
  -> CommandRepository.SaveToken
  -> WorkflowContext["Command.Token"]
  -> CommandSubmitBlock.Execute
  -> CommandService.ValidateToken
  -> CommandRepository.TokenExists
  -> CommandService.ValidateCommand
  -> CommandRepository.QueryCommandRule
  -> CommandService.ResolveMqTopic
  -> CommandRepository.QueryMqEndpoint
  -> MQ publish
```

Dependency evidence:
- `TokenGenerateBlock.Execute` creates and stores a token, then writes it to `WorkflowContext["Command.Token"]`.
- `CommandSubmitBlock.Execute` requires that exact context value and validates it against `COMMAND_TOKEN`.
- A valid command is persisted to `COMMAND_AUDIT` and then published to the external MQ boundary.
- MQ ACK produces `CommandSubmit.Result=SUBMITTED`; NACK produces `CommandSubmit.Result=MQ_FAILED`.

E2E rule:
- Generate only `TokenGenerate_CommandSubmit/TokenGenerate_CommandSubmit.vb`.
- Call only `CallMain()`; Main must read the ready control/status row to select `TokenGenerate_CommandSubmit` and its SOP.
- Do not create independent TokenGenerate or CommandSubmit E2E folders/tests.
- This composite must follow the exact mapping order above. Reuse Global Workflow/SOP templates when possible; only add target-specific workflow SQL if the shared templates cannot represent this dependency.
- MQ is an external boundary and may use an owning SOP fixture such as `mock_mq_response.json`.
- Minimal MQ fixture format: `{"ack": true}` or `{"ack": false}`.
- `COMMAND_TOKEN` and `COMMAND_AUDIT` are production outputs for the normal success path and must not be pre-seeded by `prepare.sql`.
- This composite may still require fixture SQL for both blocks' parameters and lookup dependencies. Do not treat "do not pre-seed output tables" as "no SQL is needed."
- Prepare before-state for TokenGenerate, CommandSubmit, and their dependency handoff independently:
  - `E2E_SOP_CONTROL` SOP/control status row that makes Main pick this target/SOP;
  - `COMMAND_TOKEN_POLICY` rows in the parameter DB used by TokenGenerate before token creation;
  - `COMMAND_RULE` rows in the parameter DB used by CommandSubmit to validate whether the command may be submitted;
  - `MQ_ENDPOINT` rows in the master/config DB used by CommandSubmit to resolve the MQ topic;
  - any extra tables queried by repositories/services while driving the case to the intended branch.
- If those dependencies live in different DB roots/schemas, split them into multiple `prepare*.sql` files such as `prepare.sql`, `prepare.params.sql`, and `prepare.command-db.sql`, and execute each from the TestMethod before `CallMain()`.
- Every fixture SQL file must use the safe fixture rules: existing control rows may only update `STATUS`; all other rows are insert-if-not-exists by an explicit key; no delete, broad update, merge, or output pre-seeding.


Meaningful E2E behaviors:
1. valid correlation + MQ ACK -> token is produced, command is persisted, MQ is called, result is `SUBMITTED`;
2. valid correlation + MQ NACK -> token and command are produced by production code, MQ is called, result is `MQ_FAILED`;
3. correlation id `INVALID` -> TokenGenerate still executes but returns an empty token, CommandSubmit executes the invalid-token path and does not publish MQ.

These are different behaviors, not data-only duplicates.
