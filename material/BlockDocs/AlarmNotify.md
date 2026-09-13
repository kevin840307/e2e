# AlarmNotify

`AlarmNotifyBlock.Execute` resolves alarm channel configuration, applies a message template, then calls the external HTTP/API boundary through `FakeHttpClient.Post`.

Internal flow:

```text
AlarmNotifyBlock.Execute
  -> ResolveChannel
  -> ApplyChannel -> ALARM_CHANNEL
  -> ApplyTemplate -> ALARM_TEMPLATE
  -> FakeHttpClient.Post
```

E2E may configure an API fixture in its owning SOP, for example:

```text
mock_api_response.json
```

Example payload:

```json
{"statusCode": 500}
```

The test should call `UseApiMock(...)`, prepare `E2E_SOP_CONTROL` plus any required `ALARM_CHANNEL` / `ALARM_TEMPLATE` rows, then `CallMain()`, and assert the observable workflow context result. Do not mock Workflow/Block/Main.


Useful cases:
- `{"statusCode": 200}` -> workflow observes HTTP 200;
- `{"statusCode": 500}` -> workflow observes HTTP 500.

Fixture implications:
- `ALARM_CHANNEL` and `ALARM_TEMPLATE` may live in `E2E_CONFIG_DB_ROOT`.
- SQL may be split as `prepare.sql` for SOP/control and `prepare.config.sql` for channel/template data.
