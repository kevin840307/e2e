# AlarmNotify

`AlarmNotifyBlock.Execute` calls the external HTTP/API boundary through `FakeHttpClient.Post`.

E2E may configure an API fixture in its owning SOP, for example:

```text
mock_api_response.json
```

Example payload:

```json
{"statusCode": 500}
```

The test should call `UseApiMock(...)`, then `CallMain("AlarmNotify")`, and assert the observable workflow context result. Do not mock Workflow/Block/Main.


Useful cases:
- `{"statusCode": 200}` -> workflow observes HTTP 200;
- `{"statusCode": 500}` -> workflow observes HTTP 500.
